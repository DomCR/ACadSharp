using ACadSharp.Classes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
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
		/// Initialises a text from the class it was read as.
		/// </summary>
		internal TchText(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the insertion point of the text, in drawing coordinates.
		/// </summary>
		/// <remarks>
		/// Zero when <see cref="HasPosition"/> is false, which happens when the object carries
		/// nothing that reads as a position.
		/// </remarks>
		public XYZ InsertionPoint { get; internal set; } = XYZ.Zero;

		/// <summary>
		/// Gets a value indicating whether a position was found.
		/// </summary>
		public bool HasPosition { get; internal set; }

		/// <summary>
		/// Gets the text itself.
		/// </summary>
		/// <remarks>
		/// A multiline text keeps its line breaks, so the value can contain <c>\r\n</c>.
		/// </remarks>
		public string Value { get; internal set; }

		/// <summary>
		/// Gets the height the text is drawn at, in drawing units.
		/// </summary>
		/// <remarks>
		/// Not-a-number when the object carries no readable height, in which case
		/// <see cref="ToTextEntity"/> leaves the standard text's own default in place.
		/// </remarks>
		public double Height { get; internal set; } = double.NaN;

		/// <summary>
		/// Builds the standard text this object stands for.
		/// </summary>
		/// <remarks>
		/// The result is a new <see cref="TextEntity"/>; this object is left alone.
		/// </remarks>
		/// <returns>The text entity.</returns>
		public TextEntity ToTextEntity()
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

			return text;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> Explode()
		{
			yield return this.ToTextEntity();
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
			if (!this.HasPosition)
			{
				return BoundingBox.Null;
			}

			BoundingBox box = BoundingBox.Null;

			foreach (Entity entity in this.Explode())
			{
				box = box.Merge(entity.GetBoundingBox());
			}

			return box;
		}

		/// <summary>
		/// Says whether a Tianzheng class writes text, and so is read as a
		/// <see cref="TchText"/> rather than as an unlisted entity.
		/// </summary>
		/// <param name="dxfName">DXF name of the class.</param>
		/// <returns>True when the class writes text.</returns>
		internal static bool WritesText(string dxfName)
		{
			return dxfName == "TCH_TEXT" || dxfName == "TCH_MTEXT" || dxfName == "TCH_DRAWINGNAME";
		}
	}
}
