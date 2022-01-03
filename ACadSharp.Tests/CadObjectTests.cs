using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> Types;

		static CadObjectTests()
		{
			Types = new TheoryData<Type>();

			var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");

			foreach (var item in d.GetTypes())
			{
				if (item.IsSubclassOf(typeof(Entity)) || item.IsSubclassOf(typeof(TableEntry)))
				{
					Types.Add(item);
				}
			}
		}

		[Theory]
		[MemberData(nameof(Types))]
		public void GetCadObjectMapTest(Type t)
		{
			DxfNameAttribute att = t.GetCustomAttribute<DxfNameAttribute>();

			Assert.NotNull(att);

			switch (att.Name)
			{
				case DxfFileToken.EntityAttribute:
				case DxfFileToken.EntityAttributeDefinition:
					return;
				case DxfFileToken.EntityArc:
					DxfMap.Create<Arc>();
					break;
				case DxfFileToken.EntityCircle:
					DxfMap.Create<Circle>();
					break;
				case DxfFileToken.EntityEllipse:
					DxfMap.Create<Ellipse>();
					break;
				case DxfFileToken.Entity3DFace:
					DxfMap.Create<Face3D>();
					break;
				case DxfFileToken.EntityHatch:
					DxfMap.Create<Hatch>();
					break;
				case DxfFileToken.EntityInsert:
					DxfMap.Create<Insert>();
					break;
				case DxfFileToken.EntityLine:
					DxfMap.Create<Line>();
					break;
				case DxfFileToken.EntityMLine:
					DxfMap.Create<MLine>();
					break;
				case DxfFileToken.EntityMText:
					DxfMap.Create<MText>();
					break;
				case DxfFileToken.EntityPoint:
					DxfMap.Create<Point>();
					break;
				case DxfFileToken.EntityPolyline:
					DxfMap.Create<PolyLine>();
					break;
				case DxfFileToken.EntityRay:
					DxfMap.Create<Ray>();
					break;
				case DxfFileToken.Entity3DSolid:
					DxfMap.Create<Solid3D>();
					break;
				case DxfFileToken.EntityText:
					DxfMap.Create<TextEntity>();
					break;
				case DxfFileToken.EntityVertex:
					DxfMap.Create<Vertex>();
					break;
				case DxfFileToken.EntityViewport:
					DxfMap.Create<Viewport>();
					break;
				default:
					throw new NotImplementedException($"Test not implemented for type {t.Name}");
			}
		}
	}
}
