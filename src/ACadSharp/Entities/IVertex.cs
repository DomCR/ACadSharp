using CSMath;

namespace ACadSharp.Entities
{
	public interface IVertex
	{
		/// <summary>
		/// Location point (in OCS when 2D, and WCS when 3D)
		/// </summary>
		IVector Location { get; }

		/// <summary>
		/// The bulge is the tangent of one fourth the included angle for an arc segment, made negative if the arc goes clockwise from the start point to the endpoint.A bulge of 0 indicates a straight segment, and a bulge of 1 is a semicircle
		/// </summary>
		double Bulge { get; }
	}
}
