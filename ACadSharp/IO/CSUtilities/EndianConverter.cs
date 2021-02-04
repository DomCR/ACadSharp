using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtilities.Converters
{
	internal abstract class EndianConverter : IEndianConverter
	{
		protected readonly IEndianConverter m_converter;
		public EndianConverter() { }
		protected EndianConverter(IEndianConverter converter)
		{
			m_converter = converter;
		}
		public byte[] GetBytes(char value) => m_converter.GetBytes(value);
		public byte[] GetBytes(short value) => m_converter.GetBytes(value);
		public byte[] GetBytes(ushort value) => m_converter.GetBytes(value);
		public byte[] GetBytes(int value) => m_converter.GetBytes(value);
		public byte[] GetBytes(uint value) => m_converter.GetBytes(value);
		public byte[] GetBytes(long value) => m_converter.GetBytes(value);
		public byte[] GetBytes(ulong value) => m_converter.GetBytes(value);
		public byte[] GetBytes(double value) => m_converter.GetBytes(value);
		public byte[] GetBytes(float value) => m_converter.GetBytes(value);
		public char ToChar(byte[] bytes) => m_converter.ToChar(bytes);
		public short ToInt16(byte[] bytes) => m_converter.ToInt16(bytes);
		public ushort ToUInt16(byte[] bytes) => m_converter.ToUInt16(bytes);
		public int ToInt32(byte[] bytes) => m_converter.ToInt32(bytes);
		public uint ToUInt32(byte[] bytes) => m_converter.ToUInt32(bytes);
		public long ToInt64(byte[] bytes) => m_converter.ToInt64(bytes);
		public ulong ToUInt64(byte[] bytes) => m_converter.ToUInt64(bytes);
		public double ToDouble(byte[] bytes) => m_converter.ToDouble(bytes);
		public float ToSingle(byte[] bytes) => m_converter.ToSingle(bytes);
		public char ToChar(byte[] bytes, int offset) => m_converter.ToChar(bytes, offset);
		public short ToInt16(byte[] bytes, int offset) => m_converter.ToInt16(bytes, offset);
		public ushort ToUInt16(byte[] bytes, int offset) => m_converter.ToUInt16(bytes, offset);
		public int ToInt32(byte[] bytes, int offset) => m_converter.ToInt32(bytes, offset);
		public uint ToUInt32(byte[] bytes, int offset) => m_converter.ToUInt32(bytes, offset);
		public long ToInt64(byte[] bytes, int offset) => m_converter.ToInt64(bytes, offset);
		public ulong ToUInt64(byte[] bytes, int offset) => m_converter.ToUInt64(bytes, offset);
		public double ToDouble(byte[] bytes, int offset) => m_converter.ToDouble(bytes, offset);
		public float ToSingle(byte[] bytes, int offset) => m_converter.ToSingle(bytes, offset);
	}
}
