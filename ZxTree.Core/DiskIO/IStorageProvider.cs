using ZxTree.Core.Converting;

namespace ZxTree.Core.DiskIO;

public interface IStorageProvider
{
    public DataPresent Read(string path);
    public void Write(string path, DataPresent data);
    public void Delete(string path);

    public IEnumerable<DataPresent> Read(IEnumerable<string> paths);
    public int Write(IEnumerable<KeyValuePair<string, DataPresent>> items);

    public int Delete(IEnumerable<string> paths);

}