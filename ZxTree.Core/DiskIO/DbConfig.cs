using System;
using ZxTree.Core.Models;

namespace ZxTree.Core.DiskIO;

public class DbConfig : ConfigurationBase
{
    public string DbName { get; set; } = string.Empty;
    public long MaxSectionIndexSize { get; set; } = ushort.MaxValue;
    public Guid CurrentSectionId { get; set; } = Guid.Empty;
    public Guid[] Sections { get; set; } = Array.Empty<Guid>();
    public int Permissions { get; set; } = 0;
    public long OwnerId { get; set; } = 0;
    public long GroupId { get; set; } = 0;
}