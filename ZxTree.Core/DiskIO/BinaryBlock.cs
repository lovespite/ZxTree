using System;
using System.IO;
using System.Threading;
using ZxTree.Core.DiskIO;

namespace ZxTree.Core.DiskIO;

public enum BlockMode
{
    Read,
    Write,
    ReadWrite
}

public class BinaryBlock : IDisposable
{
    private readonly Stream _stream;
    private readonly BlockMode _mode;

    private BinaryBlock(Stream stream, BlockMode mode)
    {
        _stream = Stream.Synchronized(stream);
        _mode = mode;
    }

    private void CheckMode(BlockMode requiredMode)
    {
        if (_mode == BlockMode.ReadWrite) return;

        if (_mode != requiredMode) throw new Exception($"Block is in {_mode} mode, but {_mode} mode is required");
    }

    private Memory<byte> Get(Int64Cursor cursor)
    {
        CheckMode(BlockMode.Read);
        var buffer = new byte[cursor.Length].AsMemory();
        _stream.Seek(cursor.Position, SeekOrigin.Begin);
        var read = _stream.Read(buffer.Span);
        if (read != buffer.Length) throw new Exception("Read error: length is not met");

        return buffer;
    }

    private void Put(Int64Cursor cursor, Memory<byte> buffer)
    {
        CheckMode(BlockMode.Write);
        if (buffer.Length != cursor.Length) throw new Exception("Write error: length is not met");
        _stream.Seek(cursor.Position, SeekOrigin.Begin);
        _stream.Write(buffer.Span);
    }

    private void Zero(Int64Cursor cursor)
    {
        CheckMode(BlockMode.Write);
        var buffer = new byte[cursor.Length].AsMemory();
        buffer.Span.Clear(); // zero
        _stream.Seek(cursor.Position, SeekOrigin.Begin);
        _stream.Write(buffer.Span);
    }

    public static BinaryBlock AttachTo(Stream stream, BlockMode mode)
    {
        switch (mode)
        {
            case BlockMode.Read:
                if (!stream.CanRead) throw new Exception("Stream is not readable");
                break;
            case BlockMode.Write:
                if (!stream.CanWrite) throw new Exception("Stream is not writable");
                break;
            case BlockMode.ReadWrite:
                if (!stream.CanRead || !stream.CanWrite) throw new Exception("Stream is not readable and writable");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        return new BinaryBlock(stream, mode);
    }

    public static BinaryBlock Open(FileInfo file, BlockMode mode)
    {
        FileAccess access;
        FileShare share;

        switch (mode)
        {
            case BlockMode.Read:
                share = FileShare.Read;
                access = FileAccess.Read;
                break;
            case BlockMode.Write:
                share = FileShare.None;
                access = FileAccess.Write;
                break;
            case BlockMode.ReadWrite:
                share = FileShare.None;
                access = FileAccess.ReadWrite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        Stream stream;
        var retry = 0;

    tryOpen:
        try
        {
            stream = file.Open(FileMode.OpenOrCreate, access, share);
        }
        catch (IOException)
        {
            // IN USE
            Thread.Sleep(100);

            if (++retry > 50) throw;

            goto tryOpen;
        }


        return new BinaryBlock(stream, mode);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}