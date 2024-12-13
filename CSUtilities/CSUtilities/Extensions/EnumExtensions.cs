using CSUtilities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSUtilities.Extensions
{
#if PUBLIC
	public 
#else
	internal
#endif
	static class EnumExtensions
	{
		[Obsolete("Use Type.GetValues()")]
		public static IEnumerable<T> GetValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		[Obsolete("Use Type.GetNames()")]
		public static IEnumerable<string> GetNames<T>(this T value)
			where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>().Select(o => o.ToString());
		}

		public static T GetValueByName<T>(string name)
		{
			return Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault(o => o.ToString() == name);
		}

		/// <summary>
		/// Adds a flag value to enum
		/// </summary>
		public static T AddFlag<T>(this T value, T flag)
			where T : Enum
		{
			return (T)(object)((int)(object)value | (int)(object)flag);
		}

		/// <summary>
		/// Removes the flag value from enum
		/// </summary>
		public static T RemoveFlag<T>(this T value, T flag)
			where T : Enum
		{
			return (T)(object)((int)(object)value & ~(int)(object)flag);
		}

		/// <summary>
		/// Gets a string value for a particular enum value.
		/// Converts the string representation of the name or numeric value of one or more
		/// enumerated constants to an equivalent enumerated object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static T Parse<T>(this string value, bool ignoreCase = false)
			where T : Enum
		{
			return (T)Enum.Parse(typeof(T), value, ignoreCase);
		}

		/// <summary>
		/// Converts the string representation compared with the assigned <see cref="StringValueAttribute"/> value of one or more
		/// enumerated constants to an equivalent enumerated object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T ParseByStringValue<T>(this string value)
			where T : Enum
		{
			foreach (FieldInfo fi in typeof(T).GetFields())
			{
				if (!fi.TryGetAttribute(out StringValueAttribute att))
				{
					continue;
				}

				if (att.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					return (T)Enum.Parse(typeof(T), fi.Name);
				}
			}

			throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Converts the string representation compared with the assigned <see cref="StringValueAttribute"/> value of one or more
		/// enumerated constants to an equivalent enumerated object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryParseByStringValue<T>(this string value, out T result)
			where T : Enum
		{
			foreach (FieldInfo fi in typeof(T).GetFields())
			{
				if (!fi.TryGetAttribute(out StringValueAttribute att))
				{
					continue;
				}

				if (att.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					result = (T)Enum.Parse(typeof(T), fi.Name);
					return true;
				}
			}

			result = default(T);
			return false;
		}

		/// <summary>
		/// Converts the string representation of the name or numeric value of one or more
		/// enumerated constants to an equivalent enumerated object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static bool TryParse<T>(this string value, out T result, bool ignoreCase = false)
			where T : struct
		{
			return Enum.TryParse(value, ignoreCase, out result);
		}

		/// <summary>
		/// Gets a string value for a particular enum value
		/// </summary>
		/// <param name="value">enum value</param>
		/// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
		public static string GetStringValue<T>(this T value)
			where T : Enum
		{
			Type type = value.GetType();

			FieldInfo fi = type.GetField(value.ToString());
			return fi.GetCustomAttribute<StringValueAttribute>()?.Value;
		}
	}
}