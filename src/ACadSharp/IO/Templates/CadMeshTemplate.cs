using ACadSharp.Entities;
using ACadSharp.Objects;
using CSMath;

namespace ACadSharp.IO.Templates
{
	internal class CadMeshTemplate : CadEntityTemplate<Mesh>
	{
		public bool SubclassMarker { get; set; } = false;

		public CadMeshTemplate() { }

		public CadMeshTemplate(Mesh mesh) : base(mesh) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			// Pull per-vertex texture coordinates from the ADSK_XREC_SUBDVERTEXTEXCOORDS
			// XRecord that AutoCAD/AcDbSubDMesh stores on the mesh's extension dictionary.
			// Coordinates are written as repeated 43/44/45 triplets (U/V/W).
			if (this.CadObject.XDictionary == null
				|| !this.CadObject.XDictionary.TryGetEntry(Mesh.TextureCoordsXRecordName, out XRecord xrec)
				|| xrec == null)
			{
				return;
			}

			double? u = null, v = null;
			foreach (var entry in xrec.Entries)
			{
				switch (entry.Code)
				{
					case 43:
						u = toDouble(entry.Value);
						break;
					case 44:
						v = toDouble(entry.Value);
						break;
					case 45:
						double w = toDouble(entry.Value) ?? 0.0;
						if (u.HasValue && v.HasValue)
						{
							this.CadObject.TextureCoordinates.Add(new XYZ(u.Value, v.Value, w));
						}
						u = null;
						v = null;
						break;
				}
			}
		}

		private static double? toDouble(object value)
		{
			if (value == null)
			{
				return null;
			}

			return System.Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}
