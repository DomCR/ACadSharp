using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Decoded Tianzheng (TArch / Tangent) property map stored inside a custom object payload.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Tianzheng custom objects (<c>TCH_*</c> / <c>TDb*</c>) carry a self-describing property block.
	/// It is written through <c>AcDbDwgFiler</c>, so it uses the ordinary DWG string encoding:
	/// a <c>BitShort</c> character count followed by UTF-16LE characters.
	/// </para>
	/// <para>
	/// The block is <b>sparse</b>: only properties that differ from their default are present, and
	/// different classes carry different properties. Property names are upper snake case ASCII
	/// (<c>PIPE_SYSTEM_NAME</c>, <c>PIPE_THICK</c>, ...); values may be numeric text or localized text.
	/// </para>
	/// <para>
	/// The sequence ends with an (usually empty) <c>TDBXDATAMAP_BLOCK_BEGIN_</c> /
	/// <c>TDBXDATAMAP_BLOCK_END_</c> marker pair. These markers terminate the map; they do not wrap it.
	/// </para>
	/// </remarks>
	public sealed class TchDataMap
	{
		/// <summary>Marker written after the property sequence.</summary>
		public const string BlockBeginMarker = "TDBXDATAMAP_BLOCK_BEGIN_";

		/// <summary>Closing marker of the terminating marker pair.</summary>
		public const string BlockEndMarker = "TDBXDATAMAP_BLOCK_END_";

		private const int DefaultMaxEntries = 4096;

		private const int MaxStringLength = 200;

		private readonly List<string> _entries = new List<string>();

		private readonly Dictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.Ordinal);

		private TchDataMap() { }

		/// <summary>Gets the decoded strings in stream order.</summary>
		public IReadOnlyList<string> Entries => _entries;

		/// <summary>Gets the property name to value map built from <see cref="Entries"/>.</summary>
		/// <remarks>
		/// Tianzheng writes a property name immediately before its value. Names without a following
		/// value (for example when a flag is set) are still listed in <see cref="Entries"/> but are
		/// not present here.
		/// </remarks>
		public IReadOnlyDictionary<string, string> Properties => _properties;

		/// <summary>Gets a value indicating whether the closing marker was reached.</summary>
		public bool IsComplete { get; private set; }

		/// <summary>Gets a value indicating whether the object carried no properties.</summary>
		/// <remarks>
		/// The terminating marker pair is not considered a property, so an object whose payload only
		/// contains the markers is reported as empty.
		/// </remarks>
		public bool IsEmpty => _properties.Count == 0;

		/// <summary>
		/// Reads a Tianzheng property map from the current position of <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">Reader positioned at the start of the map.</param>
		/// <param name="maxEntries">Safety limit for the number of decoded strings.</param>
		/// <returns>The decoded map; never null.</returns>
		/// <remarks>
		/// The reader position is restored if a string cannot be decoded, so the caller can continue
		/// reading the enclosing object.
		/// </remarks>
		internal static TchDataMap Read(IDwgStreamReader reader, int maxEntries = DefaultMaxEntries)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			TchDataMap map = new TchDataMap();

			for (int i = 0; i < maxEntries; i++)
			{
				if (reader.IsEmpty)
				{
					break;
				}

				long start = reader.PositionInBits();
				string value;
				try
				{
					value = reader.ReadVariableText();
				}
				catch (Exception)
				{
					reader.SetPositionInBits(start);
					break;
				}

				if (string.IsNullOrEmpty(value) || value.Length > MaxStringLength || !isPrintable(value))
				{
					reader.SetPositionInBits(start);
					break;
				}

				map._entries.Add(value);

				if (value == BlockEndMarker)
				{
					map.IsComplete = true;
					break;
				}
			}

			map.buildProperties();
			return map;
		}

		/// <summary>
		/// Reads a Tianzheng property map from a detached payload, such as the retained data of a
		/// proxy entity.
		/// </summary>
		/// <param name="payload">Raw payload bytes.</param>
		/// <param name="version">Drawing version used for the string encoding.</param>
		public static TchDataMap Read(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				return Read(reader);
			}
		}

		private static bool isPrintable(string value)
		{
			foreach (char c in value)
			{
				if (char.IsControl(c))
				{
					return false;
				}
			}

			return true;
		}

		private void buildProperties()
		{
			string pending = null;
			foreach (string entry in _entries)
			{
				if (entry == BlockBeginMarker || entry == BlockEndMarker)
				{
					continue;
				}

				if (isPropertyName(entry))
				{
					// A name with no value of its own: it is either a flag or its value is written
					// through the native (non string) path.
					if (pending != null)
					{
						_properties[pending] = null;
					}

					pending = entry;
					if (!_properties.ContainsKey(entry))
					{
						_properties[entry] = null;
					}
				}
				else if (pending != null)
				{
					_properties[pending] = entry;
					pending = null;
				}
			}
		}

		/// <summary>
		/// Decides whether an entry looks like a Tianzheng property name.
		/// </summary>
		/// <remarks>
		/// Names are upper case ASCII. Numeric looking entries such as <c>DN25</c> are values, not
		/// names, so an entry containing digits is only accepted as a name when it also contains an
		/// underscore.
		/// </remarks>
		private static bool isPropertyName(string value)
		{
			if (value.Length < 3)
			{
				return false;
			}

			bool hasLetter = false;
			bool hasDigit = false;
			bool hasUnderscore = false;

			foreach (char c in value)
			{
				if (c >= 'a' && c <= 'z')
				{
					return false;
				}
				else if (c >= 'A' && c <= 'Z')
				{
					hasLetter = true;
				}
				else if (c >= '0' && c <= '9')
				{
					hasDigit = true;
				}
				else if (c == '_')
				{
					hasUnderscore = true;
				}
				else
				{
					return false;
				}
			}

			return hasLetter && (!hasDigit || hasUnderscore);
		}
	}
}
