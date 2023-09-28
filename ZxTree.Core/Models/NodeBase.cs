using System;
using System.Collections.Generic;
using System.IO;

namespace ZxTree.Core.Models
{
    public abstract class NodeBase
    {
        public Guid Id { get; } = Guid.NewGuid();

        private string _nodeName = string.Empty;

        public string Name
        {
            get => _nodeName;
            protected set => _nodeName = CheckNodeName(value);
        }

        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        protected NodeBase AsRoot()
        {
            // skip validation
            _nodeName = string.Empty;

            return this;
        }

        public static string CheckNodeName(string name)
        {
            if (name.Length == 0) throw new ArgumentException("Name cannot be empty");
            if (name.Length > 255) throw new ArgumentException("Name cannot be longer than 255 characters");
            if (name.IndexOfAny(InvalidPathChars) >= 0)
                throw new ArgumentException("Name cannot contain invalid path characters");

            return name;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeBase other && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        public class EqualityComparer : IEqualityComparer<NodeBase>
        {
            public bool Equals(NodeBase x, NodeBase y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(NodeBase obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}