using ACadSharp.Attributes;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="GeoData"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectGeoData"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.GeoData"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectGeoData)]
	[DxfSubClass(DxfSubclassMarker.GeoData)]
	public partial class GeoData : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectGeoData;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.GeoData;

		/// <summary>
		/// AcDbGeoData Object version.
		/// </summary>
		/// <remarks>
		/// 1 - 2009 <br/>
		/// 2 - 2010 <br/>
		/// 3 - 2013
		/// </remarks>
		[DxfCodeValue(90)]
		public GeoDataVersion Version { get; set; } = GeoDataVersion.R2013;

		/// <summary>
		/// Type of design coordinates.
		/// </summary>
		[DxfCodeValue(70)]
		public DesignCoordinatesType CoordinatesType { get; set; } = DesignCoordinatesType.LocalGrid;

		/// <summary>
		/// Host block table record.
		/// </summary>
		public BlockRecord HostBlock
		{
			get { return this._hostBlock; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._hostBlock = CadObject.updateCollection(value, this.Document.BlockRecords);
				}
				else
				{
					this._hostBlock = value;
				}
			}
		}

		/// <summary>
		/// Design point, reference point in WCS coordinates.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ DesignPoint { get; set; }

		/// <summary>
		/// Reference point in coordinate system coordinates, valid only when coordinate type is Local Grid.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ ReferencePoint { get; set; }

		/// <summary>
		/// North direction vector.
		/// </summary>
		[DxfCodeValue(12, 22)]
		public XY NorthDirection { get; set; } = XY.AxisY;

		/// <summary>
		/// Horizontal unit scale, factor which converts horizontal design coordinates to meters by multiplication.
		/// </summary>
		[DxfCodeValue(41)]
		public double HorizontalUnitScale { get; set; } = 1;

		/// <summary>
		/// Vertical unit scale, factor which converts vertical design coordinates to meters by multiplication.
		/// </summary>
		[DxfCodeValue(40)]
		public double VerticalUnitScale { get; set; } = 1;

		/// <summary>
		/// Horizontal units.
		/// </summary>
		[DxfCodeValue(91)]
		public UnitsType HorizontalUnits { get; set; } = UnitsType.Meters;

		/// <summary>
		/// Vertical units.
		/// </summary>
		[DxfCodeValue(92)]
		public UnitsType VerticalUnits { get; set; } = UnitsType.Meters;

		/// <summary>
		/// Up direction.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ UpDirection { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Scale estimation method.
		/// </summary>
		[DxfCodeValue(95)]
		public ScaleEstimationType ScaleEstimationMethod { get; set; } = ScaleEstimationType.None;

		/// <summary>
		/// Bool flag specifying whether to do sea level correction.
		/// </summary>
		[DxfCodeValue(294)]
		public bool EnableSeaLevelCorrection { get; set; }

		/// <summary>
		/// User specified scale factor.
		/// </summary>
		[DxfCodeValue(141)]
		public double UserSpecifiedScaleFactor { get; set; }

		/// <summary>
		/// Sea level elevation.
		/// </summary>
		[DxfCodeValue(142)]
		public double SeaLevelElevation { get; set; }

		/// <summary>
		/// Coordinate projection radius.
		/// </summary>
		[DxfCodeValue(143)]
		public double CoordinateProjectionRadius { get; set; }

		/// <summary>
		/// Coordinate system definition string.
		/// </summary>
		[DxfCodeValue(301)]
		public string CoordinateSystemDefinition { get; set; } = string.Empty;

		/// <summary>
		/// GeoRSS tag.
		/// </summary>
		[DxfCodeValue(302)]
		public string GeoRssTag { get; set; } = string.Empty;

		/// <summary>
		/// Observation from tag.
		/// </summary>
		[DxfCodeValue(305)]
		public string ObservationFromTag { get; set; } = string.Empty;

		/// <summary>
		/// Observation to tag.
		/// </summary>
		[DxfCodeValue(306)]
		public string ObservationToTag { get; set; } = string.Empty;

		/// <summary>
		/// Observation coverage tag.
		/// </summary>
		[DxfCodeValue(307)]
		public string ObservationCoverageTag { get; set; } = string.Empty;

		/// <summary>
		/// Geo-Mesh points.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 93)]
		public List<GeoMeshPoint> Points { get; } = new();

		/// <summary>
		/// Faces.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 96)]
		public List<GeoMeshFace> Faces { get; } = new();

		private BlockRecord _hostBlock;

		internal override void AssignDocument(CadDocument doc)
		{
			//TODO: the assigned block is always the owner of the dictionary??
			base.AssignDocument(doc);
		}

		internal override void UnassignDocument()
		{
			base.UnassignDocument();
		}
	}
}
