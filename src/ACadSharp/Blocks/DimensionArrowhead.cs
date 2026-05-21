using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Blocks;

public static class DimensionArrowhead
{
	public static BlockRecord ClosedBlank
	{
		get
		{
			BlockRecord block = new BlockRecord(ClosedBlankName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6, 0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-1, -1 / 6, 0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6, 0), new XYZ(-1, -1 / 6, 0))));

			return block;
		}
	}

	public static BlockRecord Dot
	{
		get
		{
			BlockRecord block = new BlockRecord(DotName);

			List<LwPolyline.Vertex> vertexes = new();
			vertexes.Add(new LwPolyline.Vertex(-0.25, 0.0) { Bulge = 1.0 });
			vertexes.Add(new LwPolyline.Vertex(0.25, 0.0) { Bulge = 1.0 });

			LwPolyline pline = setDefault(() => new LwPolyline(vertexes));
			pline.IsClosed = true;
			pline.ConstantWidth = 0.5;

			block.Entities.Add(pline);

			Line line = new Line(new XYZ(-0.5, 0.0, 0.0), new XYZ(-1.0, 0.0, 0.0))
			{
				Layer = Layer.Default,
				LineType = LineType.ByBlock,
				Color = Color.ByBlock,
				LineWeight = LineWeightType.ByBlock
			};

			block.Entities.Add(line);

			return block;
		}
	}

	public const string ClosedBlankName = "_ClosedBlank";

	public const string DotName = "_Dot";

	private static T setDefault<T>(Func<T> factory)
		where T : Entity
	{
		T entity = factory();

		entity.Layer = Layer.Default;
		entity.LineType = LineType.ByBlock;
		entity.Color = Color.ByBlock;

		return entity;
	}
}