using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Objects;

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
	/// <summary>
	/// Gets or sets the collection of CAD objects associated with this entity.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 97)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 331)]
	public List<CadObject> CadObjects { get; set; } = new();

	/// <summary>
	/// Gets the collection of child fields associated with this object.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 90)]
	[DxfCollectionCodeValue(DxfReferenceType.Handle, 360)]
	public CadObjectCollection<Field> Children { get; private set; }

	/// <summary>
	/// Gets or sets the error code resulting from the evaluation process.
	/// </summary>
	[DxfCodeValue(96)]
	public int EvaluationErrorCode { get; set; }

	/// <summary>
	/// Gets or sets the error message generated during evaluation.
	/// </summary>
	[DxfCodeValue(300)]
	public string EvaluationErrorMessage { get; set; }

	/// <summary>
	/// Gets or sets the evaluation option flags that control how the associated expression is evaluated.
	/// </summary>
	[DxfCodeValue(91)]
	public EvaluationOptionFlags EvaluationOptionFlags { get; set; }

	/// <summary>
	/// Gets or sets the evaluation status flags that indicate the current state of the object evaluation process.
	/// </summary>
	/// <remarks>These flags provide information about the outcome or progress of the evaluation, such as whether
	/// the evaluation succeeded, failed, or is in progress. The specific meaning of each flag is defined by the
	/// EvaluationStatusFlags enumeration.</remarks>
	[DxfCodeValue(95)]
	public EvaluationStatusFlags EvaluationStatusFlags { get; set; }

	/// <summary>
	/// Gets or sets the identifier of the evaluator associated with this entity.
	/// </summary>
	[DxfCodeValue(1)]
	public string EvaluatorId { get; set; }

	/// <summary>
	/// Gets or sets the code string representing the field definition in the DXF file.
	/// </summary>
	/// <remarks>The field code typically contains the raw expression or formula used in the field. This value may
	/// be used for parsing or evaluating the field within DXF processing workflows.</remarks>
	[DxfCodeValue(2)]
	public string FieldCode { get; set; }

	/// <summary>
	/// Gets or sets the flags that indicate the current state of the field.
	/// </summary>
	[DxfCodeValue(94)]
	public FieldStateFlags FieldStateFlags { get; set; }

	/// <summary>
	/// Gets or sets the set of flags that specify filing options for the associated entity.
	/// </summary>
	/// <remarks>The filing option flags determine how the entity is processed or stored during serialization. The
	/// specific meaning of each flag depends on the context in which the entity is used. Refer to the documentation for
	/// the FilingOptionFlags enumeration for details on available options.</remarks>
	[DxfCodeValue(92)]
	public FilingOptionFlags FilingOptionFlags { get; set; }

	/// <summary>
	/// Gets or sets the format string used to control how values are displayed or serialized.
	/// </summary>
	/// <remarks>The format string determines the representation of values when output or stored. The expected
	/// format depends on the context in which this property is used. If the format string is invalid or unsupported, the
	/// output may not be formatted as intended.</remarks>
	[DxfCodeValue(301)]
	public string FormatString { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectField;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Field;

	[DxfCodeValue(7)]
	public CadValue Value { get; set; } = new();

	/// <summary>
	/// Gets or sets the collection of CAD values associated with the field.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 93)]
	[DxfCollectionCodeValue(6)]
	public Dictionary<string, CadValue> Values { get; private set; } = new(StringComparer.InvariantCultureIgnoreCase);

	/// <inheritdoc/>
	public Field() : base()
	{
		this.Children = new(this);
	}

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

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		doc.RegisterCollection(this.Children);
	}

	internal override void UnassignDocument()
	{
		this.Document.UnregisterCollection(this.Children);

		base.UnassignDocument();
	}
}