#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	//Is it present in the DXF ??
	public enum AttributeType   //TODO: Check how AttributeType is assigned in Autocad
	{
		SingleLine = 1,
		MultiLine = 2,
		ConstantMultiLine = 4,
	}

	/// <summary>
	/// Common base class for <see cref="AttributeEntity" /> and <see cref="AttributeDefinition" />.
	/// </summary>
	public abstract class AttributeBase : TextEntity
	{
		[DxfCodeValue(74)]
		public override TextVerticalAlignment VerticalAlignment { get; set; } = TextVerticalAlignment.Baseline;

		[DxfCodeValue(280)]
		public byte Version { get; set; }
		/// <summary>
		/// Specifies the tag string of the object
		/// </summary>
		/// <value>
		/// Cannot contain spaces
		/// </value> 
		[DxfCodeValue(2)]
		public string Tag { get; set; }

		/// <summary>
		/// Attribute flags
		/// </summary>
		[DxfCodeValue(70)]
		public AttributeFlags Flags { get; set; }

		/// <summary>
		/// MText flag
		/// </summary>
		//[DxfCodeValue(280)]
		//TODO: Check the dxf code of Attribute type.
		//Missmatch between Autodesk documentation and OpenDesign
		public AttributeType AttributeType { get; set; }

		//Missmatch between Autodesk documentation and OpenDesign
		public bool IsReallyLocked { get; set; }

		public AttributeBase() : base() { }
	}
}
