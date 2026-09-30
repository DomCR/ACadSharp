using ACadSharp.Classes;
using ACadSharp.Header;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// [PATCH] DWG lazy (partial) read context.
	/// <para>
	/// Produced by <see cref="DwgReader.PreparePartialRead"/>: reads only the file header / section map / class table /
	/// header section / handles section (KB~MB cost) without decoding any object; the caller can then call
	/// <see cref="DwgReader.ReadObjectsPartial"/> multiple times to decode objects on demand from a seed handle set, avoiding the whole object section in memory at once (large-file pruning scenarios).
	/// </para>
	/// </summary>
	public class DwgPartialReadContext
	{
		/// <summary>DWG version (only AC1018/R2004 and above are supported).</summary>
		public ACadVersion Version { get; }

		/// <summary>Handle -> offset inside the object section (Handles section). The random-access primitive.</summary>
		public Dictionary<ulong, long> HandleMap { get; }

		/// <summary>Class table (Classes section).</summary>
		public DxfClassCollection Classes { get; }

		/// <summary>Header-section handle set (includes MODEL_SPACE/PAPER_SPACE and the table control objects).</summary>
		public DwgHeaderHandlesCollection HeaderHandles { get; }

		/// <summary>Decoded CadHeader (/ variables).</summary>
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
