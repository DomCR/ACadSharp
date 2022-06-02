using ACadSharp.Attributes;
using ACadSharp.Blocks;
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
	public class DxfMapTests
	{
		public static readonly TheoryData<Type> Types;

		static DxfMapTests()
		{
			Types = new TheoryData<Type>();

			var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");

			foreach (var item in d.GetTypes().Where(i => !i.IsAbstract && i.IsPublic))
			{
				if (item.IsSubclassOf(typeof(Entity)) || item.IsSubclassOf(typeof(TableEntry)))
				{
					Types.Add(item);
				}
			}
		}

		[Theory]
		[MemberData(nameof(Types))]
		public void CreateMapTest(Type t)
		{
			DxfNameAttribute att = t.GetCustomAttribute<DxfNameAttribute>();
			DxfSubClassAttribute subclass = t.GetCustomAttribute<DxfSubClassAttribute>();

			Assert.NotNull(att);

			switch (att.Name)
			{
				case DxfFileToken.TableAppId:
					DxfMap.Create<AppId>();
					break;
				case DxfFileToken.EntityAttribute:
					DxfMap.Create<AttributeEntity>();
					break;
				case DxfFileToken.EntityAttributeDefinition:
					DxfMap.Create<AttributeDefinition>();
					return;
				case DxfFileToken.EntityArc:
					DxfMap.Create<Arc>();
					break;
				case DxfFileToken.Block:
					DxfMap.Create<Block>();
					break;
				case DxfFileToken.EndBlock:
					DxfMap.Create<BlockEnd>();
					break;
				case DxfFileToken.TableBlockRecord:
					DxfMap.Create<BlockRecord>();
					break;
				case DxfFileToken.EntityCircle:
					DxfMap.Create<Circle>();
					break;
				case DxfFileToken.TableDimstyle:
					DxfMap.Create<DimensionStyle>();
					break;
				case DxfFileToken.EntityDimension:
					Assert.NotNull(subclass);
					switch (subclass.ClassName)
					{
						case DxfSubclassMarker.AlignedDimension:
							DxfMap.Create<DimensionAligned>();
							break;
						case DxfSubclassMarker.Angular2LineDimension:
							DxfMap.Create<DimensionAngular2Line>();
							break;
						case DxfSubclassMarker.Angular3PointDimension:
							DxfMap.Create<DimensionAngular3Pt>();
							break;
						case DxfSubclassMarker.DiametricDimension:
							DxfMap.Create<DimensionDiameter>();
							break;
						case DxfSubclassMarker.LinearDimension:
							DxfMap.Create<DimensionLinear>();
							break;
						case DxfSubclassMarker.OrdinateDimension:
							DxfMap.Create<DimensionOrdinate>();
							break;
						case DxfSubclassMarker.RadialDimension:
							DxfMap.Create<DimensionRadius>();
							break;
						default:
							throw new NotImplementedException($"Test not implemented for type {t.Name}");
					}
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
				case DxfFileToken.TableLayer:
					DxfMap.Create<Layer>();
					break;
				case DxfFileToken.EntityLeader:
					DxfMap.Create<Leader>();
					break;
				case DxfFileToken.TableLinetype:
					DxfMap.Create<LineType>();
					break;
				case DxfFileToken.EntityLine:
					DxfMap.Create<Line>();
					break;
				case DxfFileToken.EntityLwPolyline:
					DxfMap.Create<LwPolyline>();
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
					DxfMap.Create<Polyline>();
					break;
				case DxfFileToken.EntityRay:
					DxfMap.Create<Ray>();
					break;
				case DxfFileToken.EntityShape:
					DxfMap.Create<Shape>();
					break;
				case DxfFileToken.EntitySeqend:
					DxfMap.Create<Seqend>();
					break;
				case DxfFileToken.EntitySolid:
					DxfMap.Create<Solid>();
					break;
				case DxfFileToken.Entity3DSolid:
					DxfMap.Create<Solid3D>();
					break;
				case DxfFileToken.EntitySpline:
					DxfMap.Create<Spline>();
					break;
				case DxfFileToken.EntityText:
					DxfMap.Create<TextEntity>();
					break;
				case DxfFileToken.TableStyle:
					DxfMap.Create<TextStyle>();
					break;
				case DxfFileToken.TableUcs:
					DxfMap.Create<UCS>();
					break;
				case DxfFileToken.EntityVertex:
					Assert.NotNull(subclass);
					switch (subclass.ClassName)
					{
						case DxfSubclassMarker.PolylineVertex:
							DxfMap.Create<Vertex2D>();
							break;
						case DxfSubclassMarker.Polyline3dVertex:
							DxfMap.Create<Vertex3D>();
							break;
						default:
							throw new NotImplementedException($"Test not implemented for type {t.Name}");
					}
					break;
				case DxfFileToken.TableView:
					DxfMap.Create<View>();
					break;
				case DxfFileToken.EntityViewport:
					DxfMap.Create<Viewport>();
					break;
				case DxfFileToken.TableVport:
					DxfMap.Create<VPort>();
					break;
				case DxfFileToken.EntityXline:
					DxfMap.Create<XLine>();
					break;
				default:
					throw new NotImplementedException($"Test not implemented for type {t.Name}");
			}
		}
	}
}
