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
	internal class DwgObjectSectionWriter : DwgSectionIO
	{
		public event NotificationEventHandler OnNotification;

		/// <summary>
		/// Key : handle | Value : Offset
		/// </summary>
		public Dictionary<ulong, long> Map { get; } = new Dictionary<ulong, long>();

		private Queue<CadObject> _objects = new Queue<CadObject>();

		private MemoryStream _msmain;

		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;

		public DwgObjectSectionWriter(Stream stream, CadDocument document) : base(document.Header.Version)
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
			long sizeb = (this._msmain.Length << 3) - this._writer.SavedPositionInBits;
			this.writeSize(crc, size);

			//R2010+:
			if (this.R2010Plus)
			{
				//MC : Size in bits of the handle stream (unsigned, 0x40 is not interpreted as sign).
				//This includes the padding bits at the end of the handle stream
				//(the padding bits make sure the object stream ends on a byte boundary).
				this.writeSizeInBits(crc, (ulong)sizeb);
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

		private void writeXrefDependantBit(TableEntry entry)
		{
			if (this.R2007Plus)
			{
				//xrefindex+1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				this._writer.WriteBitShort((short)(entry.Flags.HasFlag(StandardFlags.XrefDependent) ? 0b100000000 : 0));
			}
			else
			{
				//64-flag B 70 The 64-bit of the 70 group.
				this._writer.WriteBit(entry.Flags.HasFlag(StandardFlags.Referenced));

				//xrefindex + 1 BS 70 subtract one from this value when read.
				//After that, -1 indicates that this reference did not come from an xref,
				//otherwise this value indicates the index of the blockheader for the xref from which this came.
				this._writer.WriteBitShort(0);

				//Xdep B 70 dependent on an xref. (16 bit)
				this._writer.WriteBit(entry.Flags.HasFlag(StandardFlags.XrefDependent));
			}
		}

		private void writeTables()
		{
			this.writeBlockControl();
			this.writeLayerControl();
		}

		private void writeObject<T>(T cadObject)
			where T : CadObject
		{
			switch (cadObject.ObjectType)
			{

				default:
					break;
			}

			this.registerObject(cadObject);
		}

		private void writeBlockControl()
		{
			this.writeTable(this._document.BlockRecords);

			//*MODEL_SPACE and *PAPER_SPACE(hard owner).
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.ModelSpace);
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.PaperSpace);

			this.registerObject(this._document.BlockRecords);
		}

		private void writeLayerControl()
		{
			this.writeTable(this._document.Layers);

			this.registerObject(this._document.Layers);

			foreach (var item in this._document.Layers)
			{
				this.writeLayer(item);
			}
		}

		private void writeLayer(Layer layer)
		{
			this.writeCommonNonEntityData(layer);

			//Common:
			//Entry name TV 2
			this._writer.WriteVariableText(layer.Name);

			this.writeXrefDependantBit(layer);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//Frozen B 70 if frozen (1 bit)
				this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.Frozen));
				//On B if on.
				this._writer.WriteBit(layer.IsOn);
				//Frz in new B 70 if frozen by default in new viewports (2 bit)
				this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.FrozenNewViewports));
				//Locked B 70 if locked (4 bit)
				this._writer.WriteBit(layer.Flags.HasFlag(LayerFlags.Locked));
			}

			//R2000+:
			if (this.R2000Plus)
			{
				//and lineweight (mask with 0x03E0)
				short values = (short)(DwgLineWeightConverter.ToIndex(layer.LineWeight) << 5);

				//contains frozen (1 bit),
				values |= (short)LayerFlags.Frozen;

				//on (2 bit)
				if (layer.IsOn)
					values |= 0b10;

				//frozen by default in new viewports (4 bit)
				values |= (short)LayerFlags.FrozenNewViewports;

				//locked (8 bit)
				values |= (short)LayerFlags.Locked;

				//plotting flag (16 bit),
				if (layer.PlotFlag)
					values |= 0b10000;

				//Values BS 70,290,370
				this._writer.WriteBitShort(values);
			}

			//Common:
			//Color CMC 62
			this._writer.WriteCmColor(layer.Color);

			//Handle refs H Layer control (soft pointer)
			//[Reactors(soft pointer)]
			//xdicobjhandle(hard owner)
			//External reference block handle(hard pointer)
			this._writer.HandleReference(DwgReferenceType.SoftPointer, this._document.Layers);

			//R2000+:
			if (this.R2000Plus)
			{
				//H 390 Plotstyle (hard pointer), by default points to PLACEHOLDER with handle 0x0f.
				this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//H 347 Material
				this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			}

			//Common:
			//H 6 linetype (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, layer.LineType.Handle);

			if (R2013Plus)
			{
				//H Unknown handle (hard pointer). Always seems to be NULL.
				this._writer.HandleReference(DwgReferenceType.HardPointer, 0);
			}

			this.registerObject(layer);
		}

		private void writeTable<T>(Table<T> table)
			where T : TableEntry
		{
			this.writeCommonNonEntityData(table);

			//Common:
			//Numentries BL 70
			this._writer.WriteBitLong(table.Count);

			foreach (var item in table)
			{
				//Handle refs H NULL(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftOwnership, item);
			}
		}

		private void writeCommonNonEntityData(CadObject cadObject)
		{
			this.writeCommonData(cadObject);

			//R13-R14 Only:
			//Obj size RL size of object in bits, not including end handles
			if (this.R13_14Only)
				this._writer.SavePositonForSize();

			//[Owner ref handle (soft pointer)]
			this._writer.HandleReference(DwgReferenceType.SoftPointer, cadObject.Owner.Handle);

			//write the cad object reactors
			this.writeReactorsAndDictionaryHandle(cadObject);
		}

		private void writeCommonEntityData(Entity entity)
		{
			this.writeCommonData(entity);

			//Graphic present Flag B 1 if a graphic is present
			this._writer.WriteBit(false);

			//R13 - R14 Only:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1014)
			{
				this._writer.SavePositonForSize();
			}

			//FE: Entity mode(entmode). Generally, this indicates whether or not the owner
			//relative handle reference is present.The values go as follows:

			//00 : The owner relative handle reference is present.
			//Applies to the following:
			//VERTEX, ATTRIB, and SEQEND.
			//BLOCK, ENDBLK, and the defining entities in all
			//block defs except *MODEL_SPACE and *PAPER_SPACE.

			//01 : PSPACE entity without a owner relative handle ref.
			//10 : MSPACE entity without a owner relative handle ref.
			//11 : Not used.
			byte entmode = getEntMode(entity);
			this._writer.Write2Bits(entmode);
			if (entmode == 0)
			{
				this._writer.HandleReference(DwgReferenceType.SoftPointer, entity.Owner.Handle);
			}

			this.writeReactorsAndDictionaryHandle(entity);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//8 LAYER (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Layer.Handle);

				//Isbylayerlt B 1 if bylayer linetype, else 0
				bool isbylayerlt = entity.LineType.Name == LineType.ByLayerName;
				this._writer.WriteBit(isbylayerlt);
				if (isbylayerlt)
					//6 [LTYPE (hard pointer)] (present if Isbylayerlt is 0)
					this._writer.HandleReference(DwgReferenceType.HardPointer, entity.LineType.Handle);
			}

			//R13-R2000 Only:
			//previous/next handles present if Nolinks is 0.
			//Nolinks B 1 if major links are assumed +1, -1, else 0 For R2004+this always has value 1 (links are not used)
			bool hasLinks = true;
			if (!this.R2004Plus)
			{
				//TODO: Process the entities for before and after

				////[PREVIOUS ENTITY (relative soft pointer)]
				//template.PrevEntity = this.handleReference(entity.Handle);
				////[NEXT ENTITY (relative soft pointer)]
				//template.NextEntity = this.handleReference(entity.Handle);
				//this._writer.WriteBit(hasLinks);

				throw new NotImplementedException();
			}

#if false

			//Color	CMC(B)	62
			this._writer.WriteEnColor(entity.Color, entity.Transparency);

			//R2004+:
			if ((this._version >= ACadVersion.AC1018) && colorFlag)
				//[Color book color handle (hard pointer)]
				template.ColorHandle = this.handleReference();

			//Ltype scale	BD	48
			this._writer.WriteBitDouble(entity.LinetypeScale);

			if (!(this._version >= ACadVersion.AC1015))
			{
				//Common:
				//Invisibility BS 60
				entity.IsInvisible = (this._objectReader.ReadBitShort() & 1) == 0;

				return;
			} 
#endif
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
					throw new NotImplementedException();
				default:
					this._writer.WriteObjectType(cadObject.ObjectType);
					break;
			}

			if (this._version >= ACadVersion.AC1015 && this._version < ACadVersion.AC1024)
				//Obj size RL size of object in bits, not including end handles
				this._writer.SavePositonForSize();

			//Common:
			//Handle H 5 code 0, length followed by the handle bytes.
			this._writer.Main.HandleReference(cadObject);

			//Extended object data, if any
			this.writeExtendedData(cadObject.ExtendedData);
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
				_writer.WriteBit(true);
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

		private byte getEntMode(Entity entity)
		{
			if (entity.Owner.Handle == this._document.PaperSpace.Handle)
			{
				return 0b01;
			}

			if (entity.Owner.Handle == this._document.ModelSpace.Handle)
			{
				return 0b10;
			}

			return 0;
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

		private void writePoint(Point point)
		{
			this.writeCommonEntityData(point);

			//Point 3BD 10
			this._writer.Write3BitDouble(point.Location);
			//Thickness BT 39
			this._writer.WriteBitThickness(point.Thickness);
			//Extrusion BE 210
			this._writer.WriteBitExtrusion(point.Normal);
			//X - axis ang BD 50 See DXF documentation
			this._writer.WriteBitDouble(point.Rotation);

			this.registerObject(point);
		}

		private void Notify(string message, NotificationType type)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, type));
		}
	}
}
