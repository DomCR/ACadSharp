using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfHeaderSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.HeaderSection; } }

		public DxfHeaderSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			Dictionary<string, CadSystemVariable> map = Header.CadHeader.GetHeaderMap();

			foreach (var item in map)
			{
				this._writer.Write(DxfCode.CLShapeText, item.Key);

				foreach (var csv in item.Value.DxfCodes)
				{
					object value = item.Value.GetValue(csv, this._document.Header);

					if (value == null)
						continue;

					this._writer.Write((DxfCode)csv, value);
				}
			}
		}
	}
}
