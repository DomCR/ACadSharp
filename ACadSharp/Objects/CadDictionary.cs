﻿using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Objects
{
	public class CadDictionary : CadObject
	{
		public override string ObjectName => DxfFileToken.ObjectDictionary;
		//100	Subclass marker(AcDbDictionary)
		/// <summary>
		/// indicates that elements of the dictionary are to be treated as hard-owned.
		/// </summary>
		[DxfCodeValue(DxfCode.Int8)]
		public bool HardOwnerFlag { get; set; }
		/// <summary>
		/// Duplicate record cloning flag (determines how to merge duplicate entries)
		/// </summary>
		[DxfCodeValue(DxfCode.DictionaryFlags)]
		public DictionaryCloningFlags ClonningFlags { get; set; }


		//3	Entry name(one for each entry) (optional)
		//350	Soft-owner ID/handle to entry object (one for each entry) (optional)
		public Dictionary<string, string> Entries { get; set; } = new Dictionary<string, string>();
	}
}
