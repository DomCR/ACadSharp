using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Templates
{
	//TODO: Replace dxf templates for dwg templates, use only 1 kind of template.

	internal class DxfEntityTemplate : Entity
	{
		public override ObjectType ObjectType => ObjectType.INVALID;
		public string EntityName { get; set; }
	}
	internal class DxfEntryTemplate : TableEntry 
	{
		public override ObjectType ObjectType => ObjectType.INVALID;
		public string TableName { get; set; }
	}
	internal class DxfTableTemplate : CadObject
	{
		public override ObjectType ObjectType => ObjectType.INVALID;
		public string Name { get; set; }
		public int MaxEntries { get; internal set; }
	}
}
