using ACadSharp.Classes;
using ACadSharp.Header;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// [PATCH] DWG lazy (partial) read context.
	/// <para>
	/// Produced by <see cref="DwgReader.PreparePartialRead"/>: only reads the file header / section table / class table / header section / handle section
	/// (KB~MB-scale overhead), decoding no objects; the caller can then repeatedly use <see cref="DwgReader.ReadObjectsPartial"/>
	/// to decode objects on demand by a specified set of seed handles, avoiding the whole file's object section entering memory at once (large-file pruning scenario).
	/// </para>
	/// </summary>
	public class DwgPartialReadContext
	{
		/// <summary>DWG version (only AC1018/R2004 and later are supported).</summary>
		public ACadVersion Version { get; }

		/// <summary>Handle → offset within the object section (Handles section). Random-access primitive.</summary>
		public Dictionary<ulong, long> HandleMap { get; }

		/// <summary>Class table (Classes section).</summary>
		public DxfClassCollection Classes { get; }

		/// <summary>Handle set from the header section (includes MODEL_SPACE/PAPER_SPACE and each table's control object).</summary>
		public DwgHeaderHandlesCollection HeaderHandles { get; }

		/// <summary>Decoded CadHeader ($VAR/$DICTIONARY variables).</summary>
		public CadHeader Header { get; }

		/// <summary>Object section (decompressed, seekable) raw stream; its lifetime must cover all ReadObjectsPartial calls.</summary>
		public Stream ObjectsStream { get; }

		internal DwgPartialReadContext(
			ACadVersion version,
			Dictionary<ulong, long> handleMap,
			DxfClassCollection classes,
			DwgHeaderHandlesCollection headerHandles,
			CadHeader header,
			Stream objectsStream)
		{
			this.Version = version;
			this.HandleMap = handleMap;
			this.Classes = classes;
			this.HeaderHandles = headerHandles;
			this.Header = header;
			this.ObjectsStream = objectsStream;
		}
	}
}
