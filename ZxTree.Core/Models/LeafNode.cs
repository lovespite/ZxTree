using System;

namespace ZxTree.Core.Models
{
    public class LeafNode : TreeNodeBase
    {
        public LeafNode(TreeNodeBase parent, string name) : base(name)
        {
        }

        public override bool IsRoot => false;
        public override bool IsLeaf => true;

        public override TreeNodeBase CreateChild(string name)
        {
            throw new InvalidOperationException("Leaf node cannot have children");
        }

        public override bool RemoveChild(string name, out TreeNodeBase nodeBase)
        {
            throw new InvalidOperationException("Leaf node cannot have children");
        }

        public override TreeNodeBase GetChild(string name)
        {
            throw new InvalidOperationException("Leaf node cannot have children");
        }

        public override bool HasChild(string name)
        {
            return false;
        }

        public override void ClearChildren()
        {
            throw new InvalidOperationException("Leaf node cannot have children");
        }
    }
}