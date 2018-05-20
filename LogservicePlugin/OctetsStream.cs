using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LogservicePlugin
{
    public class OctetsStream
    {
        public bool BigEdian = true;
        public uint OpCode = 0;
        public byte[] Data
        {
            get => stream.ToArray();
        }

        private MemoryStream stream = new MemoryStream();

        public OctetsStream()
        {

        }

        public OctetsStream(byte[] data, RemoveHeaderType rht = RemoveHeaderType.None, bool BigEdian = true)
        {
            stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            this.BigEdian = BigEdian;
            switch (rht)
            {
                case RemoveHeaderType.Auto:
                    stream.Seek(2, SeekOrigin.Begin);
                    if (stream.ReadByte() == 8)
                    {
                        stream.Seek(12, SeekOrigin.Begin);
                    }
                    else
                    {
                        stream.Seek(11, SeekOrigin.Begin);
                    }
                    break;
                case RemoveHeaderType.Read:
                    ReadCUInt();
                    ReadCUInt();
                    break;
            }
        }

        #region Read
        public byte[] ReadBytes(int len)
        {
            byte[] data = new byte[len];
            stream.Read(data, 0, len);
            return data;
        }

        public byte ReadByte() => ReadBytes(1)[0];

        public uint ReadCUInt()
        {
            bool e = BigEdian;
            BigEdian = true;
            byte b = ReadByte();
            uint value = 0;
            switch (b & 0xE0)
            {
                case 0xE0:
                    value = (uint)ReadInt32();
                    break;
                case 0xC0:
                    stream.Seek(-1, SeekOrigin.Current);
                    value = (uint)ReadInt32() & 0x3FFFFFFF;
                    break;
                case 0x80:
                case 0xA0:
                    stream.Seek(-1, SeekOrigin.Current);
                    value = (uint)ReadInt16() & 0x7FFF;
                    break;
                default:
                    value =  b;
                    break;
            }
            BigEdian = e;
            return value;
        }

        public short ReadInt16()
        {
            byte[] buffer = ReadBytes(2);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public ushort ReadUInt16()
        {
            byte[] buffer = ReadBytes(2);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public int ReadInt32()
        {
            byte[] buffer = ReadBytes(4);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public uint ReadUInt32()
        {
            byte[] buffer = ReadBytes(4);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public long ReadInt64()
        {
            byte[] buffer = ReadBytes(8);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public ulong ReadUInt64()
        {
            byte[] buffer = ReadBytes(8);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public float ReadFloat()
        {
            byte[] buffer = ReadBytes(4);
            if (BigEdian)
                Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public string ReadString(OctetsString type = OctetsString.Octet)
        {
            uint len = ReadCUInt();
            byte[] buffer = ReadBytes((int)len);
            switch (type)
            {
                case OctetsString.GBK:
                    return Encoding.GetEncoding("GB2312").GetString(buffer);
                case OctetsString.ASCII:
                    return Encoding.ASCII.GetString(buffer);
                case OctetsString.Octet:
                    return Encoding.Default.GetString(buffer);
                case OctetsString.Unicode:
                    return Encoding.Unicode.GetString(buffer);
                case OctetsString.UTF8:
                    return Encoding.UTF8.GetString(buffer);
                default:
                    return "";
            }
        }
        #endregion

        #region Write
        public void WriteBytes(byte[] data) => stream.Write(data, 0, data.Length);

        public void WriteByte(byte b) => WriteBytes(new byte[] { b });

        public byte[] CUInt(uint v)
        {
            if (v < 64)
            {
                return new byte[] { (byte)v };
            }
            if (v < 16384)
            {
                byte[] b = BitConverter.GetBytes((ushort)(v + 0x8000));
                Array.Reverse(b);
                return b;
            }
            if (v < 536870912)
            {
                byte[] b = BitConverter.GetBytes(v + 536870912);
                Array.Reverse(b);
                return b;
            }
            return new byte[0] { };
        }

        public void WriteCUint(uint v) => WriteBytes(CUInt(v));

        public void WriteInt16(short v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteUInt16(ushort v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteInt32(int v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteUInt32(uint v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteInt64(long v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteUInt64(ulong v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            if (BigEdian)
                Array.Reverse(buffer);
            WriteBytes(buffer);
        }

        public void WriteString(string v, OctetsString type = OctetsString.Octet)
        {
            uint len = ReadCUInt();
            byte[] buffer = new byte[0];
            switch (type)
            {
                case OctetsString.GBK:
                    buffer = Encoding.GetEncoding("GB2312").GetBytes(v);
                    break;
                case OctetsString.ASCII:
                    buffer = Encoding.ASCII.GetBytes(v);
                    break;
                case OctetsString.Octet:
                    buffer = Encoding.Default.GetBytes(v);
                    break;
                case OctetsString.Unicode:
                    buffer = Encoding.Unicode.GetBytes(v);
                    break;
                case OctetsString.UTF8:
                    buffer = Encoding.UTF8.GetBytes(v);
                    break;
            }
            WriteInt32(buffer.Length);
            WriteBytes(buffer);
        }
        #endregion

        public byte[] Send(IPEndPoint ip, bool Recive = false, bool Empty = false)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(CUInt(OpCode));
            buffer.AddRange(CUInt((uint)Data.Length));
            buffer.AddRange(Data);
            Socket s = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                byte[] empty = new byte[32768];
                s.Connect(ip);
                if (Empty)
                {
                    s.Receive(empty);
                }
                s.Send(buffer.ToArray());
                if (Recive)
                {
                    byte[] data = new byte[32768];
                    s.Receive(data);
                    return data;
                }
                s.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return new byte[0];
        }
    }

    public enum OctetsString
    {
        GBK,
        Unicode,
        ASCII,
        UTF8,
        Octet
    }

    public enum RemoveHeaderType
    {
        None,
        Auto,
        Read
    }
}
