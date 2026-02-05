using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadGeoDataTemplate : CadTemplate<GeoData>
	{
		public ulong? HostBlockHandle { get; set; }

		public CadGeoDataTemplate() : base(new GeoData()) { }

		public CadGeoDataTemplate(GeoData geodata) : base(geodata) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.getTableReference(builder, this.HostBlockHandle, null, out BlockRecord host))
			{
				this.CadObject.HostBlock = host;
			}
		}
	}
}
