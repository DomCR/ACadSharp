using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Linq;

namespace ACadSharp.Tests.Common
{
	public class EntityFactory : Factory
	{
		public static T CreateDefault<T>()
			where T : Entity, new()
		{
			return new T();
		}

		/// <summary>
		/// Create a default entity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="randomize">Populate the entity with random values and fields</param>
		/// <returns></returns>
		public static T Create<T>(bool randomize = true)
			where T : Entity, new()
		{
			return (T)Create(typeof(T), randomize);
		}

		public static Entity Create(Type type, bool randomize = true)
		{
			object e = null;

			if (type == typeof(Insert))
			{
				e = new Insert(TableEntryFactory.Create<BlockRecord>());
			}
			else if (type == typeof(Block))
			{
				e = new Block(TableEntryFactory.Create<BlockRecord>());
			}
			else if (type == typeof(BlockEnd))
			{
				e = new BlockEnd(TableEntryFactory.Create<BlockRecord>());
			}
			else
			{
				e = Activator.CreateInstance(type);
			}

			if (!randomize)
				return (Entity)e;

			return (Entity)map(e);
		}

		private static T map<T>(T e)
		{
			foreach (var p in e.GetType()
				.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
				.Where(o => o.CanWrite && !o.PropertyType.IsClass && !o.PropertyType.IsEnum && !o.PropertyType.IsInterface))
			{
				p.SetValue(e, _random.Next(p.PropertyType));
			}

			return e;

			switch (e)
			{
				case Arc arc:
					RandomizeArc(arc);
					break;
				case Block block:
					RandomizeBlock(block);
					break;
				case Circle circle:
					RandomizeCircle(circle);
					break;
				case Dimension dimension:
					RandomizeDimension(dimension);
					switch (dimension)
					{
						case DimensionLinear linear:
							RandomizeDimensionAligned(linear);
							RandomizeDimensionLinear(linear);
							break;
						case DimensionAligned aligned:
							RandomizeDimensionAligned(aligned);
							break;
						case DimensionRadius radius:
							RandomizeDimensionRadius(radius);
							break;
						case DimensionAngular2Line angular2Line:
							RandomizeDimensionAngular2Line(angular2Line);
							break;
						case DimensionAngular3Pt angular3pt:
							RandomizeDimension3Pt(angular3pt);
							break;
						case DimensionDiameter diamenter:
							RandomizeDimensionDiameter(diamenter);
							break;
						case DimensionOrdinate ordinate:
							RandomizeDimensionOrdinate(ordinate);
							break;
						default:
							throw new NotImplementedException();
					}
					break;
				case Ellipse ellipse:
					RandomizeEllipse(ellipse);
					break;
				case Line line:
					RandomizeLine(line);
					break;
				case LwPolyline lwPolyline:
					RandomizeLwPolyline(lwPolyline);
					break;
				case Point point:
					RandomizePoint(point);
					break;
				case Polyline2D pl2d:
					RandomizePolyline(pl2d);
					break;
				case Polyline3D pl3d:
					RandomizePolyline(pl3d);
					break;
				case TextEntity text:
					RandomizeText(text);
					break;
				default:
					throw new NotImplementedException();
			}

			return e;
		}

		public static void RandomizeEntity(Entity entity)
		{
			entity.Color = _random.NextColor();
		}

		public static void RandomizeArc(Arc arc)
		{
			RandomizeCircle(arc);

			arc.StartAngle = _random.NextDouble();
			arc.EndAngle = _random.NextDouble();
		}

		public static void RandomizeBlock(Block block)
		{
			RandomizeEntity(block);

			block.BasePoint = _random.NextXYZ();
			//block.XrefPath = _random.RandomString(10);
			block.Comments = _random.RandomString(10);
		}

		public static void RandomizeCircle(Circle circle)
		{
			RandomizeEntity(circle);

			// circle.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			circle.Center = _random.NextXYZ();
			circle.Thickness = _random.NextDouble();
			circle.Radius = _random.NextDouble();
		}

		public static void RandomizeDimension(Dimension dimension)
		{
			dimension.Block = null;
			dimension.DefinitionPoint = _random.NextXYZ();
			dimension.TextMiddlePoint = _random.NextXYZ();
			dimension.InsertionPoint = _random.NextXYZ();
			dimension.LineSpacingFactor = _random.NextDouble();
			//dimension.Measurement = _random.NextDouble();
			dimension.Text = _random.RandomString(10);
			dimension.TextRotation = _random.NextDouble();
			dimension.HorizontalDirection = _random.NextDouble();
		}

		public static void RandomizeDimensionAligned(DimensionAligned dimension)
		{
			dimension.FirstPoint = _random.NextXYZ();
			dimension.SecondPoint = _random.NextXYZ();
			dimension.ExtLineRotation = _random.NextDouble();
		}

		public static void RandomizeDimensionLinear(DimensionLinear dimension)
		{
			dimension.Rotation = _random.NextDouble();
		}

		public static void RandomizeDimensionRadius(DimensionRadius dimension)
		{
			dimension.AngleVertex = _random.NextXYZ();
			dimension.LeaderLength = _random.NextDouble();
		}

		public static void RandomizeDimensionAngular2Line(DimensionAngular2Line dimension)
		{
			dimension.FirstPoint = _random.NextXYZ();
			dimension.SecondPoint = _random.NextXYZ();
			dimension.AngleVertex = _random.NextXYZ();
			dimension.DimensionArc = _random.NextXYZ();
		}

		public static void RandomizeDimension3Pt(DimensionAngular3Pt dimension)
		{
			dimension.FirstPoint = _random.NextXYZ();
			dimension.SecondPoint = _random.NextXYZ();
			dimension.AngleVertex = _random.NextXYZ();
		}

		public static void RandomizeDimensionDiameter(DimensionDiameter dimension)
		{
			dimension.AngleVertex = _random.NextXYZ();
			dimension.LeaderLength = _random.NextDouble();
		}

		public static void RandomizeDimensionOrdinate(DimensionOrdinate dimension)
		{
			dimension.FeatureLocation = _random.NextXYZ();
			dimension.LeaderEndpoint = _random.NextXYZ();
		}

		public static void RandomizeEllipse(Ellipse ellipse)
		{
			RandomizeEntity(ellipse);

			// circle.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			ellipse.Center = _random.NextXYZ();
			ellipse.Thickness = _random.NextDouble();
			ellipse.EndPoint = _random.NextXYZ();
			ellipse.RadiusRatio = _random.NextDouble();
			ellipse.StartParameter = _random.NextDouble();
			ellipse.EndParameter = _random.NextDouble();
		}

		public static void RandomizeLine(Line line)
		{
			RandomizeEntity(line);

			line.Thickness = _random.NextDouble();
			// line.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			line.StartPoint = _random.NextXYZ();
			line.EndPoint = _random.NextXYZ();
		}

		public static void RandomizeLwPolyline(LwPolyline lwPolyline)
		{
			RandomizeEntity(lwPolyline);

			lwPolyline.Thickness = _random.NextDouble();
			// line.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			lwPolyline.ConstantWidth = _random.NextDouble();
			lwPolyline.Elevation = _random.NextDouble();

			for (int i = 0; i < _random.NextShort(); i++)
			{
				lwPolyline.Vertices.Add(new LwPolyline.Vertex
				{
					Location = _random.NextXY(),
					StartWidth = _random.NextDouble(),
					EndWidth = _random.NextDouble(),
					Bulge = _random.NextDouble(),
					CurveTangent = _random.NextDouble(),
					Id = i
				});
			}
		}

		public static void RandomizePoint(Point point)
		{
			RandomizeEntity(point);

			point.Thickness = _random.NextDouble();
			// line.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			point.Location = _random.NextXYZ();
		}

		public static void RandomizePolyline(Polyline pline)
		{
			RandomizeEntity(pline);

			int nv = _random.Next(2, 100);
			for (int i = 0; i < nv; i++)
			{
				Vertex v = null;

				switch (pline)
				{
					case Polyline2D:
						v = new Vertex2D();
						break;
					case Polyline3D:
						v = new Vertex3D();
						break;
				}

				v.Id = i;
				v.Location = _random.NextXYZ();

				pline.Vertices.Add(v);
			}
		}

		public static void RandomizeText(TextEntity text)
		{
			RandomizeEntity(text);

			text.Thickness = _random.NextDouble();
			text.InsertPoint = _random.NextXYZ();
			text.AlignmentPoint = _random.NextXYZ();
			text.Height = _random.NextDouble();
			text.Value = _random.RandomString(10);
			text.Rotation = _random.NextDouble();
			text.WidthFactor = _random.NextDouble();
			text.ObliqueAngle = _random.NextDouble();

			text.Mirror = (TextMirrorFlag)_random.Next(0, 4);
			text.HorizontalAlignment = (TextHorizontalAlignment)_random.Next(0, 5);
			text.VerticalAlignment = (TextVerticalAlignmentType)_random.Next(0, 3);
		}
	}
}
