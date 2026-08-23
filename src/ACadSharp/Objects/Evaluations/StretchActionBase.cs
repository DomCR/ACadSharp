using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(null, true)]
public abstract class StretchActionBase : BlockAction
{
	/// <summary>
	/// Gets or sets the angle offset for the stretch action.
	/// </summary>
	public virtual double AngleOffset { get; set; }

	public virtual List<XY> Boundary { get; protected set; } = new();

	/// <summary>
	/// Gets or sets the distance multiplier for the stretch action.
	/// </summary>
	public virtual double DistanceMultiplier { get; set; }

	public List<StretchEntityBind> StretchBindings { get; protected set; } = new();

	public List<StretchNode> StretchNodes { get; protected set; } = new();
}