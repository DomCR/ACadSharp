using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Extensions
{
	public static class CadObjectExtensions
	{
		public static T CloneTyped<T>(this T obj)
			where T : CadObject
		{
			return (T)obj.Clone();
		}
	}
}
