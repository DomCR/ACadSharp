using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadViewportEntityControlTemplate : CadTemplate
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public CadViewportEntityControlTemplate() : base(new VPEntityPlaceholder()) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}

		public class VPEntityPlaceholder : CadObject
		{
			public override ObjectType ObjectType { get; }

			public override string SubclassMarker { get; }
		}
	}
}
