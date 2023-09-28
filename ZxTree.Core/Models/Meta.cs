using System;
using System.IO;
using System.Runtime.InteropServices;
using ZxTree.Core.Converting;

namespace ZxTree.Core.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Meta
    {
        public DataType DataType { get; set; }
        public long TimeCreated { get; set; }
        public long TimeWritten { get; set; }
        public bool ReadOnly { get; set; }

        public int OwnerId { get; set; }
        public int GroupId { get; set; }
        public int Permissions { get; set; }

        public long Address { get; set; }
        public long Size { get; set; }

        public long Reserved0 { get; set; }
        public long Reserved1 { get; set; }

        public bool IsEmpty => Size == 0;

        public byte[] GetBytes()
        {
            var buffer = new byte[Marshal.SizeOf<Meta>()];

            var bufPtr = Marshal.AllocHGlobal(buffer.Length);

            Marshal.StructureToPtr(this, bufPtr, false);

            Marshal.Copy(bufPtr, buffer, 0, buffer.Length);

            Marshal.FreeHGlobal(bufPtr);

            return buffer;
        }

        public void CopyTo(Stream stream)
        {
            var buffer = GetBytes();

            stream.Write(buffer, 0, buffer.Length);
        }

        public static Meta FromBytes(byte[] bytes)
        {
            if (bytes.Length < Marshal.SizeOf<Meta>()) throw new ArgumentException("Invalid byte array");

            var bufPtr = Marshal.AllocHGlobal(bytes.Length);

            Marshal.Copy(bytes, 0, bufPtr, bytes.Length);

            var metaData = Marshal.PtrToStructure<Meta>(bufPtr);

            Marshal.FreeHGlobal(bufPtr);

            return metaData;
        }

        public static Meta FromStream(Stream stream)
        {
            var buffer = new byte[Marshal.SizeOf<Meta>()];

            var read = stream.Read(buffer, 0, buffer.Length);

            if (read != buffer.Length) throw new InvalidDataException("Invalid stream");

            return FromBytes(buffer);
        }
    }
}