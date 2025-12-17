using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.Entities
{
	public partial class HatchPattern
	{
		public static HatchPattern Solid { get { return new HatchPattern("SOLID"); } }

		/// <summary>
		/// Description for this pattern.
		/// </summary>
		/// <remarks>
		/// The description is not saved in dxf or dwg files, its only used in the .pat files.
		/// </remarks>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the collection of <see cref="Line"/> objects.
		/// </summary>
		/// <remarks>This property allows adding, removing, or modifying the lines in the collection. Changes to this
		/// collection directly affect the associated context.</remarks>
		[DxfCodeValue(DxfReferenceType.Count, 79)]
		public List<Line> Lines { get; set; } = new List<Line>();

		/// <summary>
		/// Name for this pattern.
		/// </summary>
		[DxfCodeValue(2)]
		public string Name { get; set; }

		/// <summary>
		/// Default constructor of a hatch pattern.
		/// </summary>
		/// <param name="name"></param>
		public HatchPattern(string name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Load a collection of patterns from a .pat file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
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
					int index = line.IndexOf(',');
					string noPrefix = line.Remove(0, 1);
					current = new HatchPattern(noPrefix.Substring(0, index - 1));
					current.Description = new string(noPrefix.Skip(index).ToArray()).Trim();

					patterns.Add(current);
				}
				else
				{
					//;angle, x, y, shift, offset, [dash, space, ...]
					string[] data = line.Split(',');
					Line l = new Line();
					//angle
					l.Angle = MathHelper.DegToRad(double.Parse(data[0], CultureInfo.InvariantCulture));
					//x, y
					l.BasePoint = new XY(double.Parse(data[1], CultureInfo.InvariantCulture), double.Parse(data[2], CultureInfo.InvariantCulture));

					double shift = double.Parse(data[3], CultureInfo.InvariantCulture);
					double offset = double.Parse(data[4], CultureInfo.InvariantCulture);
					XY dir = new XY(shift, offset);
					double cos = Math.Cos(l.Angle);
					double sin = Math.Sin(l.Angle);
					l.Offset = new XY(dir.X * cos - dir.Y * sin, dir.X * sin + dir.Y * cos);

					IEnumerable<string> dashes = data.Skip(5);
					if (dashes.Any())
					{
						l.DashLengths.AddRange(dashes.Select(d => double.Parse(d, CultureInfo.InvariantCulture)));
					}

					current.Lines.Add(l);
				}
			}

			return patterns;
		}

		/// <summary>
		/// Write a pattern or a collection of patterns into a .pat file.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="patterns"></param>
		public static void SavePatterns(string filename, params HatchPattern[] patterns)
		{
			using StreamWriter writer = File.CreateText(filename);

			foreach (HatchPattern p in patterns)
			{
				writer.Write($"*{p.Name}");

				if (!p.Description.IsNullOrEmpty())
				{
					writer.Write($",{p.Description}");
				}

				writer.WriteLine();

				foreach (Line line in p.Lines)
				{
					StringBuilder sb = new StringBuilder();

					double angle = MathHelper.DegToRad(line.Angle);
					double cos = Math.Cos(0.0 - line.Angle);
					double sin = Math.Sin(0.0 - line.Angle);

					var v = new XY(line.Offset.X * cos - line.Offset.Y * sin, line.Offset.X * sin + line.Offset.Y * cos);

					sb.Append(angle.ToString(CultureInfo.InvariantCulture));
					sb.Append(",");
					sb.Append(line.BasePoint.ToString(CultureInfo.InvariantCulture));
					sb.Append(",");
					sb.Append(v.ToString(CultureInfo.InvariantCulture));

					if (line.DashLengths.Count > 0)
					{
						sb.Append(",");
						sb.Append(line.DashLengths[0].ToString(CultureInfo.InvariantCulture));
						for (int i = 1; i < line.DashLengths.Count; i++)
						{
							sb.Append(",");
							sb.Append(line.DashLengths[i].ToString(CultureInfo.InvariantCulture));
						}
					}

					writer.WriteLine(sb.ToString());
				}
			}
		}

		/// <summary>
		/// Clones the current pattern.
		/// </summary>
		/// <returns></returns>
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

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.Name}";
		}

		/// <summary>
		/// Update the pattern geometry with a translation, rotation and scale.
		/// </summary>
		/// <param name="translation"></param>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		public void Update(XY translation, double rotation, double scale)
		{
			var tr = Transform.CreateTranslation(translation.Convert<XYZ>());
			var sc = Transform.CreateScaling(new XYZ(scale));
			var rot = Transform.CreateRotation(XYZ.AxisZ, rotation);

			var transform = new Transform(tr.Matrix * sc.Matrix * rot.Matrix);

			foreach (var line in Lines)
			{
				line.Angle += rotation;
				line.BasePoint = transform.ApplyTransform(line.BasePoint.Convert<XYZ>()).Convert<XY>();
				line.Offset = transform.ApplyTransform(line.Offset.Convert<XYZ>()).Convert<XY>();
				line.DashLengths = line.DashLengths.Select(d => d * scale).ToList();
			}
		}
	}
}