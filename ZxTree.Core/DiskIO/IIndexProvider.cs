using System;
using System.Collections.Generic;

namespace ZxTree.Core.DiskIO;

public interface IIndexProvider : IDisposable
{
    IEnumerable<KeyValuePair<string, Guid>> Query(IEnumerable<string> entryNames);
    long Create(IEnumerable<KeyValuePair<string, Guid>> items);
    void Update(IEnumerable<KeyValuePair<string, Guid>> items);
    long UpdateOrCreate(IEnumerable<KeyValuePair<string, Guid>> items);
    long Count();
}