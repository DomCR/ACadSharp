using ACadSharp.IO.Templates;
using ACadSharp.Objects.Evaluations;
using System.Diagnostics;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectReader : DwgSectionIO
	{
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
}