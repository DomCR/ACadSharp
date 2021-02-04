using System;

namespace ACadSharp.Attributes
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	sealed class DxfSubClassEntityAttribute : Attribute
	{
		public string ClassName { get; }
		public DxfSubClassEntityAttribute(string className)
		{
			ClassName = className;
		}
	}
}
