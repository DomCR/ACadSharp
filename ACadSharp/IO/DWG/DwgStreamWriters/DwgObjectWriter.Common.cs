using ACadSharp.Entities;
using ACadSharp.Tables;
using CSUtilities.Converters;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.AcDbObjects;

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
			this._writer.WriteBitShort(0);
			//TODO: Implement write en color
			//this._writer.WriteEnColor(entity.Color, entity.Transparency);

			//R2004+:
			//if ((this._version >= ACadVersion.AC1018) && colorFlag)
			//	//[Color book color handle (hard pointer)]
			//	template.ColorHandle = this.handleReference();

			//Ltype scale	BD	48
			this._writer.WriteBitDouble(entity.LinetypeScale);

			if (!(this._version >= ACadVersion.AC1015))
			{
				//Common:
				//Invisibility BS 60
				this._writer.WriteBitShort((short)(entity.IsInvisible ? 1 : 0));

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
	}
}
