using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSUtilities.Extensions
{
	/// <summary>
	/// Queue utility extensions.
	/// </summary>
	internal static class QueueExtensions
	{
		/// <summary>
		/// Dequeue an element in a avoiding the exceptions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="q"></param>
		/// <returns>the last element or the default value for the type.</returns>
		public static T TryDequeue<T>(this Queue<T> q)
		{
			if (!q.Any())
			{
				return default;
			}
			else
			{
				return q.Dequeue();
			}
		}
		/// <summary>
		/// Dequeue an element in a avoiding the exceptions.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="q"></param>
		/// <param name="element"></param>
		/// <returns>If the operation has succeded.</returns>
		public static bool TryDequeue<T>(this Queue<T> q, out T element)
		{
			if (!q.Any())
			{
				element = default;
				return false;
			}
			else
			{
				element = q.Dequeue();
				return true;
			}
		}
	}
}
