using System;

namespace CSUtilities.Extensions
{
	internal static class ObjectExtensions
	{
		public delegate bool Check<T>(T obj);

		public static void ThrowIfNull(this object parameter)
		{
			if (parameter != null)
				return;

			throw new System.ArgumentNullException();
		}

		public static void ThrowIfNull(this object parameter, string paramName)
		{
			if (parameter != null)
				return;

			throw new System.ArgumentNullException(paramName);
		}

		public static void ThrowIfNull(this object parameter, string paramName, string message)
		{
			if (parameter != null)
				return;

			throw new System.ArgumentNullException(paramName, message);
		}

		public static void ThrowIf<T, E>(this T parameter, Check<T> check)
			where E : Exception, new()
		{
			if (check(parameter))
			{
				throw new E();
			}
		}

		public static void ThrowIf<T, E>(this T parameter, Check<T> check, string message)
			where E : Exception, new()
		{
			if (check(parameter))
			{
				throw Activator.CreateInstance(typeof(E), message) as E;
			}
		}
	}
}
