using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSUtilities.Converters;
using System;
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

		private MemoryStream _msmain;

		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;

		public DwgObjectWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;

			this._msmain = new MemoryStream();
			this._writer = DwgStreamWriterBase.GetMergedWriter(document.Header.Version, this._msmain, Encoding.Default);
		}

		public void Write()
		{
			this.writeTables();
		}

		private void registerObject(CadObject cadObject)
		{
			this._writer.WriteSpearShift();

			//Set the position to the entity to find
			long position = this._stream.Position;
			CRC8StreamHandler crc = new CRC8StreamHandler(this._stream, 0xC0C1);

			//MS : Size of object, not including the CRC
			uint size = (uint)this._msmain.Length;
			this.writeSize(crc, size);

			//R2010+:
			if (this.R2010Plus)
			{
				long sizeb = (this._msmain.Length << 3) - this._writer.PositionInBits;
				this.writeSizeInBits(this._msmain, (ulong)sizeb);
			}

			//Write the object in the stream
			crc.Write(this._msmain.GetBuffer(), 0, (int)this._msmain.Length);
			crc.Write(LittleEndianConverter.Instance.GetBytes(crc.Seed), 0, 2);

			this.Map.Add(cadObject.Handle, position);
		}

		private void writeSize(Stream stream, uint size)
		{
			// This value is only read in IDwgStreamReader.ReadModularShort()
			// this should do the trick to write the modular short

			if (size >= 0b1000000000000000)
			{
				stream.WriteByte((byte)(size & 0b11111111));
				stream.WriteByte((byte)(((size >> 8) & 0b1111111) | 0b10000000));
				stream.WriteByte((byte)((size >> 15) & 0b11111111));
				stream.WriteByte((byte)((size >> 23) & 0b11111111));
			}
			else
			{
				stream.WriteByte((byte)(size & 0b11111111));
				stream.WriteByte((byte)((size >> 8) & 0b11111111));
			}
		}

		private void writeSizeInBits(Stream stream, ulong size)
		{
			// This value is only read in IDwgStreamReader.ReadModularChar()
			// this should do the trick to write the modular char

			if (size == 0)
			{
				stream.WriteByte(0);
				return;
			}

			ulong shift = size >> 7;
			while (size != 0)
			{
				byte b = (byte)(size & 0b1111111);
				if (shift != 0)
				{
					b = (byte)(b | 0b10000000);
				}

				stream.WriteByte(b);
				size = shift;
				shift = size >> 7;
			}
		}

		private void writeTables()
		{
			this.writeBlockControl();
		}

		private void writeBlockControl()
		{
			this.writeCommonNonEntityData(this._document.BlockRecords);

			this._writer.WriteBitLong(this._document.BlockRecords.Count);

			foreach (var item in this._document.BlockRecords)
			{
				this._writer.HandleReference(DwgReferenceType.SoftOwnership, item);
			}

			//*MODEL_SPACE and *PAPER_SPACE(hard owner).
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.ModelSpace);
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.PaperSpace);

			this.registerObject(this._document.BlockRecords);
		}

		private void writeCommonNonEntityData<T>(Table<T> table)
			where T : TableEntry
		{
			this.writeCommonData(table);
		}

		private void writeCommonEntityData(Entity entity)
		{

		}

		private void writeCommonData(CadObject cadObject)
		{
			//Reset the current stream to re-write a new object in it
			this._writer.ResetStream();

			switch (cadObject.ObjectType)
			{
				//TODO: Invalid type codes, what to do??
				case ObjectType.UNLISTED:
				case ObjectType.INVALID:
				case ObjectType.UNUSED:
					return;
				default:
					this._writer.WriteObjectType(cadObject.ObjectType);
					break;
			}

			if (this._version >= ACadVersion.AC1015 && this._version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				this._writer.SavePositonForSize();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			this._writer.HandleReference(cadObject);

			//Extended object data, if any
			this.writeExtendedData(cadObject.ExtendedData);

			//R13-R14 Only:
			//Obj size RL size of object in bits, not including end handles
			if (this.R13_14Only)
				this._writer.SavePositonForSize();

			//[Owner ref handle (soft pointer)]
			this._writer.HandleReference(cadObject.Owner.Handle);

			//write the cad object reactors
			this.writeReactorsAndDictionaryHandle(cadObject);
		}

		private void writeExtendedData(ExtendedDataDictionary data)
		{
			//EED size BS size of extended entity data, if any
			this._writer.WriteBitShort(0);
		}

		private void writeReactorsAndDictionaryHandle(CadObject cadObject)
		{
			//TODO: Write reactors and dictionary

			//Numreactors S number of reactors in this object
			this._writer.WriteBitLong(0);

			//for (int i = 0; i < 0; ++i)
			//	//[Reactors (soft pointer)]
			//	template.CadObject.Reactors.Add(this.handleReference(), null);

			//R2004+:
			if (this.R2004Plus)
			{
				_writer.WriteBit(false);
				//_writer.WriteBit(cadObject.XDictionary == null);
				//this._writer.HandleReference(DwgReferenceType.HardOwnership, cadObject.XDictionary);
			}

			//R2013+:
			if (this.R2013Plus)
			{
				//Has DS binary data B If 1 then this object has associated binary data stored in the data store
				this._writer.WriteBit(false);
			}
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
			this._writer.WriteBitThickness(line.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(line.Normal);

			this.registerObject(line);
		}
	}
}
