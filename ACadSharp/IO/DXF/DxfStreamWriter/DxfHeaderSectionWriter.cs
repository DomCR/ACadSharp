using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfHeaderSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.HeaderSection; } }

		public DxfHeaderSectionWriter(IDxfStreamWriter writer, CadDocument document) : base(writer, document)
		{
		}

		protected override void writeSection()
		{
			Dictionary<string, DxfCode[]> map = Header.CadHeader.GetHeaderMap();

			foreach (var item in map)
			{
				this._writer.Write(DxfCode.CLShapeText, item.Key);

				foreach (var cv in this._document.Header.GetValues(item.Key))
				{
					this._writer.Write(cv.Key, cv.Value.ToString());
				}
			}
		}
	}
}
