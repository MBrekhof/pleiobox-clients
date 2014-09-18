using System;
using LocalBox_Common.Remote.Authorization;
using Synchronization.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalBox_Common.Remote;
using System.IO;
using System.Linq;
using SQLite;
using System.Threading;
using System.Security.Cryptography;

namespace LocalBox_Common
{
    public class DataLayer
    {   
        private static DataLayer _instance = null;
        private Database database = null;
        private readonly string databasePath;
		public int loginAttempts;

        public static DataLayer Instance
        { 
            get
            {
                if (_instance == null)
                {
                    _instance = new DataLayer();
                }
                return _instance;
            }
        }

        DataLayer()
        {
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "localbox.db");
        }
            
        public Database DbInstance()
        {
            return database;
        }

        public bool DatabaseCreated()
        {
            return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "localbox.db"));
        }

        public bool DatabaseUnlocked()
        {
            return database != null;
        }


		public bool UnlockDatabase(string pin)
        {
            try
            {
				loginAttempts++;

				database = new Database(databasePath, pin);

				loginAttempts = 0;
				return true;
            }
			catch (SQLiteException)
            {
				if (loginAttempts >= 5) 
				{
					//5x foutieve pincode ingevoerd dus verwijder geregistreerde localboxen
					DeleteDatabase ();
				}

                if (database != null)
                    database.Dispose();
                database = null;
                return false;
            }
        }

		private async void DeleteDatabase()
		{
			if(File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "localbox.db"))){
				File.Delete (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "localbox.db"));
			}
		}


        public void LockDatabase()
        {
            if (database != null)
                database.Dispose();
            database = null;
        }

        public Task<bool> ValidatePincode(string pincode)
        {
            return Task.Run(() =>
            {
                Database db = null;
                try
                {
                    db = new Database(databasePath, pincode);
                }
                catch (SQLiteException)
                {
                    return false;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                return true;
            });
        }


		public Task<TreeNode> GetTreeNode(string path)
		{
			return Task.Run (() => {
				return database.GetTree (path);
			});
		}


        public Task<TreeNode> GetFolder(string path, bool refresh = false)
        {
            return Task.Run(() =>
            {
                if (refresh)
                {
                    RefreshFolder(path);
                }
                var result = database.GetTree(path);

                if (result != null && result.Children.Count == 0)
                {
                    RefreshFolder(path);
                    result = database.GetTree(path);
                    UpdateFolderKey(result);
                }
                else // root is null
                {
					

                    if(result == null && (path.Equals("/") || path.Equals(""))) 
                    {
                        int boxId = Waardes.Instance.GeselecteerdeBox;

                        var node = new TreeNode(){
                            Name = "Root",
                            Path = "/",
                            ParentId =  0,
                            IsDirectory = true,
                            LocalBoxId = boxId
                        };
                        database.AddNode(node);

						//Create unencrypted public folder in empty root directory
						var explorer = new RemoteExplorer();
						string nameOfUser = explorer.GetUser().Name;
						CreateFolder("/Publiek - " + nameOfUser, false);

                        RefreshFolder(node.Path);
                        result = database.GetTree(path);
					}

                }
                return result;
            });
        }

        private void UpdateFolderKey(TreeNode node) {
            // alleen keys ophalen als het nog niet eerder is gedaan en het op een map op het rootniveau is.
            if (!node.CheckedForKeys && node.Path.Count(e => e == '/') == 1 && node.Path.Length > 1)
            {
                var explorer = new RemoteExplorer();
                AesKeyResponse response;

                if (explorer.GetAesKey(node.Path, out response))
                {
                    if (response != null && response.IV != null && response.Key != null)
                    {
                        var encryptedIV = Convert.FromBase64String(response.IV);
                        var encryptedKey = Convert.FromBase64String(response.Key);
                        var box = GetSelectedOrDefaultBox();
                        node.Key = CryptoHelper.DecryptPgp(encryptedKey, box.PrivateKey, box.PassPhrase);
                        node.IV = CryptoHelper.DecryptPgp(encryptedIV, box.PrivateKey, box.PassPhrase);
                    }

                    node.CheckedForKeys = true;
                    database.Update(node);
                }
            }

        }

        private void RefreshFolder(string path) {
			var explorer = new RemoteExplorer();

			//Get all pending share inventations and accept them
			List<ShareInventation> foundInventations = explorer.GetPendingShareInventations ();
			foreach (ShareInventation foundShareInventation in foundInventations) { //Accept all inventations
                explorer.AcceptShareInventation (foundShareInventation.Id);
			}

            var localData = database.GetTree(path);
			var remoteData = MapDataGroupToTreeNode(explorer.GetFiles(localData.Path), localData.Id, localData.LocalBoxId);

            List<string> inBoth = (from local in localData.Children
                join remote in remoteData
                on local.Path equals remote.Path
                select local.Path).ToList();

            List<TreeNode> updateNodes = (from local in localData.Children
                join remote in remoteData
                on local.Path equals remote.Path
                select new TreeNode{ Id = local.Id, Name = remote.Name, Type = local.Type, 
					ParentId = local.ParentId, Path = local.Path, IsDirectory = remote.IsDirectory, HasKeys = remote.HasKeys,
                IsFavorite = local.IsFavorite, IsShare = remote.IsShare, IsShared = remote.IsShared, LocalBoxId = local.LocalBoxId, IV = local.IV, Key = local.Key, CheckedForKeys = local.CheckedForKeys} ).ToList();

            var toAdd = remoteData.Where(e => !inBoth.Contains(e.Path)).ToList();
            var toRemove = localData.Children.Where(e => !inBoth.Contains(e.Path)).ToList();

            updateNodes.ForEach(e => database.Update(e));
            toAdd.ForEach(e => database.AddNode(e));
            toRemove.ForEach(e =>  {
                DeleteLocalFileOrFolder(e);
                database.RemoveNode(e);
            });
        }

        private List<TreeNode> MapDataGroupToTreeNode(DataGroup dg, int parent, int boxId) {
           
            List<TreeNode> result = new List<TreeNode>();
            if (dg.Items != null)
            {
                foreach (DataItem di in dg.Items)
                {
                    var newNode = new TreeNode()
                    {
                        Name = di.Title,
                        Type = di.FileType,
                        Path = di.Path,
                        IsDirectory = di.IsFolder,
                        ParentId = parent,
                        IsShare = di.Share,
                        IsShared = di.Shared,
						LocalBoxId = boxId,
						HasKeys = di.HasKeys
                    };
                    result.Add(newNode);
                }
            }
            return result;
        }

        public void EmptyDecryptedFolder() {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "decrypted");
            if (Directory.Exists(path))
            {
                Array.ForEach(Directory.GetDirectories(path), e=> Directory.Delete(e, true));
                Array.ForEach(Directory.GetFiles(path), File.Delete);
            }
        }

        public string GetFilePathSync(string path)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var pathToFile =  Path.Combine(documentsPath, "" + Waardes.Instance.GeselecteerdeBox, path.Substring(1, path.Length -1));
            EmptyDecryptedFolder();


			if(!File.Exists(pathToFile)) //Does not exist
            {
                var explorer = new RemoteExplorer();
                var fileBytes = explorer.GetFile(path);

                if(fileBytes == null) {
                    return null;
                }

				if (!Directory.Exists(pathToFile.Substring(0, pathToFile.LastIndexOf("/"))))
                {
					Directory.CreateDirectory(pathToFile.Substring(0, pathToFile.LastIndexOf("/")));
                }

				if (pathToFile != null)
					File.WriteAllBytes(pathToFile, fileBytes);
            }



            // moeten buiten de root zitten
            if (path.Count(e => e == '/') > 1)
            {
                int index = path.IndexOf('/', path.IndexOf('/') + 1);
                var rootFolder = path.Substring(0, index);
                var folder = GetFolder(rootFolder).Result;

                if (folder.HasCryptoKeys)
                {
					try{
						Rijndael rijndael = Rijndael.Create(); 
                    	rijndael.Key = folder.Key; 
                    	rijndael.IV = folder.IV; 
							
                    	var decpath = Path.Combine(documentsPath, "decrypted", path.Substring(1, path.Length - 1));
                    	if (!Directory.Exists(decpath.Substring(0, decpath.LastIndexOf("/"))))
                    	{
                    	    Directory.CreateDirectory(decpath.Substring(0, decpath.LastIndexOf("/")));
                    	}

						using (var stream = new CryptoStream(File.OpenRead(pathToFile), rijndael.CreateDecryptor(), CryptoStreamMode.Read))
                    	using(var fs = File.OpenWrite(decpath))
                    	{
                    	    stream.CopyTo(fs);
                    	}
                    	return decpath;
					}catch{
						return pathToFile;
					}
                }
            }
			
			return pathToFile;
        }

        public Task<string> GetFilePath(string path)
        {
            return Task.Run(() =>
            {
                return GetFilePathSync(path);
            });
        }

        public Task<bool> CreateFolder(string path, bool encrypt = true) {
            return Task.Run(() =>
            {
                var explorer = new RemoteExplorer();
                var result = explorer.CreateFolder(path);

                // kijken of het een map op rootniveau is
                if(encrypt && path.Count(e => e == '/') == 1) {
                    AESKeyAndIVGenerator gen = new AESKeyAndIVGenerator();
                    var box = GetSelectedOrDefaultBox();
                    var post = new AesKeyPost() {
                        User = null,
                        Key = Convert.ToBase64String(CryptoHelper.EncryptPgp(gen.aesKey, box.PublicKey)),
                        IV = Convert.ToBase64String(CryptoHelper.EncryptPgp(gen.aesIV, box.PublicKey))
                    };

                    var keyResult = explorer.AddAesKey(path, post);
                }
                return result;
            });
        }

        public Task<bool> FavoriteFile(TreeNode treeNodeToFavorite)
		{
            return Task.Run(async () =>
			{
				bool updateSucceeded = false;
				
				if (!treeNodeToFavorite.IsDirectory) {
					treeNodeToFavorite.IsFavorite = true;
                    database.UpdateNode (treeNodeToFavorite);
                    await GetFilePath(treeNodeToFavorite.Path);
					updateSucceeded = true;
				} else {
					updateSucceeded = false;
				}
				return updateSucceeded;
			});
		}


		public Task<bool> UnFavoriteFile(TreeNode treeNodeToUnFavorite)
		{
			return Task.Run(() =>
				{
					bool updateSucceeded = false;

					if (treeNodeToUnFavorite.IsFavorite) {
						treeNodeToUnFavorite.IsFavorite = false;
						database.UpdateNode (treeNodeToUnFavorite);
						updateSucceeded = true;
					} else {
						updateSucceeded = false;
					}
					return updateSucceeded;
				});
		}


        public List<string> GetBoxNames()
        {
            var result = new List<string>();
            var boxes = database.GetLocalBoxes();

            foreach(LocalBox b in boxes)
            {
                result.Add(b.Name);
            }

            return result;
        }

        public Task <List<LocalBox>> GetLocalBoxes()
        {
            return Task.Run (() => {
                return database.GetLocalBoxes ();
            });
        }

        public List<LocalBox> GetLocalBoxesSync()
        {
            return database.GetLocalBoxes ();
        }


		public List<TreeNode> GetFavorites()
		{
			return database.GetFavorites(GetSelectedOrDefaultBox().Id);
		}

        public bool NodeExists(TreeNode node) {
            return database.NodeExist(node);
        }


        public LocalBox GetSelectedOrDefaultBox()
        {
            var boxes = database.GetLocalBoxes();

            int geselecteerdeBox = Waardes.Instance.GeselecteerdeBox;
            if (geselecteerdeBox != -1)
            {
                foreach(LocalBox b in boxes)
                {
                    if (b.Id == geselecteerdeBox)
                    {
                        return b;
                    }
                }
            }

            // Eerste in de lijst:
            Waardes.Instance.GeselecteerdeBox = boxes[0].Id;
            return boxes[0];
        }

		public Task<bool> DeleteFileOrFolder(string fileOrFolderPath) {
			return Task.Run (() => {
				var remoteExplorer = new RemoteExplorer();
                DeleteLocalFileOrFolder(fileOrFolderPath);
				return remoteExplorer.DeleteFile(fileOrFolderPath);
			});
		}

		public void DeleteLocalFileOrFolder(string path)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pathToRemove = Path.Combine(documentsPath, "" + Waardes.Instance.GeselecteerdeBox, path.Substring(1, path.Length -1));

            if(File.Exists(pathToRemove) || Directory.Exists(pathToRemove)) {
                bool directory = (File.GetAttributes(pathToRemove) & FileAttributes.Directory) == FileAttributes.Directory;
                if(directory) {
                    Directory.Delete(pathToRemove, true);
                } else {
					if(File.Exists(pathToRemove)){
                   	 	File.Delete(pathToRemove);
					}
					bool fileExists = File.Exists (pathToRemove);
					Console.WriteLine (fileExists);
                }
            }
        }

        void DeleteLocalFileOrFolder(TreeNode e)
        {
            DeleteLocalFileOrFolder(e.Path);
        }

        public int AddLocalBox(LocalBox box)
        {
            return database.AddLocalBox(box);
        }

        public void UpdateLocalBox(LocalBox box)
        {
            database.Update(box);
        }

		public Task<PublicUrl> CreatePublicFileShare(string filePath, DateTime expirationDateOfShare)
		{
			return Task.Run (() => {
				var remoteExplorer = new RemoteExplorer();

				return remoteExplorer.CreatePublicFileShare(filePath, expirationDateOfShare);
			});
		}

		public void DeleteLocalBox(int localBoxId)
		{
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pathToRemove = Path.Combine(documentsPath, "" + localBoxId);
            if (Directory.Exists(pathToRemove))
            {
                Directory.Delete(pathToRemove, true);
            }
               
			database.DeleteLocalBox (localBoxId);
		}


        public Task<bool> UploadFile (string destination, string file)
		{
			return Task.Run (() => {
				try {
					var remoteExplorer = new RemoteExplorer ();

					if (destination.Count (e => e == '/') > 1) {
						int index = destination.IndexOf ('/', destination.IndexOf ('/') + 1);
						var rootFolder = destination.Substring (0, index);
						var folder = GetFolder (rootFolder).Result;

						if (folder.HasCryptoKeys) {
							Rijndael rijndael = Rijndael.Create (); 
							rijndael.Key = folder.Key; 
							rijndael.IV = folder.IV; 

							using (var stream = new CryptoStream (File.OpenRead (file), rijndael.CreateEncryptor (), CryptoStreamMode.Read)) {
								return remoteExplorer.UploadFile (destination, stream);
							}
						}
					}

					using (var stream = File.OpenRead (file)) {
						return remoteExplorer.UploadFile (destination, stream);
					}
				} catch {
					return false;
				}

			});
		}
        

		public async Task<bool> SavePdfAnnotations (string pathOfFileToUpdate, string temporaryFilePath, bool androidClient, bool isFavorite)
		{
			bool deleteSucceeded = false;
			bool uploadedSucceeded = false;

			try {
				if(androidClient){
					temporaryFilePath = temporaryFilePath.Substring(7);
				}else{
					temporaryFilePath = temporaryFilePath.Substring(8);
				}
				if (File.Exists (temporaryFilePath)) {
					//Delete file from local box
					deleteSucceeded = await DataLayer.Instance.DeleteFileOrFolder (pathOfFileToUpdate);
				}

				if (deleteSucceeded) {
					//Upload new file to local box
					uploadedSucceeded = await DataLayer.Instance.UploadFile (pathOfFileToUpdate, temporaryFilePath);
				}

				//Verwijder temporary file
				if (File.Exists (temporaryFilePath)) {
					Console.WriteLine("File to delete: " + temporaryFilePath);
					File.Delete (temporaryFilePath);
				}

				if(isFavorite)
				{
					TreeNode treeNodeToFavorite = await DataLayer.Instance.GetTreeNode(pathOfFileToUpdate);
					await FavoriteFile(treeNodeToFavorite);

					string filePath = GetFilePathSync(pathOfFileToUpdate);

					return uploadedSucceeded;
				}
				else{
					return uploadedSucceeded;
				}



			} catch (Exception ex){
				Console.WriteLine (ex.Message);
				return uploadedSucceeded;
			}
		}


        public Task<List<Identity>> GetLocalboxUsers()
        {
            return Task.Run (() => {
                var currentBox = GetSelectedOrDefaultBox();
                var remoteExplorer = new RemoteExplorer();
                var users = remoteExplorer.GetLocalBoxUsers().Result.Where(e => e.Username != currentBox.User).ToList();
                return users;
            });
        }


    }
}

/*
		public async Task<bool> SavePdfAnnotations (string pathOfFileToUpdate)
		{
			bool deleteSucceeded = false;
			bool uploadedSucceeded = false;

			try {

				//Save pdf file to temporary file - this file is used to upload
				var documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
				string temporaryFilePath = System.IO.Path.Combine (documentsPath, "temporary.pdf");

				if (File.Exists (temporaryFilePath)) {
					File.Delete (temporaryFilePath);
				}
					
				//Save temporary file in filesystem
				RemoteExplorer remoteExplorer = new RemoteExplorer ();
				Byte[] fileBytes = remoteExplorer.GetFile (pathOfFileToUpdate);

				File.WriteAllBytes (temporaryFilePath, fileBytes);

				if (File.Exists (temporaryFilePath)) {
					//Delete file from local box
					deleteSucceeded = await DataLayer.Instance.DeleteFileOrFolder (pathOfFileToUpdate);
				}

				if (deleteSucceeded) {
					//Upload new file to local box
					uploadedSucceeded = await DataLayer.Instance.UploadFile (pathOfFileToUpdate, temporaryFilePath);
				} else {
					//Verwijder temporary file
					if (File.Exists (temporaryFilePath)) {
						File.Delete (temporaryFilePath);
					}
				}

				return uploadedSucceeded;

			} catch {
				return uploadedSucceeded;
			}
		}*/