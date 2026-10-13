using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
using System;
using ACadSharp.Objects;
using System.Linq;
using CSUtilities.Extensions;
using ACadSharp.IO;

namespace ACadSharp.Entities;

/// <summary>
/// Common base class for <see cref="RasterImage" /> and <see cref="Wipeout" />.
/// </summary>
[DxfSubClass(null, true)]
public abstract class CadWipeoutBase : Entity
{
	/// <summary>
	/// Brightness
	/// </summary>
	/// <remarks>
	/// 0-100; default = 50
	/// </remarks>
	[DxfCodeValue(281)]
	public byte Brightness
	{
		get { return this._brightness; }
		set
		{
			if (value < 0 || value > 100)
			{
				throw new ArgumentException($"Invalid Brightness value: {value}, must be in range 0-100");
			}

			this._brightness = value;
		}
	}

	/// <summary>
	/// Class version
	/// </summary>
	[DxfCodeValue(90)]
	public int ClassVersion { get; set; }

	/// <summary>
	/// Clip boundary vertices.
	/// </summary>
	/// <remarks>
	/// For rectangular clip boundary type, two opposite corners must be specified.Default is (-0.5,-0.5), (size.x-0.5, size.y-0.5). 2) For polygonal clip boundary type, three or more vertices must be specified.Polygonal vertices must be listed sequentially
	/// </remarks>
	[DxfCodeValue(DxfReferenceType.Count, 91)]
	[DxfCollectionCodeValue(14, 24)]
	public List<XY> ClipBoundaryVertices { get; set; } = new List<XY>();

	/// <summary>
	/// Clipping state.
	/// </summary>
	[DxfCodeValue(290)]
	public ClipMode ClipMode { get; set; }

	/// <summary>
	/// Clipping state.
	/// </summary>
	[DxfCodeValue(280)]
	public bool ClippingState { get; set; }

	/// <summary>
	/// Clipping boundary type.
	/// </summary>
	[DxfCodeValue(71)]
	public ClipType ClipType { get { return this.ClipBoundaryVertices.Count > 2 ? ClipType.Polygonal : ClipType.Rectangular; } }

	/// <summary>
	/// Contrast
	/// </summary>
	/// <remarks>
	/// 0-100; default = 50
	/// </remarks>
	[DxfCodeValue(282)]
	public byte Contrast
	{
		get { return this._contrast; }
		set
		{
			if (value < 0 || value > 100)
			{
				throw new ArgumentException($"Invalid Brightness value: {value}, must be in range 0-100");
			}

			this._contrast = value;
		}
	}

	/// <summary>
	/// Image definition.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public virtual ImageDefinition Definition
	{
		get { return this._definition; }
		set
		{
			this._definition = this.updateCollectionEntry(value, d => this._definition = d, this.Document?.ImageDefinitions);
		}
	}

	/// <summary>
	/// Fade
	/// </summary>
	/// <remarks>
	/// 0-100; default = 0
	/// </remarks>
	[DxfCodeValue(283)]
	public byte Fade
	{
		get { return this._fade; }
		set
		{
			if (value < 0 || value > 100)
			{
				throw new ArgumentException($"Invalid Brightness value: {value}, must be in range 0-100");
			}

			this._fade = value;
		}
	}

	/// <summary>
	/// Image display properties.
	/// </summary>
	[DxfCodeValue(70)]
	public ImageDisplayFlags Flags { get => _flags; set => _flags = value; }

	/// <summary>
	/// Insertion point(in WCS)
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ InsertPoint { get; set; }

	/// <summary>
	/// Add the ShowImage flag to the display flags property.
	/// </summary>
	public bool ShowImage
	{
		get { return this.Flags.HasFlag(ImageDisplayFlags.ShowImage); }
		set
		{
			if (value)
			{
				this._flags.AddFlag(ImageDisplayFlags.ShowImage);
			}
			else
			{
				this._flags.RemoveFlag(ImageDisplayFlags.ShowImage);
			}
		}
	}

	/// <summary>
	/// Image size in pixels.
	/// </summary>
	/// <remarks>
	/// 2D point(U and V values).
	/// </remarks>
	[DxfCodeValue(13, 23)]
	public XY Size { get; set; }

	/// <summary>
	/// U-vector of a single pixel(points along the visual bottom of the image, starting at the insertion point) (in WCS)
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ UVector { get; set; } = XYZ.AxisX;

	/// <summary>
	/// V-vector of a single pixel(points along the visual left side of the image, starting at the insertion point) (in WCS)
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ VVector { get; set; } = XYZ.AxisY;

	/// <summary>
	/// Reference to image definition reactor.
	/// </summary>
	//It seems that is not necessecary, keep it hidden for now
	[DxfCodeValue(DxfReferenceType.Handle, 360)]
	internal ImageDefinitionReactor DefinitionReactor
	{
		get { return this._definitionReactor; }
		set
		{
			this._definitionReactor = value;
			this._definitionReactor.Owner = this;
		}
	}

	private byte _brightness = 50;

	private byte _contrast = 50;

	private ImageDefinition _definition;

	private ImageDefinitionReactor _definitionReactor;

	private byte _fade = 0;

	private ImageDisplayFlags _flags;

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		this.InsertPoint = transform.ApplyTransform(this.InsertPoint);
		this.UVector = transform.ApplyTransform(this.UVector);
		this.VVector = transform.ApplyTransform(this.VVector);
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		CadWipeoutBase clone = (CadWipeoutBase)base.Clone();

		clone.Definition = (ImageDefinition)this.Definition?.Clone();

		return clone;
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The boundary is in the image's OWN pixel space, measured from pixel centres - which is why an
	/// unclipped image stores (-0.5, -0.5) to (Size - 0.5) - and <see cref="UVector"/> and
	/// <see cref="VVector"/> are what map one pixel into the world. Adding those pixel coordinates
	/// straight to the insertion point describes a rectangle the image does not occupy as soon as
	/// the vectors are not the world axes: on a client drawing an image placed with U and V rotated
	/// 45 degrees, 10.6 units to the pixel, was reported as a 500x500 square at the insertion point
	/// while AutoCAD measures a 10,591 x 10,591 diamond hanging below it.
	/// </remarks>
	public override BoundingBox GetBoundingBox()
	{
		IReadOnlyList<XY> boundary = this.GetEffectiveClipBoundary();
		if (boundary.Count < 2)
		{
			return BoundingBox.Null;
		}

		//Two vertices are the opposite corners of a rectangle, and the other two have to be built
		//before the mapping: mapping only the diagonal measures a rotated image along that diagonal
		//instead of around its edges.
		IEnumerable<XY> corners = boundary.Count == 2
			? new[]
			{
				boundary[0],
				new XY(boundary[1].X, boundary[0].Y),
				boundary[1],
				new XY(boundary[0].X, boundary[1].Y),
			}
			: boundary;

		//X counts rightwards from the left edge and Y DOWNWARDS from the top, which is why an
		//unclipped image - whose boundary is the whole rectangle either way - cannot tell the two
		//apart, and a wipeout can: its boundary sits at the top of a 1x1 image, and AutoCAD draws it
		//at the insertion point rather than a whole V below it. Measured on both, to 1e-3 of what
		//AutoCAD reports for the same entity.
		return BoundingBox.FromPoints(corners.Select(vertex =>
			this.InsertPoint
			+ ((vertex.X + 0.5) * this.UVector)
			+ ((this.Size.Y - 0.5 - vertex.Y) * this.VVector)));
	}

	/// <inheritdoc/>
	public override bool IsValid(CadFileFormat format, ACadVersion version, out IList<string> errors)
	{
		var result = base.IsValid(format, version, out errors);

		if (this.ClipBoundaryVertices.Count < 2)
		{
			errors.Add($"Invalid {nameof(ClipBoundaryVertices)} count: {this.ClipBoundaryVertices.Count}, must be at least 2.");
			result = false;
		}

		return result;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		this._definition = this.updateCollectionEntry(this.Definition, d => this._definition = d, doc.ImageDefinitions);
	}

	internal override void UnassignDocument()
	{
		base.UnassignDocument();

		this._definition = (ImageDefinition)this.Definition?.Clone();
	}
}