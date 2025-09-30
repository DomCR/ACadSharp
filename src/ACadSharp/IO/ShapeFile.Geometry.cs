using CSMath;
using System.Collections.Generic;

namespace ACadSharp.IO
{
	public partial class ShapeFile
	{
		public class Geometry
		{
			public List<List<XY>> Lines { get; } = new();

			public string Name { get; }

			public Geometry(string name)
			{
				this.Name = name;
			}

			public static Geometry Create(string name, byte[] data)
			{
				int index = 0;

				ShapeBuilder builder = new ShapeBuilder();

				while (index < data.Length)
				{
					index += builder.ProcessValue(index, data);
				}

				return builder.Build(name);
			}
		}
	}
}