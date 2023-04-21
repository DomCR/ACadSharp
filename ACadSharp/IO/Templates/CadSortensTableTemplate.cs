using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadSortensTableTemplate : CadTemplate<SortEntitiesTable>
	{
		public ulong? BlockOwnerHandle { get; set; }

		public List<(ulong, ulong)> Values { get; } = new List<(ulong, ulong)> ();

		public CadSortensTableTemplate(SortEntitiesTable cadObject) : base(cadObject)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if(builder.TryGetCadObject(BlockOwnerHandle,out CadObject owner))
			{

			}

			foreach (var item in Values)
			{

			}
		}
	}
}
