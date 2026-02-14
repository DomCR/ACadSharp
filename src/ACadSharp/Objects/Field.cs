using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Objects;

[System.Flags]
public enum EvaluationStatusFlags
{
	NotEvaluated = 1,

	Success = 2,

	EvaluatorNotFound = 4,

	SyntaxError = 8,

	InvalidCode = 0x10,

	InvalidContext = 0x20,

	OtherError = 0x40
}

[System.Flags]
public enum FieldStateFlags
{
	Unknown = 0,

	Initialized = 1,

	Compiled = 2,

	Modified = 4,

	Evaluated = 8,

	Cached = 16
}

[System.Flags]
public enum FilingOptionFlags
{
	None = 0,

	NoFieldResult = 1
}

/// <summary>
/// Represents a <see cref="Field"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectField"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Field"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectField)]
[DxfSubClass(DxfSubclassMarker.Field)]
public class Field : NonGraphicalObject
{
	[DxfCodeValue(DxfReferenceType.Count, 97)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 331)]
	//Object ID used in the field code(AcDbSoftPointerId); repeats for the number of object IDs used in the field code
	public List<CadObject> CadObjects { get; set; } = new();

	[DxfCodeValue(DxfReferenceType.Count, 90)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 360)]
	//Child field ID(AcDbHardOwnershipId); repeats for number of children
	public List<Field> Children { get; set; } = new();

	[DxfCodeValue(96)]
	public int EvaluationErrorCode { get; set; }

	[DxfCodeValue(300)]
	public string EvaluationErrorMessage { get; set; }

	[DxfCodeValue(91)]
	public EvaluationOptionFlags EvaluationOptionFlags { get; set; }

	[DxfCodeValue(95)]
	public EvaluationStatusFlags EvaluationStatusFlags { get; set; }

	[DxfCodeValue(1)]
	public string EvaluatorId { get; set; }

	[DxfCodeValue(2)]
	public string FieldCode { get; set; }

	[DxfCodeValue(3)]
	public string FieldCodeOverflow { get; set; }

	[DxfCodeValue(94)]
	public FieldStateFlags FieldStateFlags { get; set; }

	[DxfCodeValue(92)]
	public FilingOptionFlags FilingOptionFlags { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectField;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Field;

	[DxfCodeValue(DxfReferenceType.Count, 93)]
	//Number of the data set in the field
	public List<object> Values { get; set; } = new();

	//6
	//Key string for the field data; a key-field pair is repeated for the number of data sets in the field

	//7
	//Key string for the evaluated cache; this key is hard-coded as ACFD_FIELD_VALUE

	//90

	//Data type of field value
	//140

	//Double value(if data type of field value is double)

	//330

	//ID value, AcDbSoftPointerId (if data type of field value is ID)

	//92

	//Binary data buffer size(if data type of field value is binary)

	//310

	//Binary data(if data type of field value is binary)

	[DxfCodeValue(301)]
	public string FormatString { get; set; }

	[DxfCodeValue(9)]
	public string OverflowFormatString { get; set; }


}