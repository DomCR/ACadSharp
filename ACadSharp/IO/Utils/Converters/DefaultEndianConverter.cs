using System;

namespace ACadSharp.IO.Utils.Converters
{
	internal class DefaultEndianConverter : IEndianConverter
	{
		public byte[] GetBytes(char value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(short value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(ushort value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(int value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(uint value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(long value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(ulong value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(double value) => BitConverter.GetBytes(value);
		public byte[] GetBytes(float value) => BitConverter.GetBytes(value);
		public char ToChar(byte[] arr) => BitConverter.ToChar(arr, 0);
		public short ToInt16(byte[] arr) => BitConverter.ToInt16(arr, 0);
		public ushort ToUInt16(byte[] arr) => BitConverter.ToUInt16(arr, 0);
		public int ToInt32(byte[] arr) => BitConverter.ToInt32(arr, 0);
		public uint ToUInt32(byte[] arr) => BitConverter.ToUInt32(arr, 0);
		public long ToInt64(byte[] arr) => BitConverter.ToInt64(arr, 0);
		public ulong ToUInt64(byte[] arr) => BitConverter.ToUInt64(arr, 0);
		public double ToDouble(byte[] arr) => BitConverter.ToDouble(arr, 0);
		public float ToSingle(byte[] arr) => BitConverter.ToSingle(arr, 0);
		public char ToChar(byte[] arr, int length) => BitConverter.ToChar(arr, length);
		public short ToInt16(byte[] arr, int length) => BitConverter.ToInt16(arr, length);
		public ushort ToUInt16(byte[] arr, int length) => BitConverter.ToUInt16(arr, length);
		public int ToInt32(byte[] arr, int length) => BitConverter.ToInt32(arr, length);
		public uint ToUInt32(byte[] arr, int length) => BitConverter.ToUInt32(arr, length);
		public long ToInt64(byte[] arr, int length) => BitConverter.ToInt64(arr, length);
		public ulong ToUInt64(byte[] arr, int length) => BitConverter.ToUInt64(arr, length);
		public double ToDouble(byte[] arr, int length) => BitConverter.ToDouble(arr, length);
		public float ToSingle(byte[] arr, int length) => BitConverter.ToSingle(arr, length);
	}
}
