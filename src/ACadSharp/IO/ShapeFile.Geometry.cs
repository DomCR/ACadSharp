using CSMath;
using System.Collections.Generic;

namespace ACadSharp.IO
{
	public partial class ShapeFile
	{
		public class Geometry
		{
			public byte[] Data { get; }

			public List<List<XY>> Lines { get; } = new();

			public string Name { get; }

			protected Geometry(string name, byte[] data)
			{
				this.Name = name;
				this.Data = data;
			}

			public static Geometry Create(string name, byte[] data)
			{
				Geometry geometry = new Geometry(name, data);

				int index = 0;

				ShapeBuilder rawShape = new ShapeBuilder();

				while (index < data.Length)
				{
					index += rawShape.ProcessValue(index, data);
				}

				return geometry;
			}
		}
	}
}