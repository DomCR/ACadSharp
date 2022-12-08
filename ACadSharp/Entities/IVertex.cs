using CSMath;

namespace ACadSharp.Entities
{
	public interface IVertex
	{
		IVector Location { get; }

		double Bulge { get; }
	}
}
