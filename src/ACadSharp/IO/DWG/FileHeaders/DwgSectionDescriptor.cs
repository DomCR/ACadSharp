using System;
using System.Collections.Generic;

namespace ACadSharp.IO.DWG
{
	internal class DwgSectionDescriptor
	{
		public long PageType { get; } = 0x4163043B;

		public string Name { get; set; }

		public ulong CompressedSize { get; set; }

		public int PageCount { get; set; }

		public ulong DecompressedSize { get; set; } = 0x7400;

		/// <remarks>
		/// Is only used for the version <see cref="ACadVersion.AC1018"/> and <see cref="ACadVersion.AC1024"/> or above.
		/// </remarks>
		public int CompressedCode
		{
			get => this._compressed;
			set => this._compressed = value == 1 || value == 2 ? value :
					throw new Exception();
		}

		private int _compressed = 2;

		/// <remarks>
		/// Is only used for the version <see cref="ACadVersion.AC1018"/> and <see cref="ACadVersion.AC1024"/> or above.
		/// </remarks>
		public bool IsCompressed { get { return this._compressed == 2; } }

		public int SectionId { get; set; }

		public int Encrypted { get; set; }

		public ulong? HashCode { get; internal set; }

		public ulong? Encoding { get; internal set; }

		public List<DwgLocalSectionMap> LocalSections { get; set; } = new List<DwgLocalSectionMap>();

		public DwgSectionDescriptor() { }

		public DwgSectionDescriptor(string name)
		{
			this.Name = name;
		}
	}
}