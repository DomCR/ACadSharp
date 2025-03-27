using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Objects with geometric properties that can be processed in a drawing.
	/// </summary>
	public interface IGeometricEntity
	{
		/// <summary>
		/// Gets the bounding box aligned with the axis XYZ that occupies this entity.
		/// </summary>
		/// <returns>The bounding box where this entity resides.</returns>
		BoundingBox GetBoundingBox();
	}
}
