using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
using System;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO
{
	public class SvgWriterTests
	{
		public static readonly TheoryData<Type> EntityTypes = new TheoryData<Type>();

		static SvgWriterTests()
		{
			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				if (item == typeof(UnknownEntity))
				{
					continue;
				}

				EntityTypes.Add(item);
			}

			if (!Directory.Exists(TestVariables.OutputSvgFolder))
			{
				Directory.CreateDirectory(TestVariables.OutputSvgFolder);
			}
		}

		[Theory]
		[MemberData(nameof(EntityTypes))]
		public void WriteEntitiesNoDocument(Type t)
		{
			Entity e = (Entity)Factory.CreateObject(t);
			string filename = Path.Combine(TestVariables.OutputSvgFolder, $"{e.SubclassMarker}.svg");

			using (SvgWriter writer = new SvgWriter(filename))
			{
				writer.WriteEntity(e);
			}
		}

		[Fact]
		public void WriteLineNoDocument()
		{
			Entity e = new Line(new XYZ(0, 0, 0), new XYZ(10, 10, 0));

			string filename = Path.Combine(TestVariables.OutputSvgFolder, $"{e.ObjectType}.svg");

			using (SvgWriter writer = new SvgWriter(filename))
			{
				writer.WriteEntity(e);
			}
		}

		private void writeSvg(string filename, string svg)
		{
			using (StreamWriter sw = new StreamWriter(filename))
			{
				sw.Write(svg);
			}
		}
	}
}
