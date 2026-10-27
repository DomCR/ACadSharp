using ACadSharp.Classes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent;

public class TchMText : TchEntity
{
	/// <summary>
	/// Gets the height the text is drawn at, in drawing units.
	/// </summary>
	/// <remarks>
	/// Not-a-number when the object carries no readable height.
	/// </remarks>
	public double Height { get; set; } = double.NaN;

	/// <summary>
	/// Gets the insertion point of the text, in drawing coordinates.
	/// </summary>
	public XYZ InsertionPoint { get; set; } = XYZ.Zero;

	/// <summary>
	/// Gets the line spacing factor of a multiline text, not-a-number for the other classes.
	/// </summary>
	public double LineSpacingFactor { get; set; } = double.NaN;

	/// <summary>
	/// Gets the text written under a drawing name, its scale such as <c>1:100</c>.
	/// </summary>
	public string Subtitle { get; set; }

	/// <summary>
	/// Gets the height the subtitle of a drawing name is drawn at.
	/// </summary>
	public double SubtitleHeight { get; set; } = double.NaN;

	/// <summary>
	/// Gets the text itself.
	/// </summary>
	/// <remarks>
	/// A multiline text keeps its line breaks, so the value can contain <c>\r\n</c>.
	/// </remarks>
	public string Value { get; set; }

	/// <summary>
	/// Gets the width a multiline text wraps at, not-a-number for the other classes.
	/// </summary>
	public double Width { get; set; } = double.NaN;

	public TchMText(DxfClass dxfClass)
		: base(dxfClass)
	{
	}

	public TchMText() : base(null)
	{
	}

	public override void ApplyTransform(Transform transform)
	{
		throw new NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new NotImplementedException();
	}

	public override IEnumerable<Entity> ToStandardEntities()
	{
		throw new NotImplementedException();
	}
}
