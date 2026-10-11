using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Collections.Generic;

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

			// [PATCH] A shape coming from DXF only carries its name (code 2); DWG stores it as
			// a shape number plus a hard pointer to the SHAPEFILE style that owns the .shx where
			// the shape is defined. Without both of them the shape number stays 0 and AutoCAD
			// reports "shape number <= 0 / shape index <= 0" on AUDIT, and the shape is not
			// displayed at all. Resolve the name against the shape files of the document, the
			// same way AutoCAD does when it opens a DXF.
			if (shape.ShapeStyle == null && shape.ShapeIndex == 0 && !string.IsNullOrEmpty(shape.ShapeName))
			{
				List<string> directories = builder.ShapeFontDirectories;
				IEnumerable<string> searchPaths = directories != null && directories.Count > 0
					? directories
					: ShapeFileIndex.GetDefaultDirectories();

				if (ShapeFileIndex.TryResolve(shape.ShapeName, builder.TextStyles, searchPaths, out TextStyle shapeFile, out ushort number))
				{
					shape.ShapeStyle = shapeFile;
					shape.ShapeIndex = number;
				}
				else
				{
					builder.Notify($"Shape {shape.ShapeName} not found in any shape file of the document", NotificationType.Warning);
				}
			}
		}
	}
}
