using System;
using System.Collections.Generic;

namespace ZxTree.Core.Models
{
    public class TreeNodeBase : NodeBase
    {
        protected Guid ParentId { get; set; } = Guid.Empty;

        protected TreeNodeBase(string name)
        {
            Name = name;
        }

        protected readonly IDictionary<string, TreeNodeBase> SubNodes = new Dictionary<string, TreeNodeBase>();

        /// <summary>
        /// Can be null if this is a root node
        /// </summary>
        public virtual TreeNodeBase Parent => TreeNodeCache.Get(ParentId);

        public IEnumerable<TreeNodeBase> Children => SubNodes.Values;

        public string GetFullPath()
        {
            if (ParentId == Guid.Empty) return Name; // root node
            var sb = new System.Text.StringBuilder(Name);

            var node = Parent;

            while (node != null)
            {
                sb.Insert(0, node.Name + "/");
                node = node.Parent;
            }

            return sb.ToString();
        }

        public override string ToString() => GetFullPath();

        public virtual bool IsRoot => string.IsNullOrEmpty(Name); // only root node has empty name
        public virtual bool IsLeaf => SubNodes.Count == 0 && !IsRoot;

        public bool IsFree => ParentId == Guid.Empty && !IsRoot;

        public virtual TreeNodeBase CreateChild(string name)
        {
            if (SubNodes.ContainsKey(name)) throw new ArgumentException("Node name already exists");

            var node = new TreeNodeBase(name)
            {
                // update parent
                ParentId = Id
            };

            // add to child list
            SubNodes.Add(name, node);

            TreeNodeCache.Cache(node);

            return node;
        }

        public virtual bool RemoveChild(string name, out TreeNodeBase nodeBase)
        {
            if (!SubNodes.TryGetValue(name, out nodeBase)) return false;

            // remove from child list
            SubNodes.Remove(name);

            // set parent to null
            nodeBase.ParentId = Guid.Empty;

            return true;
        }

        public virtual TreeNodeBase GetChild(string name)
        {
            if (!SubNodes.TryGetValue(name, out var node)) throw new KeyNotFoundException();
            return node;
        }

        public virtual bool HasChild(string name)
        {
            return SubNodes.ContainsKey(name);
        }

        public virtual void ClearChildren()
        {
            foreach (var tn in SubNodes.Values)
            {
                // set parent to null
                tn.ParentId = Guid.Empty;
            }

            // clear child list
            SubNodes.Clear();
        }
    }
}