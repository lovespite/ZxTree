using System;

namespace ZxTree.Core.Models
{
    public class RootNode : TreeNodeBase
    {
        public RootNode() : base("root")
        {
            AsRoot();
        }

        public override TreeNodeBase Parent => throw new InvalidOperationException("Root node has no parent");
        public override bool IsRoot => true;
        public override bool IsLeaf => false;
    }
}