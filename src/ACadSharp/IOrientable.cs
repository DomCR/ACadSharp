using CSMath;

namespace ACadSharp;

/// <summary>
/// Represents an object that has a three-dimensional orientation.
/// </summary>
public interface IOrientable
{
	/// <summary>
	/// Specifies the three-dimensional normal unit vector for the object.
	/// </summary>
	XYZ Normal { get; set; }
}