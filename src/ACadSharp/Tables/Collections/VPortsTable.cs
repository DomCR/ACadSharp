namespace ACadSharp.Tables.Collections;

/// <summary>
/// Represents a collection of <see cref="VPort"/> entries.
/// </summary>
public class VPortsTable : Table<VPort>
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.TableVport;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.VPORT_CONTROL_OBJ;

	internal VPortsTable() : base()
	{
	}

	internal VPortsTable(CadDocument document) : base(document)
	{
	}

	/// <inheritdoc/>
	/// <remarks>
	/// VPorts allow duplicated entries with the same name, after the first entry, the others will have their <see cref="CadObject.Handle"/> as a prefix.
	/// </remarks>
	public override void Add(VPort item)
	{
		if (this.Contains(item.Name))
		{
			this.addHandlePrefix(item);
		}
		else
		{
			this.add(item.Name, item);
		}
	}

	protected override string[] getDefaultEntries()
	{ return new string[] { VPort.DefaultName }; }

	protected override VPort getDefaultEntry()
	{
		return this[VPort.DefaultName];
	}
}