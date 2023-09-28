using ZxTree.Core.Converting;

namespace ZxTree.Core.Models
{
    public class Record
    {
        public Meta Meta { get; private set; }
        public DataPresent Data { get; private set; } = DataPresent.Empty;

        public bool IsEmpty => Meta.IsEmpty;

        private Record()
        {
        }
    }
}