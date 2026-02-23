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

	[DxfCodeValue(2)]
	public string FieldCode { get; set; }

	[DxfCodeValue(94)]
	public FieldStateFlags FieldStateFlags { get; set; }

	[DxfCodeValue(92)]
	public FilingOptionFlags FilingOptionFlags { get; set; }

	[DxfCodeValue(301)]
	public string FormatString { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectField;

	//Binary data(if data type of field value is binary)
	[DxfCodeValue(9)]
	public string OverflowFormatString { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Field;

	/// <inheritdoc/>
	public Field() : base()
	{
		this.Children = new(this);
	}

	/// <summary>
	/// Gets or sets the collection of CAD values associated with the field.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 93)]
	[DxfCollectionCodeValue(6)]
	public Dictionary<string, CadValue> Values { get; private set; } = new(StringComparer.InvariantCultureIgnoreCase);

	[DxfCodeValue(7)]
	public CadValue Value { get; set; }

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

	//301	Format string

	//9	Overflow of format string

	//98	Length of format string

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