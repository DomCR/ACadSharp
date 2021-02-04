using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	public class VPort : TableEntry
	{
		public override string ObjectName => DxfFileToken.TableVport;

		public VPort() : base() { }

		internal VPort(DxfEntryTemplate template) : base(template) { }
	}
}
