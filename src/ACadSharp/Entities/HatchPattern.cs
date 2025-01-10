using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class HatchPattern
	{
		public static HatchPattern Solid { get { return new HatchPattern("SOLID"); } }

		[DxfCodeValue(2)]
		public string Name { get; set; }

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

			return patterns;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.Name}";
		}
	}
}
