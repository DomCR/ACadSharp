using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ACadSharp.Entities
{
	public partial class HatchPattern
	{
		public static HatchPattern Solid { get { return new HatchPattern("SOLID"); } }

		[DxfCodeValue(2)]
		public string Name { get; set; }

		/// <summary>
		/// Additional names for this pattern.
		/// </summary>
		/// <remarks>
		/// The Additional names are not saved in dxf or dwg files, they are only used in the .pat files.
		/// </remarks>
		public string[] AdditionalNames { get; set; }

		[DxfCodeValue(DxfReferenceType.Count, 79)]
		public List<Line> Lines { get; set; } = new List<Line>();

		public HatchPattern(string name)
		{
			this.Name = name;
		}

		public HatchPattern Clone()
		{
			HatchPattern clone = (HatchPattern)this.MemberwiseClone();

			clone.Lines = new List<Line>();
			foreach (var item in this.Lines)
			{
				clone.Lines.Add(item.Clone());
			}

			return clone;
		}

		public static IEnumerable<HatchPattern> LoadFrom(string path)
		{
			List<HatchPattern> patterns = new List<HatchPattern>();
			HatchPattern current = null;

			var lines = File.ReadLines(path)
				.Select(line => line.Trim())
				.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith(";"))
				.ToQueue();

			List<int> leadingIndices = lines.Select((line, i) => (line, i))
				.Where(t => t.line.StartsWith("*"))
				.Select(t => t.i)
				.OrderBy(i => i).ToList();

			leadingIndices.Add(lines.Count);

			while (lines.TryDequeue(out string line))
			{
				if (line.StartsWith("*"))
				{
					string[] names = line.Remove(0, 1).Split(',');
					current = new HatchPattern(names.FirstOrDefault());
					patterns.Add(current);
				}
				else
				{
					string[] data = line.Split(',');
					Line l = new Line();
					l.Angle = MathHelper.DegToRad(double.Parse(data[0], CultureInfo.InvariantCulture));
					l.BasePoint = new XY(double.Parse(data[1], CultureInfo.InvariantCulture), double.Parse(data[2], CultureInfo.InvariantCulture));

					XY offset = new XY(double.Parse(data[3], CultureInfo.InvariantCulture), double.Parse(data[4], CultureInfo.InvariantCulture));
					double cos = Math.Cos(l.Angle);
					double sin = Math.Sin(l.Angle);
					l.Offset = new XY(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos);

					IEnumerable<string> dashes = data.Skip(5);
					if (dashes.Any())
					{
						l.DashLengths.AddRange(dashes.Select(d => double.Parse(d, CultureInfo.InvariantCulture)));
					}
				}
			}

			return patterns;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.Name}";
		}
	}
}
