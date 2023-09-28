using System.IO.Compression;
using System.IO;
using System;
using System.Collections.Generic;
using ZxTree.Core.Utils;
using System.ComponentModel.DataAnnotations;
using ZxTree.Core.Models.Security;
using ZxTree.Core.Converting;

namespace ZxTree.Core.DiskIO;

public class CompressionStorage : IStorageProvider
{
    protected DirectoryInfo StorageDirectory { get; }
    protected DirectoryInfo DataStorageDirectory { get; }
    protected IIndexProvider SectionIndex { get; private set; }
    protected DbConfig Config { get; private set; }

    private static readonly Guid ProviderId = new("CE5BCD5C-B616-43AA-A104-B97D518AB949");

    private CompressionStorage(DirectoryInfo storageDirectory, Type indexType, SecurityIdentity sid, AesEncryption? aes = null)
    {
        StorageDirectory = storageDirectory;
        DataStorageDirectory = new DirectoryInfo(Path.Combine(storageDirectory.FullName, "data"));

        // create directories if not exists
        Initialize();

        // check version and AES provider is correct or not
        var version = new FileInfo(Path.Combine(StorageDirectory.FullName, "version"));
        ValidateVersion(version, aes);

        // load db config
        var config = new FileInfo(Path.Combine(StorageDirectory.FullName, "config"));
        Config = (DbConfig)Models.ConfigurationBase.Open(typeof(DbConfig), config.FullName, aes is null ? null : aes.SecretProvider);

        // check permission
        CheckPermission(sid);

        // load index
        var index = new FileInfo(Path.Combine(StorageDirectory.FullName, "index"));
        SectionIndex = LoadIndex(indexType, index);
    }

    private void CheckPermission(SecurityIdentity sid, AcOperation operation = AcOperation.Read)
    {
        AcPermission acp = Config.Permissions;

        if (acp.IsInvalid) acp = AcPermission.Shared;

        bool permit = AcPolicy.CheckPermission(Config.OwnerId, Config.GroupId, acp, operation, sid);

        if (!permit) throw new UnauthorizedAccessException("Permission denied");
    }

    private static void ValidateVersion(FileInfo version, AesEncryption? aes)
    {
        if (!version.Exists)
        {
            using var vstream = version.Create();
            var data = ProviderId.ToByteArray();

            if (aes is not null)
            {
                data = aes.Encrypt(data);
            }

            vstream.Write(data);

            return;
        }

        var bytes = File.ReadAllBytes(version.FullName);

        if (aes is not null)
        {
            bytes = aes.Decrypt(bytes);
        }

        if (bytes.Length != bytes.Length) throw new Exception("Version file is corrupted");

        var id = new Guid(bytes);

        if (!ProviderId.Equals(id)) throw new InvalidOperationException("Validation failed: token is not work");
    }

    private void Initialize()
    {
        if (!StorageDirectory.Exists)
        {
            StorageDirectory.Create();
            DataStorageDirectory.Create();
        }
        else
        {
        }
    }

    protected Guid CreateSection()
    {
        Guid id; // section id

    tryCreate:
        id = Guid.NewGuid();
        var dataFile = new FileInfo(Path.Combine(DataStorageDirectory.FullName, id.ToString()));

        // if file exists, try again. Though it is unlikely to happen.
        if (dataFile.Exists) goto tryCreate;

        dataFile.Create().Dispose(); // create empty file

        return id;
    }

    private static IIndexProvider LoadIndex(Type indexProviderInterface, FileInfo indexFile)
    {
        if (indexProviderInterface.GetInterface(nameof(IIndexProvider)) is null)
            throw new Exception("Index provider interface is not valid");

        var indexProvider = Activator.CreateInstance(indexProviderInterface, indexFile);

        return indexProvider is null ? throw new Exception("Index provider is not available") : (IIndexProvider)indexProvider;
    }

    public DataPresent Read(string path)
    {
        // TODO: implement
    }

    public void Write(string path, DataPresent data)
    {
        // TODO: implement
    }

    public void Delete(string path)
    {
        // TODO: implement
    }

    public IEnumerable<DataPresent> Read(IEnumerable<string> paths)
    {
        // TODO: implement
    }

    public int Write(IEnumerable<KeyValuePair<string, DataPresent>> items)
    {
        // TODO: implement
    }

    public int Delete(IEnumerable<string> paths)
    {
        // TODO: implement
    }
}