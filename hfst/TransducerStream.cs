using System;
using System.IO;

/// <summary>
/// A simple extension of BinaryReader to handle unsigned little-endian data.
/// </summary>
namespace hfst{
    public class TransducerStream : BinaryReader
{
    /// <summary>
    /// Invokes the BinaryReader constructor with a Stream argument.
    /// </summary>
    /// <param name="stream">Stream containing little-endian unsigned variables.</param>
    public TransducerStream(Stream stream) : base(stream) { }

    /// <summary>
    /// Reads the next two bytes as an unsigned little-endian short.
    /// </summary>
    /// <returns>An int representing the unsigned short.</returns>
    public int GetUShort()
    {
        byte byte1 = base.ReadByte();
        byte byte2 = base.ReadByte();
        return byte2 << 8 | byte1;
    }

    /// <summary>
    /// Reads the next four bytes as an unsigned little-endian int.
    /// </summary>
    /// <returns>A long representing the unsigned int.</returns>
    public long GetUInt()
    {
        byte byte1 = base.ReadByte();
        byte byte2 = base.ReadByte();
        byte byte3 = base.ReadByte();
        byte byte4 = base.ReadByte();
        return ((long)byte4 << 24) | ((long)byte3 << 16) | ((long)byte2 << 8) | byte1;
    }

    /// <summary>
    /// Reads four bytes, returns false if they're all zero and true otherwise.
    /// </summary>
    /// <returns>A boolean representing the underlying unsigned int.</returns>
    public bool GetBool()
    {
        return GetUInt() != 0;
    }
}
}