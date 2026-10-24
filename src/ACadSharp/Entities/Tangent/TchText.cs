using ACadSharp.Classes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent;

public static class TchDxfClasses
{
	/// <summary>
	/// The DXF name of the class that writes text.
	/// </summary>
	public const string Text = "TCH_TEXT";
	/// <summary>
	/// The DXF name of the class that writes multiline text.
	/// </summary>
	public const string MText = "TCH_MTEXT";
	/// <summary>
	/// The DXF name of the class that writes a drawing name.
	/// </summary>
	public const string DrawingName = "TCH_DRAWINGNAME";
}

/// <summary>
/// A Tianzheng text object.
/// </summary>
/// <remarks>
/// <para>
/// Covers the classes whose object is text: <c>TCH_TEXT</c>, <c>TCH_MTEXT</c> and
/// <c>TCH_DRAWINGNAME</c>. They are one type because they are read the same way — the object's
/// string stream is walked for the string that is content rather than a field — and
/// <see cref="TchEntity.DxfClass"/> still says which of them it was.
/// </para>
/// <para>
/// It is not a <see cref="TextEntity"/>. The bytes were written by <c>TDbText::dwgInFields</c>,
/// not by the <c>TEXT</c> reader, and treating it as one would mean the writers produced a
/// <c>TEXT</c> that was never in the drawing. <see cref="ToTextEntity"/> is how a caller asks for
/// one deliberately.
/// </para>
/// </remarks>
public class TchText : TchEntity
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
	/// Initialises a text from the class it was read as.
	/// </summary>
	public TchText(DxfClass dxfClass)
		: base(dxfClass)
	{
	}

	public TchText() : base(null)
	{
	}

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		this.InsertionPoint = transform.ApplyTransform(this.InsertionPoint);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// A text with no position has no extent, so the box is null rather than empty.
	/// </remarks>
	public override BoundingBox GetBoundingBox()
	{
		BoundingBox box = BoundingBox.Null;
		foreach (Entity entity in this.ToStandardEntities())
		{
			box = box.Merge(entity.GetBoundingBox());
		}

		return box;
	}

	/// <inheritdoc/>
	public override IEnumerable<Entity> ToStandardEntities()
	{
		TextEntity text = new TextEntity
		{
			InsertPoint = this.InsertionPoint,
			Value = this.Value ?? string.Empty,
		};

		if (!double.IsNaN(this.Height))
		{
			text.Height = this.Height;
		}

		text.MatchProperties(this);

		yield return text;
	}

	/// <summary>
	/// Says whether a Tianzheng class writes text, and so is read as a
	/// <see cref="TchText"/> rather than as an unlisted entity.
	/// </summary>
	/// <param name="dxfName">DXF name of the class.</param>
	/// <returns>True when the class writes text.</returns>
	internal static bool WritesText(string dxfName)
	{
		return dxfName == TchDxfClasses.Text || dxfName == TchDxfClasses.MText || dxfName == TchDxfClasses.DrawingName;
	}
}