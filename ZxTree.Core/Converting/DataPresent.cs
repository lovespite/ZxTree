using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZxTree.Core.Models;

namespace ZxTree.Core.Converting;

public class DataPresent : IDisposable
{
    private readonly Memory<byte> _bytes;

    public ReadOnlyMemory<byte> Memory => _bytes;

    public DataType DataType { get; }

    public static DataPresent Empty => new(Array.Empty<byte>(), DataType.Unknown);

    private DataPresent(Memory<byte> bytes, DataType type)
    {
        _bytes = bytes;
        DataType = type;
    }

    public bool IsEmpty => _bytes.Length == 0;

    public void Dispose()
    {
        _bytes.Span.Clear();
    }

    public object ToDataObj()
    {
        if (IsEmpty) throw new Exception("Data is empty");

        switch (DataType)
        {
            case DataType.Text:
                return Encoding.UTF8.GetString(_bytes.Span);
            case DataType.Number:
                return BitConverter.ToInt64(_bytes.Span);
            case DataType.Boolean:
                return BitConverter.ToBoolean(_bytes.Span);
            case DataType.DateTime:
                return DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(_bytes.Span));
            case DataType.Bytes:
                return _bytes.ToArray();
            case DataType.Unknown:
            default:
                throw new NotSupportedException($"{DataType} is not supported");
        }
    }

    public T ToDataObj<T>() => (T)ToDataObj();

    public void CopyTo(Stream stream) => stream.Write(_bytes.Span);

    public static DataType QueryType(Type t)
    {

        if (t == typeof(string)) return DataType.Text;
        if (t == typeof(long) || t == typeof(int)) return DataType.Number;
        if (t == typeof(bool)) return DataType.Boolean;
        if (t == typeof(DateTimeOffset)) return DataType.DateTime;
        if (t == typeof(byte[])) return DataType.Bytes;
        return DataType.Unknown;
    }

    public static DataPresent FromText(string text) => new(Encoding.UTF8.GetBytes(text), DataType.Text);
    public static DataPresent FromNumber(long number) => new(BitConverter.GetBytes(number), DataType.Number);
    public static DataPresent FromBoolean(bool boolean) => new(BitConverter.GetBytes(boolean), DataType.Boolean);
    public static DataPresent FromDateTime(DateTimeOffset dateTime) =>
        new(BitConverter.GetBytes(dateTime.ToUnixTimeMilliseconds()), DataType.DateTime);
    public static DataPresent FromBytes(byte[] bytes) => new(bytes, DataType.Bytes);

    public static DataPresent ConsumeStream(Stream stream, DataType type, int length = 0)
    {
        if (length <= 0 || length > stream.Length) length = (int)stream.Length;

        var buffer = new byte[length];
        var read = stream.Read(buffer, 0, length);
        Debug.Assert(read == length);
        return new DataPresent(buffer, type);
    }

    public static implicit operator ReadOnlySpan<byte>(DataPresent dataPresent) => dataPresent._bytes.Span;
}