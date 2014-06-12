using System;
using System.Collections;
using SQLite;
using System.Collections.Generic;

namespace LocalBox_Common
{
    public class TreeNode
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string  Name { get; set; }
        public string Type { get; set; }
        public int  ParentId { get; set; }
        public int  LocalBoxId { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
		public bool IsFavorite { get; set; }
        public bool IsShare { get; set; }
        public bool IsShared { get; set; }
		public bool HasKeys { get; set; }

        public bool CheckedForKeys { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        [Ignore]
        public bool HasCryptoKeys { 
            get { 
                return Key != null && Key.Length > 0 && IV != null && IV.Length > 0;
            } 
        }

        [Ignore]
        public List<TreeNode> Children { get; set; }

        public TreeNode() 
        {
            this.Id = -1;
            Children = new List<TreeNode>();
        }

        public void Display(int depth)
        {
            int indent = 1;
            if (ParentId != 0)
            {
                indent = depth;
            }

            Console.WriteLine(new String(' ', indent) + Path);

            // Recursively display child nodes 
            foreach (TreeNode component in Children)
            {
                component.Display(indent + 2);
            }
        }
    } 
}

