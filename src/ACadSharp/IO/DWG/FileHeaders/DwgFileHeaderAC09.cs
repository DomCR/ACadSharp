using ACadSharp.IO.DWG.FileHeaders;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG;

/// <summary>
/// File header of the drawings older than R13: R10 (AC1006) and R11/R12 (AC1009).
/// </summary>
/// <remarks>
/// These drawings have no handles for the tables and their entries, the entities reference them by their index.
/// The reader assigns a handle to every table, entry and block so the references can be resolved by the builder.
/// </remarks>
internal class DwgFileHeaderAC09 : DwgFileHeader
{
	public class TableDescriptor
	{
		public int ItemSize { get; set; }

		public int Count { get; set; }

		public long Seeker { get; set; }

		public bool IsEmpty { get { return this.Count <= 0 || this.ItemSize <= 0 || this.Seeker <= 0; } }

		public override string ToString()
		{
			return $"Count : {this.Count} | Size : {this.ItemSize} | Seeker : {this.Seeker}";
		}
	}

	/// <summary>
	/// Order of the tables, the handles of the entries are assigned in this order.
	/// </summary>
	public static readonly string[] TableNames = new string[]
	{
		DxfFileToken.TableBlockRecord,
		DxfFileToken.TableLayer,
		DxfFileToken.TableStyle,
		DxfFileToken.TableLinetype,
		DxfFileToken.TableView,
		DxfFileToken.TableUcs,
		DxfFileToken.TableVport,
		DxfFileToken.TableAppId,
		DxfFileToken.TableDimstyle,
	};

	public DwgSectionLocatorRecord BlocksSection { get; set; } = new DwgSectionLocatorRecord();

	public DwgSectionLocatorRecord EntitiesSection { get; set; } = new DwgSectionLocatorRecord();

	public DwgSectionLocatorRecord ExtrasSection { get; set; } = new DwgSectionLocatorRecord();

	/// <summary>
	/// Handle of the first entity, the entities get the handles after the table entries.
	/// </summary>
	public ulong FirstEntityHandle { get { return this.GetEntryHandle(DxfFileToken.TableDimstyle, this.getTable(DxfFileToken.TableDimstyle).Count); } }

	public long HeaderVariablesSeeker { get; set; }

	public short HeaderVariablesCount { get; set; }

	public ulong ModelSpaceHandle { get { return (ulong)TableNames.Length + 1; } }

	public ulong PaperSpaceHandle { get { return (ulong)TableNames.Length + 2; } }

	public Dictionary<string, TableDescriptor> Tables { get; } = new Dictionary<string, TableDescriptor>(StringComparer.OrdinalIgnoreCase);

	private const ulong _firstEntryHandle = 0x10;

	public DwgFileHeaderAC09() : base() { }

	public DwgFileHeaderAC09(ACadVersion version) : base(version) { }

	public override void AddSection(string name)
	{
		throw new NotImplementedException();
	}

	public override DwgSectionDescriptor GetDescriptor(string name)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Handle assigned to the entry of a table.
	/// </summary>
	/// <param name="tableName">Table of the entry.</param>
	/// <param name="index">Index of the entry in the table.</param>
	public ulong GetEntryHandle(string tableName, int index)
	{
		ulong handle = _firstEntryHandle;

		foreach (string name in TableNames)
		{
			if (name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
			{
				return handle + (ulong)index;
			}

			handle += (ulong)this.getTable(name).Count;
		}

		throw new ArgumentException($"Table {tableName} not found in the drawing.", nameof(tableName));
	}

	/// <summary>
	/// Handle assigned to the control object of a table.
	/// </summary>
	public ulong GetTableHandle(string tableName)
	{
		int index = Array.FindIndex(TableNames, n => n.Equals(tableName, StringComparison.OrdinalIgnoreCase));
		if (index < 0)
		{
			throw new ArgumentException($"Table {tableName} not found in the drawing.", nameof(tableName));
		}

		return (ulong)index + 1;
	}

	private TableDescriptor getTable(string name)
	{
		if (this.Tables.TryGetValue(name, out TableDescriptor table))
		{
			return table;
		}

		return new TableDescriptor();
	}
}
