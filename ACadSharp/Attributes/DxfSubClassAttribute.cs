using System;

namespace ACadSharp.Attributes
{
	/// <summary>
	/// Mark the class as a dxf class equivalent
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class DxfSubClassAttribute : Attribute
	{
		/// <summary>
		/// Dxf class name
		/// </summary>
		public string ClassName { get; }

		public DxfSubClassAttribute(string className)
		{
			this.ClassName = className;
		}
	}
}
