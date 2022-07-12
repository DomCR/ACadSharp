using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="XRecrod"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableXRecord"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.XRecord"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableXRecord)]
	[DxfSubClass(DxfSubclassMarker.XRecord)]
	public class XRecrod : CadObject
	{
		public override ObjectType ObjectType => ObjectType.XRECORD;

		public override string ObjectName => DxfFileToken.TableXRecord;

		/// <summary>
		/// Duplicate record cloning flag (determines how to merge duplicate entries)
		/// </summary>
		[DxfCodeValue(280)]
		public DictionaryCloningFlags ClonningFlags { get; set; }

		//1-369 (except 5 and 105)	These values can be used by an application in any way
		public List<Entry> Entries { get; set; } = new List<Entry>();

		public class Entry
		{
			public int Code { get; }

			public object Value { get; }

			public Entry(int code, object value)
			{
				this.Code = code;
				this.Value = value;
			}
		}
	}
}
