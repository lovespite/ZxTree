using System;
using ZxTree.Core.Converting;

namespace ZxTree.Core.Models
{
    public class DataNode : TreeNodeBase
    {

        public DataNode(string name, DataType dataType) : base(name)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}