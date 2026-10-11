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

		/// <summary>
		/// Reads a Tianzheng property map from the whole class specific payload of an object, such
		/// as <see cref="Entities.UnknownEntity.RawData"/>.
		/// </summary>
		/// <remarks>
		/// Unlike <see cref="Read(byte[], ACadVersion)"/> the map is not at the start of the payload
		/// here: it follows the class's own fields and is bit aligned to whatever they consumed. The
		/// block is located by trying every bit offset and keeping the longest readable string
		/// sequence that contains the closing marker.
		/// </remarks>
		/// <param name="payload">Raw payload bytes.</param>
		/// <param name="version">Drawing version used for the string encoding.</param>
		/// <returns>The decoded map; never null. An empty map when no block is found.</returns>
		public static TchDataMap ReadFromPayload(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			if (payload.Length == 0)
			{
				return new TchDataMap();
			}

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);

				return ReadFromPayload(reader, (long)payload.Length * 8L);
			}
		}

		private static TchDataMap ReadFromPayload(IDwgStreamReader reader, long limit)
		{
			TchDataMap best = null;

			for (long offset = 0; offset < limit; offset++)
			{
				TchDataMap candidate = tryReadSequence(reader, offset);
				if (candidate == null)
				{
					continue;
				}

				//The longest run wins: the properties precede the markers, so the sequence that
				//starts earliest and still reaches the closing marker is the whole block.
				if (best == null || candidate._entries.Count > best._entries.Count)
				{
					best = candidate;
				}
			}

			return best ?? new TchDataMap();
		}

		private static TchDataMap tryReadSequence(IDwgStreamReader reader, long offset)
		{
			TchDataMap map = new TchDataMap();
			reader.SetPositionInBits(offset);

			for (int i = 0; i < DefaultMaxEntries; i++)
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
					break;
				}

				map._entries.Add(value);

				if (value == BlockEndMarker)
				{
					map.IsComplete = true;
					break;
				}
			}

			//Only a sequence that reached the closing marker describes a real block
			if (!map.IsComplete || map._entries.Count < 2)
			{
				return null;
			}

			map.buildProperties();
			return map;
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
					//A descriptor followed by a plain name looking entry is a pair: the second one is
					//a value that happens to look like a name, such as the hatch pattern PUB_HATCH.
					if (pending != null && isTypedFieldDescriptor(pending) && !isTypedFieldDescriptor(entry))
					{
						_properties[pending] = entry;
						pending = null;
						continue;
					}

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
		/// <summary>
		/// Decides whether an entry is a typed field descriptor rather than a value.
		/// </summary>
		/// <remarks>
		/// Native objects describe their fields as <c>T20</c> followed by an optional version prefix
		/// <c>V&lt;n&gt;_</c>, a type letter (<c>b</c> boolean, <c>d</c> double, <c>s</c> string,
		/// <c>u</c> unsigned) and either an index or a readable name:
		/// <c>T20s1</c>, <c>T20b2</c>, <c>T20d12</c>, <c>T20V9_dRealDist2DimLine</c>.
		/// </remarks>
		private static bool isTypedFieldDescriptor(string value)
		{
			if (value.Length < 5 || !value.StartsWith("T20", StringComparison.Ordinal))
			{
				return false;
			}

			int i = 3;

			if (i < value.Length && value[i] == 'V')
			{
				i++;
				while (i < value.Length && value[i] >= '0' && value[i] <= '9')
				{
					i++;
				}

				if (i >= value.Length || value[i] != '_')
				{
					return false;
				}

				i++;
			}

			if (i >= value.Length)
			{
				return false;
			}

			char type = value[i];
			if (type != 'b' && type != 'd' && type != 's' && type != 'u')
			{
				return false;
			}

			return i + 1 < value.Length;
		}

		private static bool isPropertyName(string value)
		{
			if (isTypedFieldDescriptor(value))
			{
				return true;
			}

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
