using ACadSharp.Classes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.XData;
using CSUtilities.Converters;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
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
			_stream.Write(LittleEndianConverter.Instance.GetBytes(crc.Seed), 0, 2);

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

		private void writeCommonData(CadObject cadObject)
		{
			//Reset the current stream to re-write a new object in it
			this._writer.ResetStream();

			switch (cadObject.ObjectType)
			{
				case ObjectType.UNLISTED:
					if (this._document.Classes.TryGetByName(cadObject.ObjectName, out DxfClass dxfClass))
					{
						this._writer.WriteObjectType(dxfClass.ClassNumber);
					}
					else
					{
						this.notify($"Dxf Class not found for {cadObject.ObjectType} fullname: {cadObject.GetType().FullName}", NotificationType.Warning);
						return;
					}
					break;
				case ObjectType.INVALID:
				case ObjectType.UNDEFINED:
					this.notify($"CadObject type: {cadObject.ObjectType} fullname: {cadObject.GetType().FullName}", NotificationType.NotImplemented);
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
			this._writer.Main.HandleReference(cadObject);

			//Extended object data, if any
			this.writeExtendedData(cadObject.ExtendedData);
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

			this.writeEntityMode(entity);
		}

		private void writeEntityMode(Entity entity)
		{
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
				this._writer.HandleReference(DwgReferenceType.SoftPointer, entity.Owner);
			}

			this.writeReactorsAndDictionaryHandle(entity);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//8 LAYER (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Layer);

				//Isbylayerlt B 1 if bylayer linetype, else 0
				bool isbylayerlt = entity.LineType.Name == LineType.ByLayerName;
				this._writer.WriteBit(isbylayerlt);
				if (isbylayerlt)
					//6 [LTYPE (hard pointer)] (present if Isbylayerlt is 0)
					this._writer.HandleReference(DwgReferenceType.HardPointer, entity.LineType);
			}

			//R13-R2000 Only:
			//previous/next handles present if Nolinks is 0.
			//Nolinks B 1 if major links are assumed +1, -1, else 0 For R2004+this always has value 1 (links are not used)
			bool hasLinks = true;
			if (!this.R2004Plus)
			{
				hasLinks = _prev != null
						&& _prev.Handle == entity.Handle - 1
						&& _next != null
						&& _next.Handle == entity.Handle + 1;

				this._writer.WriteBit(hasLinks);

				//[PREVIOUS ENTITY (relative soft pointer)]
				this._writer.HandleReference(DwgReferenceType.SoftPointer, _prev);
				//[NEXT ENTITY (relative soft pointer)]
				this._writer.HandleReference(DwgReferenceType.SoftPointer, _next);
			}

			//Color	CMC(B)	62
			this._writer.WriteEnColor(entity.Color, entity.Transparency, entity.BookColor != null);

			//R2004+:
			if ((this._version >= ACadVersion.AC1018) && entity.BookColor != null)
			{
				//[Color book color handle (hard pointer)]
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.BookColor);
			}

			//Ltype scale	BD	48
			this._writer.WriteBitDouble(entity.LineTypeScale);

			if (!(this._version >= ACadVersion.AC1015))
			{
				//Common:
				//Invisibility BS 60
				this._writer.WriteBitShort((short)(entity.IsInvisible ? 0 : 1));

				return;
			}

			//R2000+:
			//8 LAYER (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, entity.Layer);

			if (entity.LineType.Name == LineType.ByLayerName)
			{
				//Ltype flags BB 00 = bylayer,
				this._writer.Write2Bits(0b00);
			}
			else if (entity.LineType.Name == LineType.ByBlockName)
			{
				//01 = byblock,
				this._writer.Write2Bits(0b01);
			}
			else if (entity.LineType.Name == LineType.ContinuousName)
			{
				//10 = continous,
				this._writer.Write2Bits(0b10);
			}
			else
			{
				//11 = linetype handle present at end of object
				this._writer.Write2Bits(0b11);
				//6 [LTYPE (hard pointer)] present if linetype flags were 11
				this._writer.HandleReference(DwgReferenceType.HardPointer, entity.LineType);
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				this._writer.Write2Bits(0b00);

				//Shadow flags RC
				this._writer.WriteByte(0);
			}

			//R2000 +:
			//Plotstyle flags	BB	00 = bylayer, 01 = byblock, 11 = plotstyle handle present at end of object
			this._writer.Write2Bits(0b00);
			{
				//PLOTSTYLE (hard pointer) present if plotstyle flags were 11
			}

			//R2007 +:
			if (this._version > ACadVersion.AC1021)
			{
				//Material flags BB 00 = bylayer, 01 = byblock, 11 = material handle present at end of object
				this._writer.WriteBit(false);
				{
					//If has full visual style, the full visual style handle (hard pointer).
				}
				this._writer.WriteBit(false);
				{
					//If has full visual style, the full visual style handle (hard pointer).
				}
				//Shadow flags RC
				this._writer.WriteBit(false);
				{
					//If has full visual style, the full visual style handle (hard pointer).
				}
			}

			//Common:
			//Invisibility BS 60
			this._writer.WriteBitShort((short)(entity.IsInvisible ? 1 : 0));

			//R2000+:
			//Lineweight RC 370
			this._writer.WriteByte(CadUtils.ToIndex(entity.LineWeight));
		}

		private void writeExtendedData(ExtendedDataDictionary data)
		{
			if (this.WriteXData)
			{
				//EED size BS size of extended entity data, if any
				foreach (var item in data)
				{
					writeExtendedDataEntry(item.Key, item.Value);
				}
			}

			this._writer.WriteBitShort(0);
		}

		private void writeExtendedDataEntry(AppId app, ExtendedData entry)
		{
			using (MemoryStream mstream = new MemoryStream())
			{
				foreach (ExtendedDataRecord record in entry.Records)
				{
					//Each data item has a 1-byte code (DXF group code minus 1000) followed by the value.
					mstream.WriteByte((byte)(record.Code - 1000));

					switch (record)
					{
						case ExtendedDataBinaryChunk binaryChunk:
							mstream.WriteByte((byte)binaryChunk.Value.Length);
							mstream.Write(binaryChunk.Value, 0, binaryChunk.Value.Length);
							break;
						case ExtendedDataControlString control:
							mstream.WriteByte((byte)(control.Value == '}' ? 1 : 0));
							break;
						case ExtendedDataInteger16 s16:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(s16.Value), 0, 2);
							break;
						case ExtendedDataInteger32 s32:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(s32.Value), 0, 4);
							break;
						case ExtendedDataReal real:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(real.Value), 0, 8);
							break;
						case ExtendedDataScale scale:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(scale.Value), 0, 8);
							break;
						case ExtendedDataDistance dist:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(dist.Value), 0, 8);
							break;
						case ExtendedDataDirection dir:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.X), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.Y), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(dir.Value.Z), 0, 8);
							break;
						case ExtendedDataDisplacement disp:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.X), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.Y), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(disp.Value.Z), 0, 8);
							break;
						case ExtendedDataCoordinate coord:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.X), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.Y), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(coord.Value.Z), 0, 8);
							break;
						case ExtendedDataWorldCoordinate wcoord:
							mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.X), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.Y), 0, 8);
							mstream.Write(LittleEndianConverter.Instance.GetBytes(wcoord.Value.Z), 0, 8);
							break;
						case IExtendedDataHandleReference handle:
							ulong h = handle.Value;
							if (handle.ResolveReference(this._document) == null)
							{
								h = 0;
							}
							mstream.Write(BigEndianConverter.Instance.GetBytes(h), 0, 8);
							break;
						case ExtendedDataString str:
							//same as ReadTextUnicode()
							if (this.R2007Plus)
							{
								mstream.Write(LittleEndianConverter.Instance.GetBytes((ushort)str.Value.Length + 1), 0, 2);
								byte[] bytes = Encoding.Unicode.GetBytes(str.Value);

								mstream.Write(bytes, 0, bytes.Length);
								mstream.WriteByte(0);
								mstream.WriteByte(0);
							}
							else
							{
								byte[] bytes = this._writer.Encoding.GetBytes(string.IsNullOrEmpty(str.Value) ? string.Empty : str.Value);
								mstream.Write(LittleEndianConverter.Instance.GetBytes((ushort)str.Value.Length + 1), 0, 2);
								mstream.Write(bytes, 0, bytes.Length);
								mstream.WriteByte(0);
							}
							break;
						default:
							throw new System.NotSupportedException($"ExtendedDataRecord of type {record.GetType().FullName} not supported.");
					}
				}

				this._writer.WriteBitShort((short)mstream.Length);

				this._writer.Main.HandleReference(DwgReferenceType.HardPointer, app.Handle);

				this._writer.WriteBytes(mstream.GetBuffer(), 0, (int)mstream.Length);
			}
		}

		private void writeReactorsAndDictionaryHandle(CadObject cadObject)
		{
			//Numreactors S number of reactors in this object
			cadObject.CleanReactors();
			this._writer.WriteBitLong(cadObject.Reactors.Count());
			foreach (var item in cadObject.Reactors)
			{
				//[Reactors (soft pointer)]
				this._writer.HandleReference(DwgReferenceType.SoftPointer, item);
			}

			bool noDictionary = cadObject.XDictionary == null;

			//R2004+:
			if (this.R2004Plus)
			{
				this._writer.WriteBit(noDictionary);
				if (!noDictionary)
				{
					this._writer.HandleReference(DwgReferenceType.HardOwnership, cadObject.XDictionary);
				}
			}
			else
			{
				//xdicobjhandle(hard owner)
				this._writer.HandleReference(DwgReferenceType.HardOwnership, cadObject.XDictionary);
			}

			//R2013+:
			if (this.R2013Plus)
			{
				//Has DS binary data B If 1 then this object has associated binary data stored in the data store
				this._writer.WriteBit(false);
			}

			if (!noDictionary)
			{
				_dictionaries.Add(cadObject.XDictionary.Handle, cadObject.XDictionary);
				_objects.Enqueue(cadObject.XDictionary);
			}
		}

		private byte getEntMode(Entity entity)
		{
			if (entity.Owner == null)
			{
				return 0;
			}

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
	}
}
