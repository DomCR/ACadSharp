using ACadSharp.IO.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables.Collections
{
	public class LayersTable : Table<Layer>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LAYER_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLayer;

		internal LayersTable() { }

		internal LayersTable(CadDocument document) : base(document) { }
	}
}