using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Common
{
	public static class DataFactory
	{
		public static IEnumerable<Type> GetTypes<T>()
		{
			var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");

			foreach (var item in d.GetTypes().Where(i => !i.IsAbstract && i.IsPublic))
			{
				if (item.IsSubclassOf(typeof(T)))
				{
					yield return (item);
				}
			}
		} 
	}
}
