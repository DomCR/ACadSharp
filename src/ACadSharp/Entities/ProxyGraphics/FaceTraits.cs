using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

/// <summary>
/// Represents the traits of a face in proxy graphics, including its normals.
/// </summary>
public class FaceTraits : PrimitiveTraits
{
	/// <summary>
	/// Gets the list of normals associated with the face.
	/// </summary>
	public List<XYZ> Normals { get; } = new();
}
