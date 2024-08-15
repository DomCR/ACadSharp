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

		/// <summary>
		/// Flag to mark the classes that don't contain any properties by itself, they are only a base for the subclasses
		/// </summary>
		public bool IsEmpty { get; }

		public DxfSubClassAttribute(string className)
		{
			this.ClassName = className;
		}

		public DxfSubClassAttribute(string className, bool isEmpty) : this(className)
		{
			this.IsEmpty = isEmpty;
		}
	}
}
