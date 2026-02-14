using ACadSharp.Entities.AecObjects;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadWallTemplate : CadEntityTemplate<Wall>
	{
		public ulong BinRecordHandle { get; internal set; }
		public ulong StyleHandle { get; internal set; }
		public ulong CleanupGroupHandle { get; internal set; }
		public byte[] RawData { get; internal set; }

		public CadWallTemplate(Wall wall) : base(wall)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (RawData != null)
				this.CadObject.RawData = RawData;

			if (BinRecordHandle != 0 &&
				builder.TryGetCadObject(BinRecordHandle, out AecBinRecord binRecord))
			{
				this.CadObject.BinRecord = binRecord;
				this.CadObject.BinRecordHandle = BinRecordHandle;
			}

			if (StyleHandle != 0 &&
				builder.TryGetCadObject(StyleHandle, out AecWallStyle wallStyle))
			{
				this.CadObject.Style = wallStyle;
				this.CadObject.StyleHandle = StyleHandle;
			}

			if (CleanupGroupHandle != 0 &&
				builder.TryGetCadObject(CleanupGroupHandle, out AecCleanupGroup cleanupGroup))
			{
				this.CadObject.CleanupGroup = cleanupGroup;
				this.CadObject.CleanupGroupHandle = CleanupGroupHandle;
			}
		}
	}
}