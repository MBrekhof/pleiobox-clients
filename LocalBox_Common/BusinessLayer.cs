using System;
using System.Net;
using System.IO;
using System.Linq;
using LocalBox_Common.Remote;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Xamarin;

namespace LocalBox_Common
{
	public class BusinessLayer
	{
		private static BusinessLayer _instance = null;

		public static BusinessLayer Instance {
			get {
				if (_instance == null) {
					_instance = new BusinessLayer ();
				}
				return _instance;
			}
		}

		public BusinessLayer ()
		{
		}

		public Task<LocalBox> RegisterLocalBox (string boxUrl, string cookieString, bool androidClient)
		{
			return Task.Run (() => {
			
				LocalBox result = null;

				try {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(boxUrl); 

					request.ContentType = "application/json";
					request.Timeout	= 10000; //10 seconds before timeout

					//If cookie not null then add to http request header
					if (cookieString != null) {
						request.Headers.Add (HttpRequestHeader.Cookie, cookieString);
					}
					request.Method = "GET";
				
					using (HttpWebResponse response = request.GetResponse () as HttpWebResponse) {
						if (response.StatusCode != HttpStatusCode.OK) {

							Console.Out.WriteLine ("Error fetching data. Server returned status code: {0}", response.StatusCode);
							return result;
						}

						using (StreamReader reader = new StreamReader (response.GetResponseStream ())) {
							var content = reader.ReadToEnd ();
							if (string.IsNullOrWhiteSpace (content)) {
								Console.Out.WriteLine ("Response contained empty body...");
								return result;
							} else {
								try {
									//Parse json to LocalBox object
									LocalBox box = JsonConvert.DeserializeObject<LocalBox> (content);

									//Get apikeys from json
									var apiKeys = JObject.Parse(content).SelectToken("APIKeys").ToString();

									//Parse json to APIKeys list
									var foundApiKeys = JsonConvert.DeserializeObject<List<APIKeys>> (apiKeys);

									//Get correct APIKey from list
									foreach(APIKeys foundKeys in foundApiKeys)
									{
										if(androidClient == true){
											if(foundKeys.Name.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0)
											{
												box.ApiKey = foundKeys.Key;
												box.ApiSecret = foundKeys.Secret;
												break;
											}
										}else{//iOS
											if(foundKeys.Name.IndexOf("ios", StringComparison.OrdinalIgnoreCase) >= 0)
											{
												box.ApiKey = foundKeys.Key;
												box.ApiSecret = foundKeys.Secret;
												break;
											}
										}
									}

									//Voorkomt crash in Android
									if (DataLayer.Instance.DatabaseUnlocked ()) {
										box.Id = DataLayer.Instance.AddOrUpdateLocalBox (box);
									}
									result = box;
  
								} catch (Exception ex){
									Insights.Report(ex);
									result = null;
								}
							}
						}
					}
				}
				catch (Exception ex){
					Insights.Report(ex);
					Console.WriteLine(ex.Message);					
					return result;
				}
				return result;
			});
		}
			

		private void SetKeys (LocalBox localBox)
		{
			var explorer = new RemoteExplorer (localBox);
			var user = explorer.GetUser ();
			if (user != null && !(string.IsNullOrEmpty (user.PrivateKey) || string.IsNullOrEmpty (user.PublicKey))) {
				localBox.PrivateKey = Convert.FromBase64String (user.PrivateKey);
				localBox.PublicKey = Convert.FromBase64String (user.PublicKey);
			}
			DataLayer.Instance.AddOrUpdateLocalBox (localBox);
		}


		public Task<bool> Authenticate (LocalBox localBox)
		{
			return Task.Run (() => {
				//bool result = false;

				try {

					var explorer = new RemoteExplorer (localBox);
					//result = explorer.Authorize (password);

					//if (result) {
					DataLayer.Instance.AddOrUpdateLocalBox (localBox);
						SetKeys (localBox);
					//} else {
						//Login failure so delete local box
					//	DataLayer.Instance.DeleteLocalBox (localBox.Id);
					//}

					return true;
				} catch (Exception ex){
					Insights.Report(ex);
					DataLayer.Instance.DeleteLocalBox (localBox.Id);
					return false;
				}
			});
		}

		public Task<bool> ShareFolder (string path, List<Identity> users)
		{
			return Task.Run (() => {

				LocalBox box = DataLayer.Instance.GetSelectedOrDefaultBox ();
				var explorer = new RemoteExplorer (box);

				var messages = AddKeys (path, users, users.Select (e => e).ToList ());
				foreach (var message in messages) {
					var r = explorer.AddAesKey (path, message);
				}
				return explorer.ShareFolder (path, users);
			});
		}

		public Task<Share> GetShareSettings (string pathOfShare)
		{
			try{
				LocalBox box = DataLayer.Instance.GetSelectedOrDefaultBox ();
				var explorer = new RemoteExplorer (box);

				return explorer.GetShareSettings (pathOfShare);
			}
			catch (Exception ex){
				Insights.Report(ex);
				return null;
			}
		}


		public Task<bool> UpdateSettingsSharedFolder (Share share, List<Identity> usersToShareWith)
		{
			return Task.Run (() => {
				LocalBox box = DataLayer.Instance.GetSelectedOrDefaultBox ();
				var explorer = new RemoteExplorer (box);

				List<Identity> inBoth = (from local in share.Identities
				                          join remote in usersToShareWith
                on local.Id equals remote.Id
				                          select local).ToList ();

				var toAdd = usersToShareWith.Where (e => inBoth.All (f => f.Id != e.Id)).ToList ();
				var toRemove = share.Identities.Where (e => inBoth.All (f => f.Id != e.Id)).ToList ();

				var messages = AddKeys (share.Item.Path, usersToShareWith, toAdd);
				RemoveKeys (share.Item.Path, toRemove);
					

				var result = explorer.UpdateSettingsSharedFolder (share.Id, usersToShareWith);

				foreach (var message in messages) {
					var r = explorer.AddAesKey (share.Item.Path, message);
				}

				return result;
			});
		}


		private List<AesKeyPost> AddKeys (string path, List<Identity> usersToShareWith, List<Identity> toAdd)
		{
			List<AesKeyPost> result = new List<AesKeyPost> ();

			try {
				LocalBox box = DataLayer.Instance.GetSelectedOrDefaultBox ();
				var explorer = new RemoteExplorer (box);

				var node = DataLayer.Instance.GetFolder (path).Result;

				if (node.HasCryptoKeys) {
					foreach (var identity in toAdd) {
						var user = explorer.GetUser (identity.Username);
						if (!string.IsNullOrEmpty (user.PublicKey)) {
							byte[] pkey = Convert.FromBase64String (user.PublicKey);
							result.Add (new AesKeyPost () {
								User = identity.Username,
								Key = Convert.ToBase64String (CryptoHelper.EncryptPgp (node.Key, pkey)),
								IV = Convert.ToBase64String (CryptoHelper.EncryptPgp (node.IV, pkey))
							});
						} else {
							var u = identity;
							usersToShareWith.RemoveAll (e => e.Id.Equals (u.Id));
						}
					}
				}
				return result;
			} 
			catch (Exception ex){
				Insights.Report(ex);
				return result;
			}
		}

		private void RemoveKeys (string path, List<Identity> toRemove)
		{
			LocalBox box = DataLayer.Instance.GetSelectedOrDefaultBox ();
			var explorer = new RemoteExplorer (box);

			foreach (var identity in toRemove) {
				explorer.RevokeAesKey (path, new AesKeyRevoke () {
					Username = identity.Username
				});
			}
		}

		public Task<bool> SetPublicAndPrivateKey (LocalBox localBox, string passPhrase)
		{ 
			return Task.Run (() => {

				try {
					byte[] publicKey;
					byte[] privateKey;
					CryptoHelper.GenerateKeyPair (localBox.User, passPhrase, out publicKey, out privateKey);
					var explorer = new RemoteExplorer (localBox);

					var result = explorer.UpdateUser (new UserPost () {
						PublicKey = Convert.ToBase64String (publicKey),
						PrivateKey = Convert.ToBase64String (privateKey)
					});
					if (result) {
						localBox.PublicKey = publicKey;
						localBox.PrivateKey = privateKey;
						localBox.PassPhrase = passPhrase;
						DataLayer.Instance.AddOrUpdateLocalBox (localBox);
					}
					return result;
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}

			});
		}

		public Task<bool> ValidatePassPhrase (LocalBox localBox, string passPhrase)
		{
			return Task.Run (() => {
				try {
					var result = CryptoHelper.ValidatePassPhrase (localBox.PrivateKey, passPhrase);

					localBox.PassPhrase = passPhrase;
					DataLayer.Instance.AddOrUpdateLocalBox (localBox);

					return result;
				} catch (Exception ex){
					Insights.Report(ex);
					return false;
				}
			});
		}

	}
}

