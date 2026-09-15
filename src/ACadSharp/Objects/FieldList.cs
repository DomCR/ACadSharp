using ACadSharp.Attributes;
using ACadSharp.Classes;
using System.Collections.Generic;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="FieldList"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectFieldList"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.FieldList"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectFieldList)]
[DxfSubClass(DxfSubclassMarker.FieldList)]
public class FieldList : NonGraphicalObject, IDxfClassDefined
{
	/// <summary>
	/// Gets the collection of fields associated with this instance.
	/// </summary>
	/// <remarks>The returned collection is read-only from outside the class. To modify the fields, use the provided
	/// methods of the containing class, if available.</remarks>
	public List<Field> Fields { get; private set; } = new();

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectFieldList;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.FieldList;

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		FieldList clone = base.Clone() as FieldList;

		clone.Fields = new List<Field>();
		foreach (Field f in this.Fields)
		{
			clone.Fields.Add(f.Clone() as Field);
		}

		return clone;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.FieldList,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectFieldList,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}