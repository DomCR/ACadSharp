using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadShapeTemplate : DwgEntityTemplate
	{
		public ushort? ShapeIndex { get; set; }
		public ulong? ShapeFileHandle { get; set; }

		public CadShapeTemplate(Shape shape) : base(shape) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//TODO: Finish shape template
		}
	}
}
