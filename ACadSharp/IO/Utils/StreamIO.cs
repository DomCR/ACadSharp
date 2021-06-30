using ACadSharp.IO.Utils.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.Utils
{
	/// <summary>
	/// Utility class to read different data from a stream.
	/// </summary>
	internal class StreamIO : IDisposable
	{
		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		public virtual long Position
		{
			get => m_stream.Position;
			set => m_stream.Position = value;
		}
		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		public virtual long Length => m_stream.Length;
		public Stream Stream { get { return m_stream; } }
		//*******************************************************************
		protected Stream m_stream = null;
		//*******************************************************************
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		/// <param name="filename">File to read/write.</param>
		public StreamIO(string filename) : this(filename, FileMode.Open, FileAccess.Read) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		/// <param name="filename">File to read/write.</param>
		/// <param name="mode"></param>
		/// <param name="access"></param>
		public StreamIO(string filename, FileMode mode, FileAccess access)
		{
			m_stream = File.Open(filename, mode, access);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="createCopy"></param>
		/// <param name="resetPosition"></param>
		public StreamIO(Stream stream, bool createCopy, bool resetPosition)
		{
			long position = stream.Position;

			//Check if supports seeking
			if (!stream.CanSeek || createCopy)
			{
				stream.Position = 0;
				//Create a copy of the stream to allow seeking
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);
				m_stream = new MemoryStream(buffer);

				if (resetPosition)
					//Reset the position to the begining
					m_stream.Position = 0L;
			}
			else
				m_stream = stream;

			stream.Position = position;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		public StreamIO(Stream stream, bool createCopy) : this(stream, createCopy, false) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		public StreamIO(Stream stream) : this(stream, false, false) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="StreamIO" /> class.
		/// </summary>
		public StreamIO(byte[] arr) : this(new MemoryStream(arr)) { }
		//*******************************************************************
		/// <summary>
		/// Get an array of bytes given an offset, before the operation the position is set to 0.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		/// <remarks>This operation don't advance the positon.</remarks>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public byte[] GetBytes(int offset, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException("Length cannot be negative.");

			//Save the current position
			long save = Position;
			//Set the position to the begining
			Position = offset;

			byte[] buffer = ReadBytes(length);
			//if (this.m_stream.Read(buffer, offset, length) < length)
			//	throw new EndOfStreamException();

			//Reset the position
			Position = save;

			return buffer;
		}
		/// <summary>
		/// Look into a byte without moving the position of the stream.
		/// </summary>
		/// <returns></returns>
		public byte LookByte()
		{
			byte b = LookBytes(1)[0];
			return b;
		}
		/// <summary>
		/// Look into an array of bytes without moving the position of the stream.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public byte[] LookBytes(int count)
		{
			byte[] bs = ReadBytes(count);
			Position -= count;
			return bs;
		}
		/// <summary>
		/// Read a single byte form the stream.
		/// </summary>
		/// <returns></returns>
		public virtual byte ReadByte()
		{
			byte[] arr = new byte[1];
			byte b = m_stream.Read(arr, 0, 1) == 1 ?
				arr[0] : throw new EndOfStreamException();

			return b;
		}
		/// <summary>
		/// Read n bytes at the stream position.
		/// </summary>
		/// <remarks>
		/// Override this method to change the reading system of the whole the class.
		/// </remarks>
		/// <param name="length"></param>
		/// <returns></returns>
		public virtual byte[] ReadBytes(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException("Length cannot be negative.");

			byte[] buffer = new byte[length];

			if (m_stream.Read(buffer, 0, length) < length)
				throw new EndOfStreamException();

			return buffer;
		}
		/// <summary>
		/// Read a <see cref="short"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public short ReadShort()
		{
			return ReadShort<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="short"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public short ReadShort<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(2);
			return converter.ToInt16(buffer);
		}
		/// <summary>
		/// Read a <see cref="ushort"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public ushort ReadUShort()
		{
			return ReadUShort<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="ushort"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public ushort ReadUShort<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(2);
			return converter.ToUInt16(buffer);
		}
		/// <summary>
		/// Read a <see cref="int"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public int ReadInt()
		{
			return ReadInt<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="int"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public int ReadInt<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(4);
			return converter.ToInt32(buffer);
		}
		/// <summary>
		/// Read a <see cref="uint"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public uint ReadUInt()
		{
			return ReadUInt<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="uint"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public uint ReadUInt<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(4);
			return converter.ToUInt32(buffer);
		}
		/// <summary>
		/// Read a <see cref="float"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public float ReadSingle()
		{
			return ReadSingle<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="float"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public float ReadSingle<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(4);
			return converter.ToSingle(buffer);
		}
		/// <summary>
		/// Read a <see cref="double"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble()
		{
			return ReadDouble<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="double"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public double ReadDouble<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(8);
			return converter.ToDouble(buffer);
		}
		/// <summary>
		/// Read a <see cref="long"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public long ReadLong()
		{
			return ReadLong<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="long"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public long ReadLong<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(8);
			return converter.ToInt64(buffer);
		}
		/// <summary>
		/// Read a <see cref="ulong"/> value form the stream.
		/// </summary>
		/// <returns></returns>
		public ulong ReadULong()
		{
			return ReadULong<DefaultEndianConverter>();
		}
		/// <summary>
		/// Read a <see cref="ulong"/> value form the stream.
		/// </summary>
		/// <typeparam name="T">Endian converter to process the bytes.</typeparam>
		/// <returns></returns>
		public ulong ReadULong<T>() where T : IEndianConverter, new()
		{
			T converter = new T();

			byte[] buffer = ReadBytes(8);
			return converter.ToUInt64(buffer);
		}
		/// <summary>
		/// Read a string from the stream using the default encoding.
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public string ReadString(int length)
		{
			return ReadString(length, Encoding.Default);
		}
		/// <summary>
		/// Read a string from the stream.
		/// </summary>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public string ReadString(int length, Encoding encoding)
		{
			if (length == 0)
				return string.Empty;

			byte[] numArray = ReadBytes(length);
			return encoding.GetString(numArray);
		}
		/// <inheritdoc/>
		public void Dispose()
		{
			m_stream.Dispose();
		}
	}
}
