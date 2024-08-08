using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSUtilities.Extensions
{
	internal static class ByteExtensions
	{
		/// <summary>
		/// Convert a byte array into a hex string array.
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static string ToHexString(this IEnumerable<byte> array)
		{
			StringBuilder hex = new StringBuilder(array.Count() * 2);
			foreach (byte b in array)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString().ToUpper();
		}
	}
}
