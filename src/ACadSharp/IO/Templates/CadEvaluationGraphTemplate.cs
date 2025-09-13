using System;
using System.Collections.Generic;
using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal partial class CadEvaluationGraphTemplate : CadTemplate<EvaluationGraph>
	{
		public List<GraphNodeTemplate> NodeTemplates { get; } = new();

		public CadEvaluationGraphTemplate() : base(new EvaluationGraph()) { }

		public CadEvaluationGraphTemplate(EvaluationGraph evaluationGraph)
			: base(evaluationGraph)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (GraphNodeTemplate item in this.NodeTemplates)
			{
				item.Build(builder);

				this.CadObject.Nodes.Add(item.Node);
			}
		}
	}
}