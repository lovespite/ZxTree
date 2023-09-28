using System;
using System.Runtime.InteropServices;

namespace ZxTree.Core.DiskIO;

[StructLayout(LayoutKind.Sequential)]
public struct Int64Cursor
{
    public Int64 Position { get; set; }
    public Int64 Length { get; set; }
}