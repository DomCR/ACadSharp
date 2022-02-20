using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class DwgLineTypeTemplate : DwgTableEntryTemplate<LineType>
	{
		public ulong? LtypeControlHandle { get; set; }

		public DwgLineTypeTemplate(LineType entry) : base(entry)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.LtypeControlHandle.HasValue && this.LtypeControlHandle.Value > 0)
			{
				builder.NotificationHandler?.Invoke(
					this.CadObject,
					new NotificationEventArgs($"LtypeControlHandle not assigned : {LtypeControlHandle}"));
			}
		}
	}

}
