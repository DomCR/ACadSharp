using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects;

public partial class DimensionAssociation
{
	public class OsnapPointRef
	{
		/// <summary>
		/// Gets or sets the geometry parameter used for the Near object snap (Osnap).
		/// </summary>
		[DxfCodeValue(40)]
		public double GeometryParameter { get; set; }

		/// <summary>
		/// Gets or sets the object snap type associated with the entity.
		/// </summary>
		[DxfCodeValue(72)]
		public ObjectOsnapType ObjectOsnapType { get; set; }

		/// <summary>
		/// Gets or sets the object snap (Osnap) point in world coordinate system (WCS).
		/// </summary>
		/// <remarks>The Osnap point is used to specify precise locations on geometry for object snapping
		/// operations.</remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ OsnapPoint { get; set; }

		/// <summary>
		/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
		/// </summary>
		[DxfCodeValue(73)]
		public SubentType SubentType { get; set; } = SubentType.Unknown;

		/// <summary>
		/// Gets or sets the graphics system marker (GsMarker) associated with the main object.
		/// </summary>
		[DxfCodeValue(91)]
		public int GsMarker { get; set; }

		//332
		//ID of intersection object (geometry)

		[DxfCodeValue(74)]
		public SubentType IntersectionSubType { get; set; }

		[DxfCodeValue(92)]
		public int IntersectionGsMarker { get; set; }

		//302
		//Handle(string) of intersection Xref object

		[DxfCodeValue(75)]
		public bool HasLastPointRef { get; set; }

		/// <summary>
		/// Gets or sets the associated geometry object: the first object of <see cref="GeometryPath"/>, the one
		/// in the same space as the dimension (and the one that carries the reactor back to the association).
		/// </summary>
		/// <remarks>Setting it replaces the whole path with this single object.</remarks>
		public CadObject Geometry
		{
			get { return this.GeometryPath.Count > 0 ? this.GeometryPath[0] : null; }
			set
			{
				this.GeometryPath.Clear();
				if (value != null)
				{
					this.GeometryPath.Add(value);
				}
			}
		}

		/// <summary>
		/// The path to the snapped geometry, outermost first: a single object, or the block reference(s)
		/// followed by the object inside the block. Empty when the point has no object snap.
		/// </summary>
		/// <remarks>DXF code 331, repeated once per object of the path.</remarks>
		public List<CadObject> GeometryPath { get; private set; } = new List<CadObject>();

		/// <summary>
		/// Handles (as strings) of the Xref objects of the main path.
		/// </summary>
		/// <remarks>DXF code 301.</remarks>
		public List<string> XrefPaths { get; private set; } = new List<string>();

		/// <summary>
		/// The path to the second object of an intersection (or apparent intersection) snap, outermost first.
		/// </summary>
		/// <remarks>DXF code 332.</remarks>
		public List<CadObject> IntersectionPath { get; private set; } = new List<CadObject>();

		/// <summary>
		/// Handles (as strings) of the Xref objects of the intersection path.
		/// </summary>
		/// <remarks>DXF code 302.</remarks>
		public List<string> IntersectionXrefPaths { get; private set; } = new List<string>();

		/// <summary>
		/// The reference that follows this one in the same slot when <see cref="HasLastPointRef"/> is set
		/// (AutoCAD writes it after a perpendicular snap: the point the perpendicular is taken from).
		/// </summary>
		public OsnapPointRef LastPointRef { get; set; }

		/// <summary>
		/// Copies the reference, its lists and the chained reference. The referenced objects themselves are not
		/// copied (the copy points to the same geometry), but no list is shared with the original.
		/// </summary>
		public OsnapPointRef Clone()
		{
			OsnapPointRef clone = (OsnapPointRef)this.MemberwiseClone();
			clone.GeometryPath = new List<CadObject>(this.GeometryPath);
			clone.XrefPaths = new List<string>(this.XrefPaths);
			clone.IntersectionPath = new List<CadObject>(this.IntersectionPath);
			clone.IntersectionXrefPaths = new List<string>(this.IntersectionXrefPaths);
			clone.LastPointRef = this.LastPointRef?.Clone();
			return clone;
		}

		/// <summary>
		/// True when the snap type stores a second object (intersection and apparent intersection).
		/// </summary>
		internal bool HasIntersection => this.ObjectOsnapType == ObjectOsnapType.Intersection
			|| this.ObjectOsnapType == ObjectOsnapType.ApparentIntersection;
	}
}