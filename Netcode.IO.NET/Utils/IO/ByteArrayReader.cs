using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetcodeIO.NET.Utils.IO
{
	/// <summary>
	/// Helper class for a quick non-allocating way to read or write from/to temporary byte arrays as streams
	/// </summary>
	public class ByteArrayReaderWriter : IDisposable
	{
		protected static Queue<ByteArrayReaderWriter> readerPool = new Queue<ByteArrayReaderWriter>();

		/// <summary>
		/// Get a reader/writer for the given byte array
		/// </summary>
		public static ByteArrayReaderWriter Get(byte[] byteArray)
		{
			ByteArrayReaderWriter reader = null;

			if (readerPool.Count > 0)
			{
				reader = readerPool.Dequeue();
			}
			else
			{
				reader = new ByteArrayReaderWriter();
			}

			reader.SetStream(byteArray);
			return reader;
		}

		/// <summary>
		/// Release a reader/writer to the pool
		/// </summary>
		public static void Release(ByteArrayReaderWriter reader)
		{
			readerPool.Enqueue(reader);
		}

		public long ReadPosition
		{
			get { return readStream.Position; }
		}

		public long WritePosition
		{
			get { return writeStream.Position; }
		}

		protected ByteStream readStream;
		protected BinaryReader reader;

		protected ByteStream writeStream;
		protected BinaryWriter writer;

		public ByteArrayReaderWriter()
		{
			this.readStream = new ByteStream();
			this.reader = new BinaryReader(readStream);

			this.writeStream = new ByteStream();
			this.writer = new BinaryWriter(writeStream);
		}

		public void SetStream(byte[] byteArray)
		{
			this.readStream.SetStreamSource(byteArray);
			this.writeStream.SetStreamSource(byteArray);
		}

		public void Write(byte val) { writer.Write(val); }
		public void Write(byte[] val) { writer.Write(val); }
		public void Write(char val) { writer.Write(val); }
		public void Write(char[] val) { writer.Write(val); }
		public void Write(string val) { writer.Write(val); }
		public void Write(short val) { writer.Write(val); }
		public void Write(int val) { writer.Write(val); }
		public void Write(long val) { writer.Write(val); }
		public void Write(ushort val) { writer.Write(val); }
		public void Write(uint val) { writer.Write(val); }
		public void Write(ulong val) { writer.Write(val); }

		public void WriteASCII(char[] chars)
		{
			for (int i = 0; i < chars.Length; i++)
			{
				byte asciiCode = (byte)(chars[i] & 0xFF);
				Write(asciiCode);
			}
		}

		public void WriteASCII(string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				byte asciiCode = (byte)(str[i] & 0xFF);
				Write(asciiCode);
			}
		}

		public void WriteBuffer(byte[] buffer, int length)
		{
			for (int i = 0; i < length; i++)
				Write(buffer[i]);
		}

		public byte ReadByte() { return reader.ReadByte(); }
		public byte[] ReadBytes(int length) { return reader.ReadBytes(length); }
		public char ReadChar() { return reader.ReadChar(); }
		public char[] ReadChars(int length) { return reader.ReadChars(length); }
		public string ReadString() { return reader.ReadString(); }
		public short ReadInt16() { return reader.ReadInt16(); }
		public int ReadInt32() { return reader.ReadInt32(); }
		public long ReadInt64() { return reader.ReadInt64(); }
		public ushort ReadUInt16() { return reader.ReadUInt16(); }
		public uint ReadUInt32() { return reader.ReadUInt32(); }
		public ulong ReadUInt64() { return reader.ReadUInt64(); }

		public void ReadASCIICharsIntoBuffer(char[] buffer, int length)
		{
			for (int i = 0; i < length; i++)
				buffer[i] = (char)ReadByte();
		}

		public void ReadBytesIntoBuffer(byte[] buffer, int length)
		{
			for (int i = 0; i < length; i++)
				buffer[i] = ReadByte();
		}

		public void Dispose()
		{
			Release(this);
		}
	}
}
