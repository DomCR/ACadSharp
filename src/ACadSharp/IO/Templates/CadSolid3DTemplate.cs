using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadSolid3DTemplate : CadEntityTemplate<Solid3D>
	{
		public ulong? HistoryHandle { get; set; }

		public CadSolid3DTemplate() : base(new Solid3D())
		{
		}

		public CadSolid3DTemplate(Solid3D solid) : base(solid)
		{
		}
	}
}