using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ZxTree.Core.DiskIO;

public class ZipTreeIndex : IIndexProvider
{
    private readonly Stream _stream;
    private readonly ZipArchive _index;

    private IDictionary<string, Guid> _memoryCache = new ConcurrentDictionary<string, Guid>();

    public ZipTreeIndex(FileInfo indexFile, bool speedUp = true)
    {
        _stream = indexFile.Open(FileMode.OpenOrCreate);
        _index = new ZipArchive(_stream, ZipArchiveMode.Update, false, System.Text.Encoding.UTF8);
        if (speedUp) SpeedUp();
    }

    /// <summary>
    /// Load all entries into memory
    /// </summary>
    /// <exception cref="InvalidOperationException">One or more items have been cached into memory.</exception>
    private void SpeedUp()
    {
        if (_index.Entries.Count == 0) return;
        lock (_index)
        {
            foreach (var entry in _index.Entries)
            {
                var buffer = new byte[16].AsSpan();
                using var stream = entry.Open();
                stream.Read(buffer);
                _memoryCache.Add(entry.Name, new Guid(buffer));
            }
        }
    }

    /// <summary>
    /// Query the index for the given entry names
    /// </summary>
    /// <param name="entryNames"></param>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, Guid>> Query(IEnumerable<string> entryNames)
    {

        foreach (var entryName in entryNames)
        {
            if (_memoryCache.TryGetValue(entryName, out var cached))
            {
                yield return new KeyValuePair<string, Guid>(entryName, cached);
                continue;
            }

            lock (_index)
            {
                var entry = _index.GetEntry(entryName);
                if (entry is null)
                {
                    yield return new KeyValuePair<string, Guid>(entryName, Guid.Empty);
                    continue;
                }

                var buffer = new byte[16].AsSpan();
                using var stream = entry.Open();
                stream.Read(buffer);

                yield return new KeyValuePair<string, Guid>(entryName, new Guid(buffer));
                _memoryCache.Add(entryName, new Guid(buffer)); // Add to cache
            }
        }
    }

    /// <summary>
    /// Create a new entry in the index. Must ensure the entry does not exist, or will lead to index corruption.
    /// </summary>
    /// <param name="items"></param>
    /// <exception cref="ArgumentException">Entry already exists</exception>
    /// <returns></returns>
    public long Create(IEnumerable<KeyValuePair<string, Guid>> items)
    {

        lock (_index)
        {
            foreach (var item in items)
            {
                if (_memoryCache.ContainsKey(item.Key)) throw new Exception("Entry already exists");

                _memoryCache.Add(item.Key, item.Value); // Add to cache

                var entry = _index.CreateEntry(item.Key);

                using var stream = entry.Open();
                stream.Write(item.Value.ToByteArray());

            }

            _stream.Flush();
            return _index.Entries.Count;
        }
    }

    public void Update(IEnumerable<KeyValuePair<string, Guid>> items)
    {
        lock (_index)
        {
            foreach (var item in items)
            {
                var entry = _index.GetEntry(item.Key) ?? throw new Exception($"Entry {item.Key} not found");

                using var stream = entry.Open();
                stream.Write(item.Value.ToByteArray());

                _memoryCache[item.Key] = item.Value; // Update cache
            }

            _stream.Flush();
        }
    }

    public long UpdateOrCreate(IEnumerable<KeyValuePair<string, Guid>> items)
    {
        lock (_index)
        {
            foreach (var item in items)
            {
                var entry = _index.GetEntry(item.Key) ?? _index.CreateEntry(item.Key);

                using var stream = entry.Open();
                stream.Write(item.Value.ToByteArray());

                _memoryCache[item.Key] = item.Value; // Update cache
            }

            _stream.Flush();
            return _index.Entries.Count;
        }
    }

    public long Count()
    {
        lock (_index)
        {
            return _index.Entries.Count;
        }
    }

    public void Dispose()
    {
        _memoryCache.Clear();
        _index.Dispose();
    }
}