using ACadSharp.Header;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfHeaderSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.HeaderSection; } }

		public CadHeader Header { get { return this._document.Header; } }

		public DxfHeaderSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder, DxfWriterConfiguration configuration)
			: base(writer, document, holder, configuration)
		{
		}

		protected override void writeSection()
		{
			Dictionary<string, CadSystemVariable> map = CadHeader.GetHeaderMap();

			foreach (KeyValuePair<string, CadSystemVariable> item in map)
			{
				if (!this.Configuration.WriteAllHeaderVariables && !this.Configuration.HeaderVariables.Contains(item.Key))
					continue;

				if (item.Value.ReferenceType.HasFlag(DxfReferenceType.Ignored))
					continue;

				if (item.Value.GetValue(this.Header) == null)
					continue;

				this._writer.Write(DxfCode.CLShapeText, item.Key);

				if (item.Key == "$HANDSEED")    //Not very elegant but by now...
				{
					this._writer.Write(DxfCode.Handle, this._document.Header.HandleSeed);
					continue;
				}

				if (item.Key == "$CECOLOR")
				{
					this._writer.Write(62, this._document.Header.CurrentEntityColor.GetApproxIndex());
					continue;
				}

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
