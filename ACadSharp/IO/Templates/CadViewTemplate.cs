using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadViewTemplate : CadTableEntryTemplate<View>
	{
		public ulong? VisualStyleHandle { get; set; }

		public ulong? NamedUcsHandle { get; set; }

		public ulong? UcsHandle { get; set; }

		public CadViewTemplate(View entry) : base(entry) { }

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool value = base.AddHandle(dxfcode, handle);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 348:
					VisualStyleHandle = handle;
					value = true;
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.UcsHandle, out UCS ucs))
			{
				this.CadObject.UCS = ucs;
			}

			if (builder.TryGetCadObject(this.NamedUcsHandle, out UCS namedUcs))
			{
				this.CadObject.UCS = namedUcs;
			}

			if (builder.TryGetCadObject(this.VisualStyleHandle, out VisualStyle vstyle))
			{
				this.CadObject.VisualStyle = vstyle;
			}
		}
	}
}
