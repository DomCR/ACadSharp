using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.IO
{
	public class ShapeFile
	{
		public const string DefaultShapeFile = "ltypeshp.shx";

		private const string _sentinelV1 = "AutoCAD-86 shapes 1.0";

		public static void Open(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			File.OpenRead(file);

			using (BinaryReader reader = new BinaryReader(File.OpenRead(file)))
			{
				byte[] sentinel = reader.ReadBytes(21);
				StringBuilder sb = new StringBuilder(21);
				sb.Append(sentinel.Select(b => (char)b).ToArray());

				if (!sb.ToString().Equals(_sentinelV1, StringComparison.InvariantCulture))
				{
					throw new ArgumentException("Not a valid Shape binary file .SHX.", nameof(file));
				}

				reader.ReadBytes(3);

				ushort firstShape = reader.ReadUInt16();
				ushort lastShape = reader.ReadUInt16();
				ushort nEntries = reader.ReadUInt16();

				var shapes = new List<(ushort, ushort)>(nEntries);
				for (int i = 0; i < nEntries; i++)
				{
					var index = reader.ReadUInt16();
					var size = reader.ReadUInt16();
					shapes.Add((index, size));
				}

				for (int i = 0; i < nEntries; i++)
				{
					string name = nullTerminatedString(reader, Encoding.ASCII);
					byte[] shape = reader.ReadBytes(shapes[i].Item2 - (name.Length + 1));
				}
			}
		}

		private static string nullTerminatedString(BinaryReader reader, Encoding encoding)
		{
			byte c = reader.ReadByte();
			List<byte> bytes = new List<byte>();
			while (c != 0) // strings always end with a 0 byte (char NULL)
			{
				bytes.Add(c);
				c = reader.ReadByte();
			}
			return encoding.GetString(bytes.ToArray(), 0, bytes.Count);
		}
	}
}
