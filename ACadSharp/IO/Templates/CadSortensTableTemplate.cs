using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadSortensTableTemplate : CadTemplate<SortEntitiesTable>
	{
		public ulong? BlockOwnerHandle { get; set; }

		public CadSortensTableTemplate(SortEntitiesTable cadObject) : base(cadObject)
		{
		}
	}
}
