using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents the value set of a dynamic block parameter, which defines the allowed values for the parameter.
/// </summary>
public class ParameterValueSet
{
	/// <summary>
	/// Discrete list of allowed values.
	/// </summary>
	public List<double> AllowedValues { get; } = new List<double>();

	/// <summary>
	/// Distance increment between two allowed values.
	/// </summary>
	public double Increment { get; set; }

	/// <summary>
	/// Maximum allowed value.
	/// </summary>
	public double Maximum { get; set; }

	/// <summary>
	/// Minimum allowed value.
	/// </summary>
	public double Minimum { get; set; }

	/// <summary>
	/// Gets or sets the type of the value set, which can be a discrete list, a range, or a combination of both.
	/// </summary>
	public ParameterValueSetType Type { get; set; }
}