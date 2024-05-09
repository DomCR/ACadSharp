﻿using ACadSharp.IO.DWG;
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

		public override void Build(CadDocumentBuilder builder)
		{
			//TODO: implement DwgVPortTemplate

			base.Build(builder);

			if (this.BaseUcsHandle.HasValue)
			{
				this.CadObject.BaseUcs = builder.GetCadObject<UCS>(this.BaseUcsHandle.Value);
			}

			if (this.NamedUcsHandle.HasValue)
			{
				this.CadObject.NamedUcs = builder.GetCadObject<UCS>(this.NamedUcsHandle.Value);
			}

			if (builder.TryGetCadObject(StyleHandle, out CadObject style))
			{

			}
		}
	}
}
