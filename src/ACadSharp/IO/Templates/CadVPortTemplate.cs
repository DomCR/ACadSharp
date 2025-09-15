using ACadSharp.IO.DWG;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadVPortTemplate : CadTableEntryTemplate<VPort>
	{
		public ulong VportControlHandle { get; set; }

		public ulong? BackgroundHandle { get; set; }

		public ulong? StyleHandle { get; set; }

		public ulong? SunHandle { get; set; }

		public ulong? NamedUcsHandle { get; set; }

		public ulong? BaseUcsHandle { get; set; }

		public CadVPortTemplate() : base(new VPort()) { }

		public CadVPortTemplate(VPort cadObject) : base(cadObject) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (builder.TryGetCadObject(this.BaseUcsHandle, out UCS baseUcs))
			{
				this.CadObject.BaseUcs = baseUcs;
			}
			else if (this.BaseUcsHandle.HasValue && this.BaseUcsHandle > 0)
			{
				builder.Notify($"Boundary {this.BaseUcsHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			}

			if (builder.TryGetCadObject(this.NamedUcsHandle, out UCS namedUcs))
			{
				this.CadObject.BaseUcs = namedUcs;
			}
			else if (this.NamedUcsHandle.HasValue && this.NamedUcsHandle > 0)
			{
				builder.Notify($"Boundary {this.BaseUcsHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			}

			if (builder.TryGetCadObject(this.StyleHandle, out CadObject style))
			{

			}
		}
	}
}
