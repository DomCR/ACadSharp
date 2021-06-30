namespace ACadSharp.IO.Utils.Converters
{
	/// <summary>
	/// Represents a endian byte converter.
	/// </summary>
	internal interface IEndianConverter
	{
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 2.</returns>
		byte[] GetBytes(char value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 2.</returns>
		byte[] GetBytes(short value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 2.</returns>
		byte[] GetBytes(ushort value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 4.</returns>
		byte[] GetBytes(int value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 4.</returns>
		byte[] GetBytes(uint value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 8.</returns>
		byte[] GetBytes(long value);
		/// <summary>
		/// Returns the specified value as an array of bytes
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 8.</returns>
		byte[] GetBytes(ulong value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 8.</returns>
		byte[] GetBytes(double value);
		/// <summary>
		/// Returns the specified value as an array of bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>An array of bytes with length 4.</returns>
		byte[] GetBytes(float value);
		/// <summary>
		/// Converts the specified bytes to a <see cref="char" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		char ToChar(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="short" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		short ToInt16(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="ushort" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		ushort ToUInt16(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="int" />.
		/// </summary>
		int ToInt32(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="uint" />.
		/// </summary>
		uint ToUInt32(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="long" />.
		/// </summary>
		long ToInt64(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="ulong" />.
		/// </summary>
		ulong ToUInt64(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="double" />.
		/// </summary>
		double ToDouble(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to an <see cref="float" />.
		/// </summary>
		float ToSingle(byte[] arr);
		/// <summary>
		/// Converts the specified bytes to a <see cref="char" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		char ToChar(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="short" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		short ToInt16(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="ushort" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		ushort ToUInt16(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="int" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		int ToInt32(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="uint" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		uint ToUInt32(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="long" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		long ToInt64(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="ulong" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		ulong ToUInt64(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="double" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		double ToDouble(byte[] arr, int offset);
		/// <summary>
		/// Converts the specified bytes to an <see cref="float" />.
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="offset"></param>
		float ToSingle(byte[] arr, int offset);
	}
}