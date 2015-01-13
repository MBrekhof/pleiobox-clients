using System;
using SQLite;
using System.Linq;
using System.IO;
using System.Collections;
using LocalBox_Common.Remote;
using Synchronization.Models;
using System.Collections.Generic;
using Xamarin;

namespace LocalBox_Common
{
    public class Database : SQLiteConnection
    {
//        private static Database _instance = null;

        private static readonly Object _dbCreateLock = new Object();

        public object Lock { get {return _dbCreateLock; } }

//        public static Database Instance
//        { 
//            get
//            {
//                lock (_dbCreateLock)
//                {
//                    if (_instance == null)
//                    {
//                        string db = Path.Combine(Environment.GetFolderPath(DocumentConstants.DocumentsPath, "localbox.db");
//                        _instance = new Database(db, "1234");
//                    }
//                }
//                return _instance;
//            }
//        }

        public Database(string dbpath, string password) : base(dbpath, password)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            CreateTable<TreeNode>();
            CreateTable<Waarde>();
            CreateTable<LocalBox>();
            // debug sql queries, Trace = true;
        }

        public void Empty()
        {
            foreach (string tbl in new[] { 
                "TreeNode", "Waarde", "LocalBox"
            })
            {
                this.Execute(string.Format("delete from {0}", tbl));
            }
        }

        public TreeNode QueryNode(int id)
        {
            return TransactionWithResult<TreeNode>(() =>
            {
                TreeNode result =
                    (from node in Table<TreeNode>()
                        where node.Id == id
                        select node).SingleOrDefault();

                return result;
            });
        }

        public bool NodeExist(TreeNode node) {
            var q = (from n in this.Table<TreeNode>()
                              where n.LocalBoxId == node.LocalBoxId
                                  && n.Path.Equals(node.Path)
                              select n).Count();
            return q == 1;
        }

        public void InsertNode(TreeNode node)
        {
            Transaction(() =>
            {
                this.Insert(node);
            });
        }

        #region

		public int AddOrUpdateLocalBox(LocalBox box)
        {
            int a = -1;
            if ((from l in this.Table<LocalBox>()
                where l.Id == box.Id
                select l).Count() == 0)
            {
                Transaction(() =>
                {
                    Insert(box);
                    a = box.Id;
                });
            }
            else
            {
                Update(box);
                a = box.Id;
            }

            return a;
        }


		public void DeleteLocalBox(int localBoxId)
		{
			Execute ("delete from LocalBox where id = ?", localBoxId);
            Execute ("delete from TreeNode where LocalBoxId = ?", localBoxId);
		}



        public List<LocalBox> GetLocalBoxes()
        {
            var boxes = this.Query<LocalBox> (
                "SELECT * FROM LocalBox");

            return boxes;
        }

		public List<TreeNode> GetFavorites(int localBoxId)
		{
            var favorites = from f in this.Table<TreeNode>()
					where f.IsFavorite &&
					f.LocalBoxId == localBoxId
                select f;
            return favorites.ToList();
		}

        public int AddNode(TreeNode treeNode)
        {
            int a = -1;
            Transaction(() =>
            {
                var s = this.Insert(treeNode);
                a = treeNode.Id;
            });

            return a;
        }

        public void UpdateNode(TreeNode treeNode)
        {
            Transaction(() =>
            {
                this.Update(treeNode);
            });
        }

        public void RemoveNode(TreeNode node) {
            var children = from n in Table<TreeNode>()
                    where n.ParentId == node.Id
                    select n;
                
            foreach (TreeNode n in children)
            {
                RemoveNode(n);
            }

            Delete(node);
        }

        public TreeNode GetTree()
        {
            var models = this.Query<TreeNode> (
                "SELECT * FROM TreeNode");

            if (models.Count == 0)
                return null;
            TreeNode rootNode = models[0];

            foreach (TreeNode t in models)
            {

                t.Children = (from node in models
                             where node.ParentId == t.Id
                    select node).ToList();
            }

            return rootNode;
        }

        public TreeNode GetTree(string path)
        {
            var node = (from n in this.Table<TreeNode>()
                where n.Path == path &&
                n.LocalBoxId == Waardes.Instance.GeselecteerdeBox
                select n).SingleOrDefault();


            if (node != null){
                var children = this.Query<TreeNode> (
                    "SELECT * FROM TreeNode where parentId = ?", node.Id);
                node.Children = children.ToList();
            }

            return node;
        }
			
        public Dictionary<string,string> GetAllState()
        {
            return TransactionWithResult<Dictionary<string, string>>(() =>
            {
                return this.Table<Waarde>().ToDictionary(s => s.Key, s => s.Value);
            });
        }

        public void SetAllState(Dictionary<string,string> state)
        {
            Transaction(() =>
            {
                foreach (KeyValuePair<string,string> kvp in state)
                {
                    this.SetState(kvp.Key, kvp.Value);
                }
            });
        }

        public void SetState(string key, string value)
        {
            Transaction(() =>
            {
                Waarde staat = new Waarde { Key = key, Value = value };

                Waarde existing = 
                    (from s in this.Table<Waarde>()
                        where s.Key == key
                        select s).FirstOrDefault();

                if (existing != null)
                {
                    this.Update(staat);
                }
                else
                {
                    this.Insert(staat);
                }
            });
        }
        #endregion

        public TResult TransactionWithResult<TResult>(Func<TResult> action)
        {
            bool beganTransaction = false;
            try
            {

                if (!this.IsInTransaction)
                {
                    beganTransaction = true;
                    this.BeginTransaction();
                }

                TResult result = action();

                if (beganTransaction)
                    Commit();

                return result;
            }
			catch (Exception ex){
				Insights.Report(ex);
                Console.WriteLine("Exception in " + ex.Source + ": " + ex.Message);
                if (beganTransaction)
                    Rollback();
                throw;
            }
        }

        public void Transaction(Action action)
        {
            bool beganTransaction = false;
            try
            {

                if (!this.IsInTransaction)
                {
                    beganTransaction = true;
                    this.BeginTransaction();
                }

                action();

                if (beganTransaction)
                    Commit();
            }
			catch (Exception ex){
				Insights.Report(ex);
                Console.WriteLine("Exception in " + ex.Source + ": " + ex.Message);
                if (beganTransaction)
                    Rollback();
                throw;
            }
        }
    }
}

