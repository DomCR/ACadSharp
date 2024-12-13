using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="XRecord"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectXRecord"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.XRecord"/>
	/// <br/>
	/// <br/>
	/// Is not recommended to modify, add or remove entries to an XRecord unless you know what consequences will cause in the document.
	/// </remarks>
	[DxfName(DxfFileToken.ObjectXRecord)]
	[DxfSubClass(DxfSubclassMarker.XRecord)]
	public partial class XRecord : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.XRECORD;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectXRecord;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.XRecord;

		/// <summary>
		/// Duplicate record cloning flag (determines how to merge duplicate entries)
		/// </summary>
		[DxfCodeValue(280)]
		public DictionaryCloningFlags CloningFlags { get; set; }

		//1-369 (except 5 and 105)	These values can be used by an application in any way
		public IEnumerable<Entry> Entries { get { return this._entries; } }

		private readonly List<Entry> _entries = new List<Entry>();

		public XRecord() : base() { }

		public XRecord(string name) : base(name) { }

		public void CreateEntry(int code, object value)
		{
			this._entries.Add(new Entry(code, value, this));
		}
	}
}
