using System;

namespace ACadSharp.Attributes
{
	/// <summary>
	/// Mark the class as a dxf class equivalent
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class DxfNameAttribute : Attribute
	{
		/// <summary>
		/// Dxf name
		/// </summary>
		public string Name { get; }

		public DxfNameAttribute(string name)
		{
			this.Name = name;
		}
	}
}
