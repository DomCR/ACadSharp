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

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			Shape shape = this.CadObject as Shape;

			// [PATCH] Keep the raw shape name: when the STYLES table has no matching
			// entry the reference resolves to null and the name would be lost, but DXF
			// code 2 is mandatory (AutoCAD aborts the file with "Missing group code 2").
			shape.ShapeName = this.ShapeFileName;

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
