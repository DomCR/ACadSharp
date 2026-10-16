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
	/// The records sit between a <c>TDBXDATAMAP_BLOCK_BEGIN_</c> and a
	/// <c>TDBXDATAMAP_BLOCK_END_</c> marker, and the writer emits those around them: the opening
	/// marker is written first, the records follow, and the closing marker is written last.
	/// </para>
	/// <para>
	/// A numeric record carries its name here and its value elsewhere — the value is written as a
	/// native number in the object's bit stream, which this reader does not walk — so such a record
	/// appears with no value rather than with the next record's name as one.
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
		/// A name whose descriptor says its value is a string is given the entry that follows it; any
		/// other name is recorded with no value, because its value is not in the stream. The key is
		/// present either way, so a caller can tell an absent property from one that has no value
		/// here.
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
			for (int i = 0; i < _entries.Count; i++)
			{
				string entry = _entries[i];

				if (entry == BlockBeginMarker || entry == BlockEndMarker)
				{
					continue;
				}

				if (isTypedFieldDescriptor(entry))
				{
					//The descriptor states in its own name whether a value can follow it, so a numeric,
					//point or identifier record is never paired with the next record's name.
					string value = null;

					if (descriptorCarriesStringValue(entry)
						&& i + 1 < _entries.Count
						&& !isTypedFieldDescriptor(_entries[i + 1]))
					{
						value = _entries[++i];
					}

					_properties[entry] = value;
					continue;
				}

				if (!isPropertyName(entry))
				{
					continue;
				}

				//A plain name is followed by its value unless the next entry is itself a name, which
				//is what a value that reads like one — the hatch pattern PUB_HATCH — would look like.
				if (i + 1 < _entries.Count && !isPropertyName(_entries[i + 1]))
				{
					_properties[entry] = _entries[++i];
				}
				else
				{
					_properties[entry] = null;
				}
			}
		}

		/// <summary>
		/// Decides whether a typed descriptor's value is written beside it in the stream.
		/// </summary>
		/// <remarks>
		/// Only a string is, and not always then. The block writes each record as its name, then its
		/// type, then its value through whichever filer slot that type belongs to. A string's value is
		/// the only one that goes to the string stream — a number, a point or an identifier is written
		/// as a native value in the object's bit stream, which the strings here do not contain — so an
		/// entry after a numeric descriptor is always the next record's name.
		/// <para>
		/// A string record whose value is a null pointer writes nothing at all, not even an empty
		/// string, so the next entry after such a record is again the next record's name. That is why
		/// the caller also checks the shape of the entry that follows.
		/// </para>
		/// </remarks>
		private static bool descriptorCarriesStringValue(string descriptor)
		{
			int i = 3;

			if (i < descriptor.Length && descriptor[i] == 'V')
			{
				i++;
				while (i < descriptor.Length && descriptor[i] >= '0' && descriptor[i] <= '9')
				{
					i++;
				}

				i++;
			}

			return i < descriptor.Length && descriptor[i] == 's';
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
