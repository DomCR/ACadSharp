using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DXF.DxfStreamReader
{
	internal class DxfReaderBase : IDxfStreamReader
	{
		public DxfCode LastDxfCode { get; private set; }
		public GroupCodeValueType LastGroupCodeValue { get; private set; }
		public int LastCode { get { return (int)this.LastDxfCode; } }
		public object LastValue { get; private set; }
		public int Line { get; private set; }

		/// <inheritdoc/>
		public string LastValueAsString { get; set; }
		public bool LastValueAsBool { get { return this.lineAsBool(this.LastValueAsString); } }
		public short LastValueAsShort { get { return this.lineAsShort(this.LastValueAsString); } }
		public int LastValueAsInt { get { return this.lineAsInt(this.LastValueAsString); } }
		public long LastValueAsLong { get { return this.lineAsLong(this.LastValueAsString); } }
		public double LastValueAsDouble { get { return this.lineAsDouble(this.LastValueAsString); } }
		public ulong LastValueAsHandle { get { return this.lineAsHandle(this.LastValueAsString); } }
		public byte[] LastValueAsBinaryChunk { get { return this.lineAsBinaryChunk(this.LastValueAsString); } }

		public DxfReaderBase()
		{
			this.start();
		}

		public Tuple<DxfCode, object> ReadNext()
		{
			this.LastDxfCode = this.readCode();
			this.LastValueAsString = this.ReadLine();
			this.LastGroupCodeValue = GroupCodeValue.TransformValue(this.LastCode);
			this.LastValue = this.transformValue(this.LastGroupCodeValue, this.LastValueAsString);

			Tuple<DxfCode, object> pair = new Tuple<DxfCode, object>(this.LastDxfCode, this.LastValue);

			return pair;
		}

		public void Find(string dxfEntry)
		{
			this.start();

			do
			{
				this.ReadNext();
			}
			while (this.LastValueAsString != dxfEntry && (this.LastValueAsString != DxfFileToken.EndOfFile));
		}

		protected void start()
		{
			this.LastDxfCode = DxfCode.Invalid;
			this.LastValue = string.Empty;

			this.BaseStream.Position = 0;
			this.DiscardBufferedData();

			this.Line = 0;
		}
	}
}
