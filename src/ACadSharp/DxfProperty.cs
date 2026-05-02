using ACadSharp.Attributes;
using ACadSharp.XData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp;

/// <summary>
/// Represents a property of a CAD object that is associated with one or more DXF codes and provides access to its value
/// and related metadata.
/// </summary>
/// <remarks>Use the DxfProperty class to map object properties to DXF codes for serialization or processing
/// within DXF-based workflows. The property referenced must be decorated with the DxfCodeValueAttribute. This class
/// supports retrieving DXF code collections, accessing property values from CAD objects, and generating extended data
/// records for use in DXF files.</remarks>
public class DxfProperty : DxfPropertyBase<DxfCodeValueAttribute>
{
	/// <summary>
	/// Creates a dxf property referenced to an object property
	/// </summary>
	/// <remarks>
	/// The property must have the <see cref="DxfCodeValueAttribute"/>
	/// </remarks>
	/// <param name="property"></param>
	/// <exception cref="ArgumentException"></exception>
	public DxfProperty(PropertyInfo property) : base(property) { }

	/// <summary>
	/// Creates a dxf property referenced to an object property
	/// </summary>
	/// <remarks>
	/// The property must have the <see cref="DxfCodeValueAttribute"/>
	/// </remarks>
	/// <param name="code">assigned value for this property, only useful if the property has multiple codes assigned</param>
	/// <param name="property"></param>
	/// <exception cref="ArgumentException"></exception>
	public DxfProperty(int code, PropertyInfo property) : this(property)
	{
		if (!this._attributeData.ValueCodes.Contains((DxfCode)code))
			throw new ArgumentException($"The {nameof(DxfCodeValueAttribute)} does not have match with the code {code}", nameof(property));

		this._assignedCode = code;
	}

	/// <summary>
	/// Initializes a new instance of the DxfProperty class by copying the property information from the specified
	/// DxfProperty instance.
	/// </summary>
	/// <param name="value">The DxfProperty instance from which to copy property information. Cannot be null.</param>
	public DxfProperty(DxfProperty value) : this(value._property)
	{
	}

	/// <summary>
	/// Gets the collection of DXF codes associated with the property, if defined.
	/// </summary>
	/// <remarks>Use this method to retrieve custom DXF code mappings specified by the <see
	/// cref="DxfCollectionCodeValueAttribute"/> on the property. Returns <see langword="null"/> if the attribute is not
	/// present.</remarks>
	/// <returns>An array of <see cref="DxfCode"/> values representing the collection codes for the property; or <see
	/// langword="null"/> if no collection codes are defined.</returns>
	public DxfCode[] GetCollectionCodes()
	{
		return this._property.GetCustomAttribute<DxfCollectionCodeValueAttribute>()?.ValueCodes;
	}

	/// <summary>
	/// Gets the value of the specified property from the given CAD object.
	/// </summary>
	/// <typeparam name="TCadObject">The type of CAD object from which to retrieve the property value. Must derive from CadObject.</typeparam>
	/// <param name="obj">The CAD object instance from which to retrieve the property value. Cannot be null.</param>
	/// <returns>The value of the property for the specified CAD object.</returns>
	public object GetValue<TCadObject>(TCadObject obj)
		where TCadObject : CadObject
	{
		return this._property.GetValue(obj);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		string str = string.Empty;

		foreach (int code in this.DxfCodes)
		{
			str += $"{code}:";
		}

		str += this._property.Name;

		return str;
	}

	/// <summary>
	/// Creates a collection of extended data records representing the current object's assigned code and stored value.
	/// </summary>
	/// <remarks>The returned collection always contains at most two records: one for the assigned code and one for
	/// the stored value, if present.</remarks>
	/// <returns>An enumerable collection of <see cref="ExtendedDataRecord"/> objects containing the assigned code and stored value.
	/// Returns an empty collection if the stored value is <see langword="null"/>.</returns>
	public IEnumerable<ExtendedDataRecord> ToXDataRecords()
	{
		List<ExtendedDataRecord> records = new List<ExtendedDataRecord>();
		if (this.StoredValue == null)
		{
			return records;
		}

		records.Add(new ExtendedDataInteger16((short)this.AssignedCode));
		records.Add(ExtendedDataRecord.Create(this.GroupCode, this.StoredValue));

		return records;
	}
}