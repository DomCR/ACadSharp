using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using System;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectReader : DwgSectionIO
{
	private void readAnnotScaleObjectContextData(CadAnnotScaleObjectContextDataTemplate template)
	{
		this.readObjectContextData(template);

		template.ScaleHandle = this.handleReference();
	}

	private CadTemplate readBlkRefObjectContextData()
	{
		BlockReferenceObjectContextData contextData = new BlockReferenceObjectContextData();
		CadAnnotScaleObjectContextDataTemplate template = new CadAnnotScaleObjectContextDataTemplate(contextData);

		this.readAnnotScaleObjectContextData(template);

		contextData.Rotation = this._mergedReaders.ReadBitDouble();
		contextData.InsertionPoint = this._mergedReaders.Read3BitDouble();
		contextData.XScale = this._mergedReaders.ReadBitDouble();
		contextData.YScale = this._mergedReaders.ReadBitDouble();
		contextData.ZScale = this._mergedReaders.ReadBitDouble();

		return template;
	}

	private void readBlock1PtParameter(CadBlock1PtParameterTemplate template)
	{
		this.readBlockParameter(template);

		//1010 1020 1030
		template.Block1PtParameter.Location = this._mergedReaders.Read3BitDouble();
		//170
		template.Block1PtParameter.Value170 = this._mergedReaders.ReadBitShort();
		//171
		template.Block1PtParameter.Value171 = this._mergedReaders.ReadBitShort();
		//93
		template.Block1PtParameter.Value93 = this._mergedReaders.ReadBitLong();
	}

	private void readBlock2PtParameter(CadBlock2PtParameterTemplate template)
	{
		this.readBlockParameter(template);

		//1010 1020 1030
		template.Block2PtParameter.FirstPoint = this._mergedReaders.Read3BitDouble();
		//1011 1021 1031
		template.Block2PtParameter.SecondPoint = this._mergedReaders.Read3BitDouble();

		//170 (always 4)
		for (int i = 0; i < 4; i++)
		{
			//171 172 173 174
			short n = this._mergedReaders.ReadBitShort();
			for (int j = 0; j < n; j++)
			{
				//94 95 (I guess 96 97)
				var d = this._mergedReaders.ReadBitLong();
				//303 304
				var e = this._mergedReaders.ReadVariableText();
			}
		}

		for (int k = 0; k < 4; k++)
		{
			//91 values
			var f = this._mergedReaders.ReadBitLong();
		}

		var value177 = this._mergedReaders.ReadBitShort();
	}

	private void readBlockAction(CadBlockActionTemplate template)
	{
		this.readBlockElement(template);

		BlockAction blockAction = template.BlockAction;

		// 1010, 1020, 1030
		blockAction.ActionPoint = this._mergedReaders.Read3BitDouble();

		//71
		short entityCount = this._objectReader.ReadBitShort();
		for (int i = 0; i < entityCount; i++)
		{
			ulong entityHandle = this.handleReference();
			template.EntityHandles.Add(entityHandle);
		}

		// 70
		blockAction.Value70 = this._mergedReaders.ReadBitShort();
	}

	private void readBlockActionBasePt(CadBlockActionBasePtTemplate template)
	{
		this.readBlockAction(template);

		BlockActionBasePt blockActionBasePt = template.CadObject as BlockActionBasePt;

		blockActionBasePt.Value1011 = this._mergedReaders.Read3BitDouble();

		blockActionBasePt.Value92 = this._mergedReaders.ReadBitLong();
		blockActionBasePt.Value301 = this._mergedReaders.ReadVariableText();
		blockActionBasePt.Value93 = this._mergedReaders.ReadBitLong();
		blockActionBasePt.Value302 = this._mergedReaders.ReadVariableText();

		blockActionBasePt.Value280 = this._mergedReaders.ReadBit();
		blockActionBasePt.Value1012 = this._mergedReaders.Read3BitDouble();
	}

	private void readBlockElement(CadBlockElementTemplate template)
	{
		this.readEvaluationExpression(template);

		//300 name
		template.BlockElement.ElementName = this._mergedReaders.ReadVariableText();
		//98
		template.BlockElement.Value98 = this._mergedReaders.ReadBitLong();
		//99
		template.BlockElement.Value99 = this._mergedReaders.ReadBitLong();
		//1071
		template.BlockElement.Value1071 = this._mergedReaders.ReadBitLong();
	}

	private void readBlockGrip(CadBlockGripTemplate template)
	{
		this.readBlockElement(template);

		var blockGrip = template.CadObject as BlockGrip;

		blockGrip.Value91 = this._mergedReaders.ReadBitLong();
		blockGrip.Value92 = this._mergedReaders.ReadBitLong();
		blockGrip.Location = this._mergedReaders.Read3BitDouble();
		blockGrip.Value280 = this._mergedReaders.ReadBitAsShort();
		blockGrip.Value93 = this._mergedReaders.ReadBitLong();
	}

	private CadTemplate readBlockGripLocationComponent()
	{
		BlockGripExpression gripExpression = new BlockGripExpression();
		CadBlockGripExpressionTemplate template = new CadBlockGripExpressionTemplate(gripExpression);

		this.readEvaluationExpression(template);

		return template;
	}

	private void readBlockParameter(CadBlockParameterTemplate template)
	{
		this.readBlockElement(template);

		//280
		template.BlockParameter.Value280 = this._mergedReaders.ReadBit();
		//281
		template.BlockParameter.Value281 = this._mergedReaders.ReadBit();
	}

	private CadTemplate readBlockRepresentationData()
	{
		BlockRepresentationData representation = new BlockRepresentationData();
		CadBlockRepresentationDataTemplate template = new CadBlockRepresentationDataTemplate(representation);

		this.readCommonNonEntityData(template);

		representation.Value70 = this._mergedReaders.ReadBitShort();
		template.BlockHandle = this.handleReference();

		return template;
	}

	private CadTemplate readBlockRotateAction()
	{
		BlockRotationAction rotationAction = new();
		CadBlockRotationActionTemplate template = new(rotationAction);

		this.readBlockActionBasePt(template);

		rotationAction.Value94 = this._mergedReaders.ReadBitLong();
		rotationAction.Value303 = this._mergedReaders.ReadVariableText();

		return template;
	}

	private CadTemplate readBlockRotationParameter()
	{
		BlockRotationParameter blockRotationParameter = new();
		CadBlockRotationParameterTemplate template = new CadBlockRotationParameterTemplate(blockRotationParameter);

		this.readBlock2PtParameter(template);

		//1011 1021 1031
		blockRotationParameter.Point = this._mergedReaders.Read3BitDouble();
		//305
		blockRotationParameter.Name = this._mergedReaders.ReadVariableText();
		//306
		blockRotationParameter.Description = this._mergedReaders.ReadVariableText();
		//140
		blockRotationParameter.NameOffset = this._mergedReaders.ReadBitDouble();

		//307 missing text?

		blockRotationParameter.Value96 = this._mergedReaders.ReadBitLong();
		blockRotationParameter.Value141 = this._mergedReaders.ReadBitDouble();
		blockRotationParameter.Value142 = this._mergedReaders.ReadBitDouble();
		blockRotationParameter.Value143 = this._mergedReaders.ReadBitDouble();

		blockRotationParameter.Value175 = this._mergedReaders.ReadBitLong();

		return template;
	}

	private CadTemplate readBlockVisibilityParameter()
	{
		BlockVisibilityParameter blockVisibilityParameter = new BlockVisibilityParameter();
		CadBlockVisibilityParameterTemplate template = new CadBlockVisibilityParameterTemplate(blockVisibilityParameter);

		this.readBlock1PtParameter(template);

		//281
		blockVisibilityParameter.Value281 = this._mergedReaders.ReadBit();
		//301
		blockVisibilityParameter.Name = this._mergedReaders.ReadVariableText();
		//302
		blockVisibilityParameter.Description = this._mergedReaders.ReadVariableText();
		//missing bit??	91 should be an int
		blockVisibilityParameter.Value91 = this._mergedReaders.ReadBit();

		//DXF 93 Total entities count
		var totalEntitiesCount = this._objectReader.ReadBitLong();
		for (int i = 0; i < totalEntitiesCount; i++)
		{
			//331
			template.EntityHandles.Add(this.handleReference());
		}

		//DXF 92 states count
		var nstates = this._objectReader.ReadBitLong();
		for (int j = 0; j < nstates; j++)
		{
			template.StateTemplates.Add(this.readState());
		}

		return template;
	}

	private void readEvaluationExpression(CadEvaluationExpressionTemplate template)
	{
		this.readCommonNonEntityData(template);

		//AcDbEvalExpr
		var unknown = this._objectReader.ReadBitLong();

		//98
		template.CadObject.Value98 = this._objectReader.ReadBitLong();
		//99
		template.CadObject.Value99 = this._objectReader.ReadBitLong();

		//Code value
		short code = this._mergedReaders.ReadBitShort();
		if (code > 0)
		{
			var groupValue = GroupCodeValue.TransformValue(code);
			switch (groupValue)
			{
				case GroupCodeValueType.Double:
				case GroupCodeValueType.ExtendedDataDouble:
					this._mergedReaders.ReadBitDouble();
					break;
				default:
					throw new System.NotImplementedException($"[EvaluationExpression] Code not implemented {groupValue}");
			}
		}

		//90
		template.CadObject.Id = this._objectReader.ReadBitLong();
	}

	private CadTemplate readField()
	{
		var field = new Field();
		CadFieldTemplate template = new CadFieldTemplate(field);

		this.readCommonNonEntityData(template);

		//TV 1 Evaluator ID TV 2,3 Field code(in DXF strings longer than 255 characters
		//are written in chunks of 255 characters in one 2 group and one or
		//more 3 groups).
		field.EvaluatorId = this._mergedReaders.ReadVariableText();
		field.FieldCode = this._mergedReaders.ReadVariableText();
		//BL 90 Number of child fields
		int nchild = this._mergedReaders.ReadBitLong();
		for (int i = 0; i < nchild; i++)
		{
			//H 360 Child field handle (hard owner)
			template.ChildrenHandles.Add(this.handleReference());
		}

		//BL 97 Number of field objects
		int nfields = this._mergedReaders.ReadBitLong();
		for (int j = 0; j < nfields; j++)
		{
			//H 331 Field object handle (soft pointer)
			template.CadObjectsHandles.Add(this.handleReference());
		}

		//-R2004
		if (this._version < ACadVersion.AC1021)
		{
			//TV 4 Format string. After R2004 the format became part of the value object.
			field.FormatString = this._mergedReaders.ReadVariableText();
		}

		//Common BL 91 Evaluation option flags:
		field.EvaluationOptionFlags = (EvaluationOptionFlags)this._mergedReaders.ReadBitLong();
		//BL 92 Filing option flags:
		field.FilingOptionFlags = (FilingOptionFlags)this._mergedReaders.ReadBitLong();
		//BL 96 Evaluation error code
		field.FieldStateFlags = (FieldStateFlags)this._mergedReaders.ReadBitLong();
		//BL 94 Field state flags:
		field.EvaluationStatusFlags = (EvaluationStatusFlags)this._mergedReaders.ReadBitLong();
		//BL 96 Evaluation error code
		field.EvaluationErrorCode = this._mergedReaders.ReadBitLong();
		//TV 300 Evaluation error message
		field.EvaluationErrorMessage = this._mergedReaders.ReadVariableText();

		//... ... The field value, see paragraph 20.4.99.
		template.CadValueTemplates.Add(this.readCadValue(field.Value));

		//TV 301,9 Value string(DXF: written in 255 character chunks)
		field.FormatString = this._mergedReaders.ReadVariableText();
		this._mergedReaders.ReadBitLong();
		int num3 = this._mergedReaders.ReadBitLong();
		for (int k = 0; k < num3; k++)
		{
			//TV 6 Child field key
			string key = this._mergedReaders.ReadVariableText();
			CadValue value = new CadValue();
			template.CadValueTemplates.Add(this.readCadValue(value));
			field.Values.Add(key, value);
		}

		return template;
	}

	private CadValueTemplate readCadValue(CadValue value)
	{
		CadValueTemplate template = new CadValueTemplate(value);

		//R2007+:
		if (this.R2007Plus)
		{
			//Flags BL 93 Flags & 0x01 => type is kGeneral
			value.Flags = this._mergedReaders.ReadBitLong();
		}

		//Common:
		//Data type BL 90
		value.ValueType = (CadValueType)this._mergedReaders.ReadBitLong();
		if (!this.R2007Plus || !value.IsEmpty)
		{
			//Varies by type: Not present in case bit 1 in Flags is set
			switch (value.ValueType)
			{
				case CadValueType.Unknown:
				case CadValueType.Long:
					value.Value = this._mergedReaders.ReadBitLong();
					break;
				case CadValueType.Double:
					value.Value = this._mergedReaders.ReadBitDouble();
					break;
				case CadValueType.General:
				case CadValueType.String:
					value.Value = this.readStringCadValue();
					break;
				case CadValueType.Date:
					System.DateTime? dateTime = this.readDateCadValue();
					if (dateTime.HasValue)
					{
						value.Value = dateTime.Value;
					}
					break;
				case CadValueType.Point2D:
					value.Value = this.readCellValueXY();
					break;
				case CadValueType.Point3D:
					value.Value = this.readCellValueXYZ();
					break;
				case CadValueType.Handle:
					template.ValueHandle = this.handleReference();
					break;
				case CadValueType.Buffer:
				case CadValueType.ResultBuffer:
				default:
					throw new NotImplementedException();
			}
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Unit type BL 94 0 = no units, 1 = distance, 2 = angle, 4 = area, 8 = volume
			value.Units = (CadValueUnitType)this._mergedReaders.ReadBitLong();
			//Format String TV 300
			value.Format = this._mergedReaders.ReadVariableText();
			//Value String TV 302
			value.FormattedValue = this._mergedReaders.ReadVariableText();
		}

		return template;
	}

	private CadTemplate readFieldList()
	{
		FieldList fieldList = new FieldList();
		CadFieldListTemplate template = new CadFieldListTemplate(fieldList);

		this.readCommonNonEntityData(template);

		//BL Number of fields
		int nhandles = this._mergedReaders.ReadBitLong();
		//B Unknown
		this._mergedReaders.ReadBit();
		for (int i = 0; i < nhandles; i++)
		{
			//H 330 Field handle (soft pointer)
			template.OwnedObjectsHandlers.Add(this.handleReference());
		}

		return template;
	}

	private CadTemplate readMTextAttributeObjectContextData()
	{
		//TODO: MTextAttributeObjectContextData for dwg
		MTextAttributeObjectContextData contextData = new();
		CadAnnotScaleObjectContextDataTemplate template = new CadAnnotScaleObjectContextDataTemplate(contextData);

		//this.readAnnotScaleObjectContextData(template);

		return null;
	}

	private void readObjectContextData(CadTemplate template)
	{
		this.readCommonNonEntityData(template);

		ObjectContextData contextData = (ObjectContextData)template.CadObject;

		//BS	70	Version (default value is 3).
		contextData.Version = _objectReader.ReadBitShort();
		//B	290	Default flag (default value is false).
		contextData.Default = _objectReader.ReadBit();
	}

	private CadBlockVisibilityParameterTemplate.StateTemplate readState()
	{
		CadBlockVisibilityParameterTemplate.StateTemplate template = new CadBlockVisibilityParameterTemplate.StateTemplate();

		template.State.Name = this._textReader.ReadVariableText();

		//DXF 94 subset count 1
		int n1 = this._objectReader.ReadBitLong();
		for (int i = 0; i < n1; i++)
		{
			//332
			template.EntityHandles.Add(this.handleReference());
		}

		//DXF 95 subset count 2
		var n2 = this._objectReader.ReadBitLong();
		for (int i = 0; i < n2; i++)
		{
			//333
			template.ExpressionHandles.Add(this.handleReference());
		}

		return template;
	}
}