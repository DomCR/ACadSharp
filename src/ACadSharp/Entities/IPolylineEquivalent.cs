namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents an object that can be converted to a 3D polyline representation.
	/// </summary>
	/// <remarks>This interface defines a contract for objects that can produce a <see cref="Polyline3D"/> instance,
	/// which is a 3D representation of the object's geometry. Implementers of this interface should ensure that the
	/// returned polyline accurately represents the object's shape.</remarks>
	public interface IPolylineEquivalent
	{
		/// <summary>
		/// Converts the current object to a 3D polyline representation.
		/// </summary>
		/// <param name="precision">The level of precision used for the conversion. Higher values result in a more detailed polyline. The value must be
		/// between 0 and 255, inclusive. The default is <see cref="byte.MaxValue"/>.</param>
		/// <returns>A <see cref="Polyline3D"/> representing the 3D polyline approximation of the current object.</returns>
		Polyline3D ToPolyline(int precision = byte.MaxValue);
	}
}
