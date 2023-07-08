using ACadSharp.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="CadDictionaryWithDefault"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionary"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dictionary"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionary)]
	[DxfSubClass(DxfSubclassMarker.DictionaryWithDefault)]
	public class CadDictionaryWithDefault : CadDictionary
	{
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		//100	Subclass marker(AcDbDictionaryWithDefault)

		//340	Hard pointer to default object ID/handle(currently only used for plot style dictionary's default entry, named “Normal”)
		public CadObject DefaultEntry { get; set; }
	}
}
