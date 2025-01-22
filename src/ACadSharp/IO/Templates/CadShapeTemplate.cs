using ACadSharp.Entities;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadShapeTemplate : CadEntityTemplate
	{
		public ushort? ShapeIndex { get; set; }

		public ulong? ShapeFileHandle { get; set; }

		public string ShapeFileName { get; set; }

		public CadShapeTemplate(Shape shape) : base(shape) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Shape shape = this.CadObject as Shape;

			if (this.getTableReference(builder, this.ShapeFileHandle, this.ShapeFileName, out TextStyle text))
			{
				if (text.IsShapeFile)
				{
					shape.ShapeStyle = text;
				}
				else
				{
					builder.Notify($"Shape style {this.ShapeFileHandle} | {this.ShapeFileName} not found", NotificationType.Warning);
				}
			}
		}
	}
}
