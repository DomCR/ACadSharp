using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Blocks;

/// <summary>
/// Provides predefined dimension arrowhead block records used in dimensioning.
/// </summary>
public static class DimensionArrowhead
{
	/// <summary>
	/// Gets a block record for the Architectural Tick arrowhead.
	/// </summary>
	public static BlockRecord ArchitecturalTick
	{
		get
		{
			BlockRecord block = new BlockRecord(ArchitecturalTickName);

			List<LwPolyline.Vertex> vertices = new();
			vertices.Add(new LwPolyline.Vertex(-0.5, -0.5));
			vertices.Add(new LwPolyline.Vertex(0.5, 0.5));

			LwPolyline pline = setDefault(() => new LwPolyline(vertices));
			pline.IsClosed = true;
			pline.ConstantWidth = 0.15;

			block.Entities.Add(pline);

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Box Blank arrowhead.
	/// </summary>
	public static BlockRecord BoxBlank
	{
		get
		{
			BlockRecord block = new BlockRecord(BoxBlankName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, -0.5, 0.0), new XYZ(0.5, -0.5, 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(0.5, -0.5, 0.0), new XYZ(0.5, 0.5, 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(0.5, 0.5, 0.0), new XYZ(-0.5, 0.5, 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.5, 0.0), new XYZ(-0.5, -0.5, 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.0, 0.0), new XYZ(-1.0, 0.0, 0.0))));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Box Filled arrowhead.
	/// </summary>
	public static BlockRecord BoxFilled
	{
		get
		{
			BlockRecord block = new BlockRecord(BoxFilledName);

			Solid solid = setDefault(() => new Solid(
				new XYZ(-0.5, 0.5, 0.0),
				new XYZ(0.5, 0.5, 0.0),
				new XYZ(-0.5, -0.5, 0.0),
				new XYZ(0.5, -0.5, 0.0)));
			block.Entities.Add(solid);

			Line line = setDefault(() => new Line(new XYZ(-0.5, 0.0, 0.0), new XYZ(-1.0, 0.0, 0.0)));
			block.Entities.Add(line);

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Closed arrowhead.
	/// </summary>
	public static BlockRecord Closed
	{
		get
		{
			BlockRecord block = new BlockRecord(ClosedName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6.0d, 0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-1, -1 / 6.0d, 0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6.0d, 0), new XYZ(-1, -1 / 6.0d, 0))));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Closed Blank arrowhead.
	/// </summary>
	public static BlockRecord ClosedBlank
	{
		get
		{
			BlockRecord block = new BlockRecord(ClosedBlankName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6.0d, 0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-1, -1 / 6.0d, 0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 1 / 6.0d, 0), new XYZ(-1, -1 / 6.0d, 0))));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Datum Filled arrowhead.
	/// </summary>
	public static BlockRecord DatumFilled
	{
		get
		{
			BlockRecord block = new BlockRecord(DatumFilledName);

			Solid solid = setDefault(() => new Solid(
				new XYZ(0.0, 0.57735027, 0.0),
				new XYZ(-1.0, 0.0, 0.0),
				new XYZ(0.0, -0.57735027, 0.0)));
			block.Entities.Add(solid);

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Datum Triangle (blank) arrowhead.
	/// </summary>
	public static BlockRecord DatumTriangle
	{
		get
		{
			BlockRecord block = new BlockRecord(DatumBlankName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(0.0, 1 / Math.Sqrt(3), 0.0), new XYZ(-1.0, 0.0, 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-1.0, 0.0, 0.0), new XYZ(0.0, -(1 / Math.Sqrt(3)), 0.0))));
			block.Entities.Add(setDefault(() => new Line(new XYZ(0.0, -(1 / Math.Sqrt(3)), 0.0), new XYZ(0.0, 1 / Math.Sqrt(3), 0.0))));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Dot (filled) arrowhead.
	/// </summary>
	public static BlockRecord Dot
	{
		get
		{
			BlockRecord block = new BlockRecord(DotName);

			List<LwPolyline.Vertex> vertices = new();
			vertices.Add(new LwPolyline.Vertex(-0.25, 0.0) { Bulge = 1.0 });
			vertices.Add(new LwPolyline.Vertex(0.25, 0.0) { Bulge = 1.0 });

			LwPolyline pline = setDefault(() => new LwPolyline(vertices));
			pline.IsClosed = true;
			pline.ConstantWidth = 0.5;

			block.Entities.Add(pline);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.0, 0.0), new XYZ(-1.0, 0.0, 0.0))));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Dot Blank arrowhead.
	/// </summary>
	public static BlockRecord DotBlank
	{
		get
		{
			BlockRecord block = new BlockRecord(DotBlankName);

			block.Entities.Add(setDefault(() => new Circle(XYZ.Zero, 0.5)));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.0, 0.0), -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Dot Small arrowhead.
	/// </summary>
	public static BlockRecord DotSmall
	{
		get
		{
			BlockRecord block = new BlockRecord(DotSmallName);

			List<LwPolyline.Vertex> vertices = new();
			vertices.Add(new LwPolyline.Vertex(-0.0625, 0.0) { Bulge = 1.0 });
			vertices.Add(new LwPolyline.Vertex(0.0625, 0.0) { Bulge = 1.0 });

			LwPolyline pline = setDefault(() => new LwPolyline(vertices));
			pline.IsClosed = true;
			pline.ConstantWidth = 0.5;

			block.Entities.Add(pline);

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Dot Small Blank arrowhead.
	/// </summary>
	public static BlockRecord DotSmallBlank
	{
		get
		{
			BlockRecord block = new BlockRecord(DotSmallBlankName);
			block.Entities.Add(setDefault(() => new Circle(XYZ.Zero, 0.25)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Integral arrowhead.
	/// </summary>
	public static BlockRecord Integral
	{
		get
		{
			BlockRecord block = new BlockRecord(IntegralName);

			block.Entities.Add(setDefault(() => new Arc(
				new XYZ(0.44488802, -0.09133463, 0.0),
				0.4541666700000001, 1.78024, 2.93215)));

			block.Entities.Add(setDefault(() => new Arc(
				new XYZ(-0.44488802, 0.09133463, 0.0),
				0.4541666700000001, 4.9218284909877905164, 6.0737457969849337758)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the None (no arrowhead) type.
	/// </summary>
	public static BlockRecord None
	{
		get
		{
			return new BlockRecord(NoneName);
		}
	}

	/// <summary>
	/// Gets a block record for the Oblique arrowhead.
	/// </summary>
	public static BlockRecord Oblique
	{
		get
		{
			BlockRecord block = new BlockRecord(ObliqueName);
			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, -0.5, 0.0), new XYZ(0.5, 0.5, 0.0))));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Open arrowhead.
	/// </summary>
	public static BlockRecord Open
	{
		get
		{
			BlockRecord block = new BlockRecord(OpenName);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-1.0, 0.5 / 3.0d, 0.0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-1.0, -0.5 / 3.0d, 0.0))));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Open 30-degree arrowhead.
	/// </summary>
	public static BlockRecord Open30
	{
		get
		{
			BlockRecord block = new BlockRecord(Open30Name);
			block.Entities.Add(setDefault(() => new Line(new XYZ(-1, 2 - Math.Sqrt(3), 0.0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-1, -(2 - Math.Sqrt(3)), 0.0))));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Open 90-degree arrowhead.
	/// </summary>
	public static BlockRecord Open90
	{
		get
		{
			BlockRecord block = new BlockRecord(Open90Name);

			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.5, 0.0), XYZ.Zero)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, new XYZ(-0.5, -0.5, 0.0))));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Origin Indicator arrowhead.
	/// </summary>
	public static BlockRecord OriginIndicator
	{
		get
		{
			BlockRecord block = new BlockRecord(OriginIndicatorName);

			block.Entities.Add(setDefault(() => new Circle(XYZ.Zero, 0.5)));
			block.Entities.Add(setDefault(() => new Line(XYZ.Zero, -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>
	/// Gets a block record for the Origin Indicator 2 arrowhead (double circle).
	/// </summary>
	public static BlockRecord OriginIndicator2
	{
		get
		{
			BlockRecord block = new BlockRecord(OriginIndicator2Name);

			block.Entities.Add(setDefault(() => new Circle(XYZ.Zero, 0.5)));
			block.Entities.Add(setDefault(() => new Circle(XYZ.Zero, 0.25)));
			block.Entities.Add(setDefault(() => new Line(new XYZ(-0.5, 0.0, 0.0), -XYZ.AxisX)));

			return block;
		}
	}

	/// <summary>The block name for the Architectural Tick arrowhead.</summary>
	public const string ArchitecturalTickName = "_ArchTick";

	/// <summary>The block name for the Box Blank arrowhead.</summary>
	public const string BoxBlankName = "_BoxBlank";

	/// <summary>The block name for the Box Filled arrowhead.</summary>
	public const string BoxFilledName = "_BoxFilled";

	/// <summary>The block name for the Closed Blank arrowhead.</summary>
	public const string ClosedBlankName = "_ClosedBlank";

	/// <summary>The block name for the Closed arrowhead.</summary>
	public const string ClosedName = "_Closed";

	/// <summary>The block name for the Datum Blank arrowhead.</summary>
	public const string DatumBlankName = "_DatumBlank";

	/// <summary>The block name for the Datum Filled arrowhead.</summary>
	public const string DatumFilledName = "_DatumFilled";

	/// <summary>The block name for the Dot Blank arrowhead.</summary>
	public const string DotBlankName = "_DotBlank";

	/// <summary>The block name for the Dot arrowhead.</summary>
	public const string DotName = "_Dot";

	/// <summary>The block name for the Dot Small Blank arrowhead.</summary>
	public const string DotSmallBlankName = "_DotSmallBlank";

	/// <summary>The block name for the Dot Small arrowhead.</summary>
	public const string DotSmallName = "_DotSmall";

	/// <summary>The block name for the Integral arrowhead.</summary>
	public const string IntegralName = "_Integral";

	/// <summary>The block name for the None arrowhead.</summary>
	public const string NoneName = "_None";

	/// <summary>The block name for the Oblique arrowhead.</summary>
	public const string ObliqueName = "_Oblique";

	/// <summary>The block name for the Open 30-degree arrowhead.</summary>
	public const string Open30Name = "_Open30";

	/// <summary>The block name for the Open 90-degree arrowhead.</summary>
	public const string Open90Name = "_Open90";

	/// <summary>The block name for the Open arrowhead.</summary>
	public const string OpenName = "_Open";

	/// <summary>The block name for the Origin Indicator 2 arrowhead.</summary>
	public const string OriginIndicator2Name = "_Origin2";

	/// <summary>The block name for the Origin Indicator arrowhead.</summary>
	public const string OriginIndicatorName = "_Origin";

	private static T setDefault<T>(Func<T> factory)
		where T : Entity
	{
		T entity = factory();

		entity.Layer = Layer.Default;
		entity.LineType = LineType.ByBlock;
		entity.Color = Color.ByBlock;
		entity.LineWeight = LineWeightType.ByBlock;

		return entity;
	}
}