using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace NetcodeIO.NET.Utils.IO
{
	/// <summary>
	/// A simple stream implementation for reading/writing from/to byte arrays which can be reused
	/// </summary>
	public class ByteStream : Stream
	{
		protected byte[] srcByteArray;

		public override long Position
		{
			get; set;
		}

		public override long Length
		{
			get
			{
				return srcByteArray.Length;
			}
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		/// <summary>
		/// Set a new byte array for this stream to read from
		/// </summary>
		public void SetStreamSource(byte[] sourceBuffer)
		{
			this.srcByteArray = sourceBuffer;
			this.Position = 0;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readBytes = 0;
			long pos = this.Position;
			long len = this.Length;
			for (int i = 0; i < count && pos < len; i++)
			{
				buffer[i + offset] = srcByteArray[pos++];
				readBytes++;
			}

			this.Position = pos;
			return readBytes;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			long pos = this.Position;
			for (int i = 0; i < count; i++)
				srcByteArray[pos++] = buffer[i + offset];

			this.Position = pos;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
				this.Position = offset;
			else if (origin == SeekOrigin.Current)
				this.Position += offset;
			else
				this.Position = this.Length - offset - 1;

			return this.Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
	}
}
