using CSMath;

namespace ACadSharp.Entities.Mechanical;

/// <summary>
/// Represents a mechanical entity.
/// </summary>
public abstract class MechanicalEntity : Entity
{

	/// <summary>
	/// Gets or sets the position of the entity.
	/// </summary>
	public XYZ Position { get; set; }
}
