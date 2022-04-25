using ACadSharp.Attributes;
using System;
using System.Reflection;

namespace ACadSharp
{
	public class DxfProperty : DxfPropertyBase<DxfCodeValueAttribute>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <param name="property"></param>
		/// <exception cref="ArgumentException"></exception>
		public DxfProperty(PropertyInfo property) : base(property) { }
	}
}
