using ACadSharp.Entities.AecObjects;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadWallTemplate : CadEntityTemplate<Wall>
	{
		public ulong? BinRecordHandle { get; internal set; }

		public ulong? CleanupGroupHandle { get; internal set; }

		public byte[] RawData { get; internal set; }

		public ulong? StyleHandle { get; internal set; }

		public CadWallTemplate(Wall wall) : base(wall)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (RawData != null)
			{
				this.CadObject.RawData = RawData;
			}

			if (builder.TryGetCadObject(BinRecordHandle, out AecBinRecord binRecord))
			{
				this.CadObject.BinRecord = binRecord;
			}

			if (builder.TryGetCadObject(StyleHandle, out AecWallStyle wallStyle))
			{
				this.CadObject.Style = wallStyle;
			}

			if (builder.TryGetCadObject(CleanupGroupHandle, out AecCleanupGroup cleanupGroup))
			{
				this.CadObject.CleanupGroup = cleanupGroup;
			}
		}
	}
}