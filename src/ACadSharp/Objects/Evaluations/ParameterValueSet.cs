using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents the value set of a dynamic block parameter, which defines the allowed values for the parameter.
/// </summary>
public class ParameterValueSet
{
	/// <summary>
	/// Distance increment between two allowed values.
	/// </summary>
	[DxfCodeValue(143)]
	public double Increment { get; set; }

	/// <summary>
	/// Maximum allowed value.
	/// </summary>
	[DxfCodeValue(142)]
	public double Maximum { get; set; }

	/// <summary>
	/// Minimum allowed value.
	/// </summary>
	[DxfCodeValue(141)]
	public double Minimum { get; set; }

	public ParameterValueSetType Type { get; set; }

	/// <summary>
	/// Discrete list of allowed values.
	/// </summary>
	public List<double> AllowedValues { get; } = new List<double>();
}