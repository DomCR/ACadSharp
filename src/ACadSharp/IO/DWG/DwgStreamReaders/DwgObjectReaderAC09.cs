using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG.FileHeaders;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

/* Documentation:
 * The drawings older than R13 store the tables as arrays of records with a fixed size,
 * the entities are stored one after the other, each one preceded by its type, flags and size:
 *
 * - entities section: entities of the model space and, since R11, of the paper space.
 * - blocks section: entities of the block definitions, each one starts with BLOCK and ends with ENDBLK.
 * - extra entities section: entities modified after the drawing was saved for the first time,
 *	 JUMP records chain them with the entities section.
 *
 * There are no handles, the entities reference the table entries by their index in the table.
 * The reader assigns the handles from the position of the entries in the tables (see DwgFileHeaderAC09)
 * and builds the same templates as for the later versions.
 *
 * Source: https://github.com/michal-josef-spacek/CAD-Format-DWG-AC1006 (dwg_ac1006.ksy)
 * Source: https://github.com/michal-josef-spacek/CAD-Format-DWG-AC1009 (dwg_ac1009.ksy)
 */
internal partial class DwgObjectReaderAC09 : DwgSectionIO
{
	public override string SectionName { get { return DwgSectionDefinition.AcDbObjects; } }

	/// <summary>
	/// Seed of the crc of the entities, the table entries and the header variables.
	/// </summary>
	private const ushort _crcSeed = 0xC0C1;

	private readonly DwgDocumentBuilder _builder;

	private readonly DwgFileHeaderAC09 _fileHeader;

	private readonly DwgStreamReaderAC09 _reader;

	/// <summary>
	/// Templates of the block records by their index in the block table.
	/// </summary>
	private readonly Dictionary<int, CadBlockRecordTemplate> _blockRecords = new();

	/// <summary>
	/// Handles for the entities, the drawings older than R13 only store them if HANDLING is enabled.
	/// </summary>
	private ulong _nextHandle;

	public DwgObjectReaderAC09(ACadVersion version, DwgDocumentBuilder builder, DwgStreamReaderAC09 reader, DwgFileHeaderAC09 fileHeader) : base(version)
	{
		this._builder = builder;
		this._reader = reader;
		this._fileHeader = fileHeader;
		this._nextHandle = fileHeader.FirstEntityHandle;
	}

	/// <summary>
	/// Read the tables, the block definitions and the entities.
	/// </summary>
	public void Read()
	{
		this.readTables();

		this.readBlocks();

		DwgSectionLocatorRecord entities = this._fileHeader.EntitiesSection;
		if (entities.Seeker > 0 && entities.Size > 0)
		{
			this.readEntities(entities.Seeker, entities.Seeker + entities.Size, this._modelSpaceTemplate);
		}
	}

	private void checkCrc(long start, long length, string name)
	{
		if (!this.R11Plus || !this._builder.Configuration.CrcCheck)
		{
			return;
		}

		this._reader.Position = start;
		byte[] data = this._reader.ReadBytes((int)length + 2);

		ushort crc = CRC8StreamHandler.GetCRCValue(_crcSeed, data, 0, length);
		ushort expected = (ushort)(data[length] | (data[length + 1] << 8));

		if (crc != expected)
		{
			this._builder.Notify($"Invalid crc for {name}", NotificationType.Warning);
		}
	}

	private ulong nextHandle()
	{
		return this._nextHandle++;
	}

	private long resolveBlockAddress(uint raw)
	{
		DwgSectionLocatorRecord blocks = this._fileHeader.BlocksSection;

		//The highest byte flags the address as relative to the blocks section
		long address = raw & 0x00FFFFFF;
		if ((raw >> 24 & 0x40) != 0 || address < blocks.Seeker)
		{
			address += blocks.Seeker;
		}

		return address;
	}

	private long resolveJumpAddress(uint raw)
	{
		long address = raw & 0x00FFFFFF;

		//0x80 : extra entities section, 0x40 : blocks section, 0 : absolute address
		switch (raw >> 24)
		{
			case 0x80:
				return this._fileHeader.ExtrasSection.Seeker + address;
			case 0x40:
				return this._fileHeader.BlocksSection.Seeker + address;
			default:
				return address;
		}
	}
}
