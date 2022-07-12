using ACadSharp.Entities;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgObjectWriter : DwgSectionIO
	{
		/// <summary>
		/// Key : handle | Value : Offset
		/// </summary>
		public Dictionary<ulong, long> Map { get; } = new Dictionary<ulong, long>();

		private MemoryStream _msbegin;
		private MemoryStream _msmain;

		private IDwgStreamWriter _swbegin;
		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;

		public DwgObjectWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;

			this._msbegin = new MemoryStream();
			this._msmain = new MemoryStream();

			this._swbegin = DwgStreamWriterBase.GetStreamHandler(document.Header.Version, this._msbegin, Encoding.Default);
			this._writer = DwgStreamWriterBase.GetStreamHandler(document.Header.Version, _msmain, Encoding.Default);
		}

		public void Write()
		{

		}

		private void registerObject(CadObject cadObject)
		{
			this._writer.WriteSpearShift();
			long position = this._stream.Position;

			this.Map.Add(cadObject.Handle, position);
		}

		private void writeCommonEntityData(Entity entity)
		{

		}

		private void writeLine(Line line)
		{
			this.writeCommonEntityData(line);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Start pt 3BD 10
				this._writer.Write3BitDouble(line.StartPoint);
				//End pt 3BD 11
				this._writer.Write3BitDouble(line.EndPoint);
			}


			//R2000+:
			if (this.R2000Plus)
			{
				//Z’s are zero bit B
				bool flag = line.StartPoint.Z == 0.0 && line.EndPoint.Z == 0.0;
				this._writer.WriteBit(flag);

				//Start Point x RD 10
				this._writer.WriteRawDouble(line.StartPoint.X);
				//End Point x DD 11 Use 10 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.X, line.StartPoint.X);
				//Start Point y RD 20
				this._writer.WriteRawDouble(line.StartPoint.Y);
				//End Point y DD 21 Use 20 value for default
				this._writer.WriteBitDoubleWithDefault(line.EndPoint.Y, line.StartPoint.Y);

				if (!flag)
				{
					//Start Point z RD 30 Present only if “Z’s are zero bit” is 0
					this._writer.WriteRawDouble(line.StartPoint.Z);
					//End Point z DD 31 Present only if “Z’s are zero bit” is 0, use 30 value for default.
					this._writer.WriteBitDoubleWithDefault(line.EndPoint.Z, line.StartPoint.Z);
				}
			}

			//Common:
			//Thickness BT 39
			this._writer.ReadBitThickness(line.Thickness);
			//Extrusion BE 210
			this._writer.ReadBitExtrusion(line.Normal);
		}
	}
}
