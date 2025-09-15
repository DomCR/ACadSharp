using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;
using System;
using ACadSharp.Objects;
using System.Linq;
using CSUtilities.Extensions;

namespace ACadSharp.Entities
{
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
		/// Clipping state
		/// </summary>
		[DxfCodeValue(290)]
		public ClipMode ClipMode { get; set; }

		/// <summary>
		/// Clipping state
		/// </summary>
		[DxfCodeValue(280)]
		public bool ClippingState { get; set; }

		/// <summary>
		/// Clipping boundary type
		/// </summary>
		[DxfCodeValue(71)]
		public ClipType ClipType { get; set; } = ClipType.Rectangular;

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
				if (this.Document != null)
				{
					this._definition = CadObject.updateCollection(value, this.Document.ImageDefinitions);
				}
				else
				{
					this._definition = value;
				}
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
		public override BoundingBox GetBoundingBox()
		{
			if (!this.ClipBoundaryVertices.Any())
			{
				return BoundingBox.Null;
			}

			double minX = this.ClipBoundaryVertices.Select(v => v.X).Min();
			double minY = this.ClipBoundaryVertices.Select(v => v.Y).Min();
			XYZ min = new XYZ(minX, minY, 0) + this.InsertPoint;

			double maxX = this.ClipBoundaryVertices.Select(v => v.X).Max();
			double maxY = this.ClipBoundaryVertices.Select(v => v.Y).Max();
			XYZ max = new XYZ(maxX, maxY, 0) + this.InsertPoint;

			BoundingBox box = new BoundingBox(min, max);

			return box;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._definition = CadObject.updateCollection(this.Definition, doc.ImageDefinitions);

			this.Document.ImageDefinitions.OnRemove += this.imageDefinitionsOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.ImageDefinitions.OnRemove -= this.imageDefinitionsOnRemove;

			base.UnassignDocument();

			this.Definition = (ImageDefinition)this.Definition?.Clone();
		}

		private void imageDefinitionsOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Definition))
			{
				this.Definition = null;
			}
		}
	}
}