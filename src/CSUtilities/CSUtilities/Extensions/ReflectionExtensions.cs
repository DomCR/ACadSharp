using System;
using System.Linq;
using System.Reflection;

namespace CSUtilities.Extensions
{
	internal static class ReflectionExtensions
	{
		public static PropertyInfo GetPropertyByName(this Type type, string name)
		{
			return type.GetProperties().FirstOrDefault(o => o.Name == name);
		}

		/// <summary>
		/// Checks if the type implements a specific interface
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static bool HasInterface<T>(this Type type)
			where T : class
		{
			Type it = typeof(T);

			if (!it.IsInterface)
				throw new ArgumentException("Generic type is not an interface");

			return type.GetInterface(it.FullName) != null;
		}

		/// <summary>
		/// Tries to retrive a custom attribute of a specific type to a specified type member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="member"></param>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool TryGetAttribute<T>(this MemberInfo member, out T attribute)
			 where T : Attribute
		{
			attribute = member.GetCustomAttribute<T>();
			return attribute != null;
		}
	}
}
