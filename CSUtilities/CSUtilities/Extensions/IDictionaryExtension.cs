using System.Collections.Generic;

namespace CSUtilities.Extensions
{
	/// <summary>
	/// Estensions for <see cref="IDictionary{TKey, TValue}"/>
	/// </summary>
	public static class IDictionaryExtension
	{
#if NETFRAMEWORK
		/// <summary>
		/// Removes the value with the specified key from the <see cref="IDictionary{TKey, TValue}"/>, and copies the element to the value parameter
		/// </summary>
		public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
		{
			if (dictionary.TryGetValue(key, out value))
			{
				dictionary.Remove(key);
				return true;
			}

			return false;
		}
#endif
	}
}
