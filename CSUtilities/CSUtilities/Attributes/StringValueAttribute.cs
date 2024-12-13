using System;

namespace CSUtilities.Attributes
{
	/// <summary>
	/// Simple attribute class for storing String Values
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class StringValueAttribute : Attribute
	{
		/// <summary>
		/// Gets the value
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="value">Value.</param>
		public StringValueAttribute(string value)
		{
			this.Value = value;
		}
	}
}
