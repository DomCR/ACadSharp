using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="View"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableView"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockRecord"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableView)]
	[DxfSubClass(DxfSubclassMarker.View)]
	public class View : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableView;

		//TODO: finish View documentation

		public View() : base() { }

		public View(string name) : base(name) { }
	}
}
