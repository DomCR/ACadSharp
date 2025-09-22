using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Viewport"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityViewport"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Viewport"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityViewport)]
	[DxfSubClass(DxfSubclassMarker.Viewport)]
	public class Viewport : Entity
	{
		/// <summary>
		/// Ambient light color.Write only if not black color.
		/// </summary>
		[DxfCodeValue(63, 421, 431)]
		public Color AmbientLightColor { get; set; }

		/// <summary>
		/// Back clip plane Z value.
		/// </summary>
		[DxfCodeValue(44)]
		public double BackClipPlane { get; set; }

		/// <summary>
		/// Hard-pointer ID/handle to entity that serves as the viewport's clipping boundary (only present if viewport is non-rectangular)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Entity Boundary { get; set; }

		/// <summary>
		/// View brightness
		/// </summary>
		[DxfCodeValue(141)]
		public double Brightness { get; set; }

		/// <summary>
		/// Center point(in WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; }

		/// <summary>
		/// Circle zoom percent
		/// </summary>
		[DxfCodeValue(72)]
		public short CircleZoomPercent { get; set; }

		/// <summary>
		/// View contrast
		/// </summary>
		[DxfCodeValue(142)]
		public double Contrast { get; set; }

		/// <summary>
		/// Default lighting type.
		/// </summary>
		/// <remarks>
		/// 0 = One distant light
		/// 1 = Two distant lights
		/// </remarks>
		[DxfCodeValue(282)]
		public LightingType DefaultLightingType { get; set; }

		/// <summary>
		/// Display UCS icon at UCS origin flag
		/// </summary>
		/// <remarks>
		/// Controls whether UCS icon represents viewport UCS or current UCS(these will be different if UCSVP is 1 and viewport is not active). However, this field is currently being ignored and the icon always represents the viewport UCS
		/// </remarks>
		[DxfCodeValue(74)]
		public bool DisplayUcsIcon { get; set; }

		/// <summary>
		/// Viewport elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// Front clip plane Z value.
		/// </summary>
		[DxfCodeValue(43)]
		public double FrontClipPlane { get; set; }

		/// <summary>
		/// Frozen layer object ID/handle(multiple entries may exist)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Ignored, 331)]   //TODO: explore how to write list values
		public List<Layer> FrozenLayers { get; private set; } = new List<Layer>();

		/// <summary>
		/// Grid spacing
		/// </summary>
		[DxfCodeValue(15, 25)]
		public XY GridSpacing { get; set; }

		/// <summary>
		/// Height in paper space units.
		/// </summary>
		[DxfCodeValue(41)]
		public double Height { get; set; }

		/// <summary>
		/// Viewport ID.
		/// </summary>
		/// <remarks>
		/// The first value for a PaperSpace will represent the paper image in the screen.
		/// </remarks>
		[DxfCodeValue(69)]
		public short Id
		{
			get
			{
				if (this.Owner is BlockRecord record)
				{
					short id = 0;
					foreach (Viewport viewport in record.Viewports)
					{
						id += 1;
						if (viewport == this)
						{
							return id;
						}
					}
				}

				return 0;
			}
		}

		/// <summary>
		/// Perspective lens length
		/// </summary>
		[DxfCodeValue(42)]
		public double LensLength { get; set; }

		/// <summary>
		/// Frequency of major grid lines compared to minor grid lines
		/// </summary>
		[DxfCodeValue(61)]
		public short MajorGridLineFrequency { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityViewport;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEWPORT;

		/// <summary>
		/// Render mode
		/// </summary>
		[DxfCodeValue(281)]
		public RenderMode RenderMode { get; set; }

		/// <summary>
		/// Flag that set for those viewports that represent the paper in the view.
		/// </summary>
		/// <remarks>
		/// A paper viewport is only for boundaries only, does not visualize anything.
		/// </remarks>
		public bool RepresentsPaper
		{
			get
			{
				return this.Id == PaperViewId;
			}
		}

		/// <summary>
		/// Scale assigned for this viewport.
		/// </summary>
		public Scale Scale
		{
			get
			{
				if (this.Document != null)
				{
					if (this.XDictionary != null && this.XDictionary.TryGetEntry(ASDK_XREC_ANNOTATION_SCALE_INFO, out XRecord record))
					{
						foreach (XRecord.Entry item in record.Entries)
						{
							if (item.Code == 340)
							{
								return item.Value as Scale;
							}
						}
					}

					return null;
				}
				else
				{
					return this._scale;
				}
			}
			set
			{
				if (this.Document != null)
				{
					this._scale = updateCollection(value, this.Document.Scales);
				}
				else
				{
					this._scale = value;
				}

				this.updateScaleXRecord();
			}
		}

		//Soft pointer reference to viewport object (for layer VP property override)
		/// <summary>
		/// Scale factor applied in this viewport.
		/// </summary>
		/// <remarks>
		/// Represents the scale applied to all the entities in the model when are visualized in the paper.
		/// </remarks>
		public double ScaleFactor => 1 / (this.ViewHeight / this.Height);

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(170)]
		public ShadePlotMode ShadePlotMode { get; set; }

		/// <summary>
		/// Snap angle
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double SnapAngle { get; set; }

		/// <summary>
		/// Snap base point
		/// </summary>
		[DxfCodeValue(13, 23)]
		public XY SnapBase { get; set; }

		/// <summary>
		/// Snap spacing
		/// </summary>
		[DxfCodeValue(14, 24)]
		public XY SnapSpacing { get; set; }

		/// <summary>
		/// Viewport status.
		/// </summary>
		[DxfCodeValue(90)]
		public ViewportStatusFlags Status { get; set; }

		/// <summary>
		/// Plot style sheet name assigned to this viewport
		/// </summary>
		[DxfCodeValue(1)]
		public string StyleSheetName { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Viewport;

		/// <summary>
		/// View twist angle
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double TwistAngle { get; set; }

		/// <summary>
		/// UCS origin
		/// </summary>
		[DxfCodeValue(110, 120, 130)]
		public XYZ UcsOrigin { get; set; }

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(79)]
		public OrthographicType UcsOrthographicType { get; set; }

		/// <summary>
		/// UCS per viewport flag
		/// </summary>
		/// <remarks>
		///0 = The UCS will not change when this viewport becomes active.
		///1 = This viewport stores its own UCS which will become the current UCS whenever the viewport is activated
		/// </remarks>
		[DxfCodeValue(71)]
		public bool UcsPerViewport { get; set; }

		/// <summary>
		/// UCS X-axis
		/// </summary>
		[DxfCodeValue(111, 121, 131)]
		public XYZ UcsXAxis { get; set; }

		/// <summary>
		/// UCS Y-axis
		/// </summary>
		[DxfCodeValue(112, 122, 132)]
		public XYZ UcsYAxis { get; set; }

		/// <summary>
		/// Default lighting flag.On when no user lights are specified.
		/// </summary>
		[DxfCodeValue(292)]
		public bool UseDefaultLighting { get; set; }

		/// <summary>
		/// View center point(in DCS).
		/// </summary>
		[DxfCodeValue(12, 22)]
		public XY ViewCenter { get; set; }

		/// <summary>
		/// View direction vector(in WCS)
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ ViewDirection { get; set; }

		/// <summary>
		/// View height(in model space units).
		/// </summary>
		[DxfCodeValue(45)]
		public double ViewHeight { get; set; }

		/// <summary>
		/// View target point(in WCS)
		/// </summary>
		[DxfCodeValue(17, 27, 37)]
		public XYZ ViewTarget { get; set; }

		/// <summary>
		/// View width (in model space units).
		/// </summary>
		public double ViewWidth
		{
			get
			{
				return this.ViewHeight / this.Height * this.Width;
			}
		}

		/// <summary>
		/// Visual style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 348)]
		public VisualStyle VisualStyle { get; set; }

		/// <summary>
		/// Width in paper space units.
		/// </summary>
		[DxfCodeValue(40)]
		public double Width { get; set; }

		public const string ASDK_XREC_ANNOTATION_SCALE_INFO = "ASDK_XREC_ANNOTATION_SCALE_INFO";

		/// <summary>
		/// Paper view Id, it indicates that the viewport acts as a paper size.
		/// </summary>
		public const int PaperViewId = 1;

		private Scale _scale;

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			if (this.Boundary != null)
			{
				this.Boundary.ApplyTransform(transform);
			}
			else
			{
				this.Center = transform.ApplyTransform(this.Center);

				XYZ max = new XYZ(this.Center.X + this.Width / 2, this.Center.Y + this.Height / 2, this.Center.Z);
				max = transform.ApplyTransform(max);

				this.Width = max.X - this.Center.X;
				this.Height = max.Y - this.Center.Y;
			}
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Viewport clone = (Viewport)base.Clone();

			clone.VisualStyle = (VisualStyle)this.VisualStyle?.Clone();
			clone._scale = (Scale)this.Scale?.Clone();

			return clone;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			XYZ min = new XYZ(this.Center.X - this.Width / 2, this.Center.Y - this.Height / 2, this.Center.Z);
			XYZ max = new XYZ(this.Center.X + this.Width / 2, this.Center.Y + this.Height / 2, this.Center.Z);
			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Gets the bounding box of this viewport in the model space.
		/// </summary>
		/// <returns></returns>
		public BoundingBox GetModelBoundingBox()
		{
			XYZ min = new XYZ(this.ViewCenter.X - this.ViewWidth / 2, this.ViewCenter.Y - this.ViewHeight / 2, 0);
			XYZ max = new XYZ(this.ViewCenter.X + this.ViewWidth / 2, this.ViewCenter.Y + this.ViewHeight / 2, 0);
			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Gets all the entities from the model that are in the view of the viewport.
		/// </summary>
		/// <returns></returns>
		public List<Entity> SelectEntities(bool includePartial = true)
		{
			if (this.Document == null)
			{
				throw new InvalidOperationException($"Viewport needs to be assigned to a document.");
			}

			List<Entity> entities = new List<Entity>();

			BoundingBox box = this.GetModelBoundingBox();
			foreach (Entity e in this.Document.Entities)
			{
				if (box.IsIn(e.GetBoundingBox(), out bool partialIn) || (partialIn && includePartial))
				{
					entities.Add(e);
				}
			}

			return entities;
		}
		
		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._scale = updateCollection(this._scale, doc.Scales);

			this.Document.Scales.OnRemove += this.scalesOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.Scales.OnRemove -= this.scalesOnRemove;

			base.UnassignDocument();

			this._scale = (Scale)this.Scale?.Clone();
		}

		private void scalesOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.Scale))
			{
				this.Scale = this.Document.Scales.FirstOrDefault();
			}
		}

		private void updateScaleXRecord()
		{
			if (this.Document == null)
			{
				return;
			}

			if (this.XDictionary.TryGetEntry(ASDK_XREC_ANNOTATION_SCALE_INFO, out XRecord record))
			{
				foreach (XRecord.Entry item in record.Entries)
				{
					if (item.Code == 340)
					{
						item.Value = this._scale;
					}
				}
			}
			else
			{
				record = new XRecord(ASDK_XREC_ANNOTATION_SCALE_INFO);
				this.XDictionary.Add(record);

				record.CreateEntry(340, _scale);
			}
		}
	}
}