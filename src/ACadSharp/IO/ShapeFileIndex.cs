using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO
{
	/// <summary>
	/// Minimal reader for compiled AutoCAD shape files (<c>.shx</c>).
	/// </summary>
	/// <remarks>
	/// A <see cref="Entities.Shape"/> is stored in DXF by its shape name (code 2), but in
	/// DWG it is stored as a shape number (code 2 as well, but numeric) plus a hard
	/// pointer to the <c>SHAPEFILE</c> text style that owns the <c>.shx</c> where the
	/// shape is defined. When a drawing is read from DXF there is no such pointer, so the
	/// shape has to be resolved by looking up its name inside the shape files referenced
	/// by the document - exactly what AutoCAD does when it opens a DXF.
	/// <para>
	/// Layout of a compiled shape file that carries shape names:
	/// <list type="bullet">
	/// <item>header: <c>"AutoCAD-86 shapes 1.0\r\n\x1A"</c> (24 bytes)</item>
	/// <item><c>uint16</c> first shape number, <c>uint16</c> last shape number</item>
	/// <item>the shape names, NUL terminated, in ascending shape number order</item>
	/// </list>
	/// Shape files without names (for example <c>ltypeshp.shx</c>, where shapes are
	/// referenced by number only) simply yield no names and are skipped.
	/// </para>
	/// </remarks>
	internal static class ShapeFileIndex
	{
		private const int HeaderLength = 34;

		private static readonly Dictionary<string, Dictionary<string, ushort>> _cache
			= new Dictionary<string, Dictionary<string, ushort>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Reads the shape name to shape number map of a compiled shape file.
		/// </summary>
		/// <param name="path">Path of the .shx file.</param>
		/// <returns>Map of upper invariant shape names to shape numbers, empty when the file cannot be parsed.</returns>
		public static Dictionary<string, ushort> Read(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new Dictionary<string, ushort>();
			}

			lock (_cache)
			{
				if (_cache.TryGetValue(path, out Dictionary<string, ushort> cached))
				{
					return cached;
				}
			}

			Dictionary<string, ushort> map = read(path);

			lock (_cache)
			{
				_cache[path] = map;
			}

			return map;
		}

		/// <summary>
		/// Tries to resolve a shape name into the shape file style and shape number that
		/// reference it.
		/// </summary>
		/// <param name="shapeName">Shape name, as read from DXF code 2.</param>
		/// <param name="shapeStyles">Candidate text styles flagged as shape file.</param>
		/// <param name="directories">Directories where the shape files are searched.</param>
		/// <param name="style">Text style owning the shape file that contains the shape.</param>
		/// <param name="index">Shape number inside that shape file.</param>
		/// <returns><see langword="true"/> when the shape was found.</returns>
		public static bool TryResolve(
			string shapeName,
			IEnumerable<TextStyle> shapeStyles,
			IEnumerable<string> directories,
			out TextStyle style,
			out ushort index)
		{
			style = null;
			index = 0;

			if (string.IsNullOrEmpty(shapeName) || shapeStyles == null || directories == null)
			{
				return false;
			}

			string target = shapeName.ToUpperInvariant();

			foreach (TextStyle textStyle in shapeStyles)
			{
				if (textStyle == null || !textStyle.IsShapeFile || string.IsNullOrEmpty(textStyle.Filename))
				{
					continue;
				}

				foreach (string directory in directories)
				{
					if (string.IsNullOrEmpty(directory))
					{
						continue;
					}

					Dictionary<string, ushort> map = Read(Path.Combine(directory, textStyle.Filename));

					if (map.TryGetValue(target, out ushort number))
					{
						style = textStyle;
						index = number;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Default directories used to locate the shape files referenced by a document.
		/// </summary>
		public static IEnumerable<string> GetDefaultDirectories()
		{
			List<string> directories = new List<string>();

			try { directories.Add(Directory.GetCurrentDirectory()); } catch { }
			try { directories.Add(AppContext.BaseDirectory); } catch { }

			foreach (string drive in getFixedDrives())
			{
				foreach (string root in new[] { "Program Files", "Program Files (x86)" })
				{
					string autodesk = Path.Combine(drive, root, "Autodesk");
					if (!Directory.Exists(autodesk))
					{
						continue;
					}

					string[] candidates;
					try { candidates = Directory.GetDirectories(autodesk, "AutoCAD*"); }
					catch { continue; }

					foreach (string candidate in candidates)
					{
						string fonts = Path.Combine(candidate, "Fonts");
						if (Directory.Exists(fonts))
						{
							directories.Add(fonts);
						}
					}
				}
			}

			return directories;
		}

		private static IEnumerable<string> getFixedDrives()
		{
			List<string> drives = new List<string>();
			try
			{
				foreach (DriveInfo info in DriveInfo.GetDrives())
				{
					if (info.DriveType == DriveType.Fixed)
					{
						drives.Add(info.RootDirectory.FullName);
					}
				}
			}
			catch { }

			return drives;
		}

		private static Dictionary<string, ushort> read(string path)
		{
			Dictionary<string, ushort> map = new Dictionary<string, ushort>();

			try
			{
				if (!File.Exists(path))
				{
					return map;
				}

				byte[] bytes = File.ReadAllBytes(path);
				if (bytes.Length <= HeaderLength)
				{
					return map;
				}

				//Only compiled shape files are supported.
				if (System.Text.Encoding.ASCII.GetString(bytes, 0, 17) != "AutoCAD-86 shapes")
				{
					return map;
				}

				ushort first = BitConverter.ToUInt16(bytes, 24);
				ushort last = BitConverter.ToUInt16(bytes, 26);
				int count = last - first + 1;
				if (count <= 0 || count > short.MaxValue)
				{
					return map;
				}

				int i = HeaderLength;
				int start = i;
				int found = 0;

				while (i < bytes.Length && found < count)
				{
					byte b = bytes[i];

					if (b == 0)
					{
						string name = System.Text.Encoding.ASCII.GetString(bytes, start, i - start).ToUpperInvariant();
						if (name.Length > 0 && !map.ContainsKey(name))
						{
							map.Add(name, (ushort)(first + found));
						}

						found++;
						start = ++i;
						continue;
					}

					//Shape names are printable ASCII, the first non printable byte ends the name table.
					if (b < 32 || b >= 127)
					{
						break;
					}

					i++;
				}
			}
			catch
			{
				return new Dictionary<string, ushort>();
			}

			return map;
		}
	}
}
