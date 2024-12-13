using System.Collections.Generic;
using System.Linq;

namespace CSUtilities.Extensions
{
	/// <summary>
	/// Estensions for <see cref="IEnumerable{T}"/>
	/// </summary>
	internal static class IEnumerableExtensions
	{
		/// <summary>
		/// Return true if the collection is empty.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
		{
			return enumerable.GetEnumerator() == null;
		}

		/// <summary>
		/// Transforms an enumerable into a Queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
		{
			return new Queue<T>(enumerable);
		}

		/// <summary>
		/// Gets the element in an specific index or it's default value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="index"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryGet<T>(this IEnumerable<T> enumerable, int index, out T result)
		{
			if (enumerable.Count() < index)
			{
				result = default(T);
				return false;
			}

			result = enumerable.ElementAt(index);
			return true;
		}

		public static IEnumerable<T> RemoveLastEquals<T>(this IEnumerable<T> enumerable, T element)
		{
			List<T> lst = new List<T>(enumerable);
			while (lst.Last().Equals(element))
			{
				lst.RemoveAt(lst.Count - 1);
			}

			return lst;
		}
	}
}
