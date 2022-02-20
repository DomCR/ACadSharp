using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionReaderBase
	{
		protected delegate bool checkDxfCodeValue(DwgTemplate template);

		protected readonly IDxfStreamReader _reader;
		protected readonly DxfDocumentBuilder _builder;
		protected readonly NotificationEventHandler _notification;

		public DxfSectionReaderBase(
			IDxfStreamReader reader,
			DxfDocumentBuilder builder,
			NotificationEventHandler notification = null)
		{
			this._reader = reader;
			this._builder = builder;
			this._notification = notification;
		}

		public abstract void Read();

		protected bool readUntilStart() => this._reader.LastDxfCode != DxfCode.Start;

		protected bool readUntilSubClass() => this._reader.LastDxfCode != DxfCode.Subclass;

		protected void readCommonObjectCodes(DwgTemplate template)
		{
			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
				{
					//Handle
					case 5:
						template.CadObject.Handle = this._reader.LastValueAsHandle;
						break;
					//Start of application - defined group
					case 102:
						//TODO: read dictionary groups for entities
						do
						{
							this._reader.ReadNext();
						}
						while (this._reader.LastDxfCode != DxfCode.ControlString);
						break;
					//Soft - pointer ID / handle to owner BLOCK_RECORD object
					case 330:
						template.OwnerHandle = this._reader.LastValueAsHandle;
						break;
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}."));
						break;
				}

				this._reader.ReadNext();
			}
		}

		protected DwgTemplate readEntity()
		{
			DwgEntityTemplate template = null;

			switch (this._reader.LastValueAsString)
			{
				case DxfFileToken.EntityLine:
					template = new DwgEntityTemplate(new Line());
					break;
				case DxfFileToken.EntityInsert:
					template = new DwgInsertTemplate(new Insert());
					break;
				case DxfFileToken.EntityMText:
					template = new DwgTextEntityTemplate(new MText());
					break;
				case DxfFileToken.EntityPoint:
					template = new DwgEntityTemplate(new Point());
					break;
				case DxfFileToken.EntityCircle:
					template = new DwgEntityTemplate(new Circle());
					break;
				case DxfFileToken.EntityAttributeDefinition:
					template = new DwgTextEntityTemplate(new AttributeDefinition());
					break;
				default:
					Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
					this._notification?.Invoke(null, new NotificationEventArgs($"Entity not implemented: {this._reader.LastValueAsString} at line {this._reader.Line}."));
					break;
			}

			this.readCommonObjectCodes(template);

			//Jump subclass marker
			Debug.Assert(this._reader.LastValueAsString == DxfSubclassMarker.Entity);
			this._reader.ReadNext();

			this.readRawMap(template, null, this.readCommonEntity, readUntilSubClass);

			switch (this._reader.LastValueAsString)
			{
				case DxfSubclassMarker.Line:
					readRawMap(template, this._reader.LastValueAsString, readLine, readUntilStart);
					break;
				case DxfSubclassMarker.Insert:
					readRawMap(template, this._reader.LastValueAsString, readInsert, readUntilStart);
					break;
				case DxfSubclassMarker.MText:
					readRawMap(template, this._reader.LastValueAsString, readMText, readUntilStart);
					break;
				case DxfSubclassMarker.Point:
					readRawMap(template, this._reader.LastValueAsString, readPoint, readUntilStart);
					break;
				case DxfSubclassMarker.Circle:
					readRawMap(template, this._reader.LastValueAsString, null, readUntilStart);
					break;
				case DxfSubclassMarker.AttributeDefinition:
					readRawMap(template, this._reader.LastValueAsString, null, readUntilStart);
					break;
				default:
					Debug.Fail($"Unhandeled dxf entity {this._reader.LastValueAsString} at line {this._reader.Line}.");
					break;
			}

			return template;
		}

		protected bool readCommonEntity(DwgTemplate template)
		{
			DwgEntityTemplate entityTemplate = template as DwgEntityTemplate;

			switch (this._reader.LastCode)
			{
				//Layer name
				case 8:
					entityTemplate.LayerName = this._reader.LastValueAsString;
					return true;
				//Hard-pointer ID/handle to material object (present if not BYLAYER)
				case 347:
				//Hard - pointer ID / handle to the plot style object
				case 390:
				//APP: layout tab name
				case 410:
					Debug.Fail("Entity code not readed");
					return true;
				//Transparency value
				case 440:
					//TODO: implement the transparency read
					return true;
				default:
					break;
			}

			return false;
		}

		protected bool readLine(DwgTemplate template)
		{
			DwgEntityTemplate entityTemplate = template as DwgEntityTemplate;

			switch (this._reader.LastCode)
			{
				default:
					break;
			}

			return false;
		}

		protected bool readInsert(DwgTemplate template)
		{
			DwgInsertTemplate insertTemplate = template as DwgInsertTemplate;

			switch (this._reader.LastCode)
			{
				//Block name
				case 2:
					insertTemplate.BlockName = this._reader.LastValueAsString;
					return true;
				default:
					break;
			}

			return false;
		}

		protected bool readMText(DwgTemplate template)
		{
			DwgTextEntityTemplate mtextTemplate = template as DwgTextEntityTemplate;

			switch (this._reader.LastCode)
			{
				default:
					break;
			}

			return false;
		}

		protected bool readPoint(DwgTemplate template)
		{
			DwgEntityTemplate lineTemplate = template as DwgEntityTemplate;

			switch (this._reader.LastCode)
			{
				default:
					break;
			}

			return false;
		}

		/// <summary>
		/// Util method for a fast implementation.
		/// Read a cad object using the common dxf methods and assign it to the object.
		/// </summary>
		/// <param name="template"></param>
		/// <param name="subclass"></param>
		/// <param name="check">Delegate for specific codes such as handles ore assignation to the template and not the CadObject</param>
		/// <param name="readUntil"></param>
		/// <remarks>
		/// This method will disappear once all the objects are implemented
		/// </remarks>
		/// <returns></returns>
		protected void readRawMap(DwgTemplate template, string subclass, checkDxfCodeValue check, Func<bool> readUntil)
		{
			//TODO: read raw should notify the codes that aren't assigned

			Dictionary<DxfCode, object> map = new Dictionary<DxfCode, object>();

			Debug.Assert(string.IsNullOrEmpty(subclass) || this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(string.IsNullOrEmpty(subclass) || this._reader.LastValueAsString == subclass);

			//while (this._reader.LastDxfCode != DxfCode.Start)
			while (readUntil.Invoke())
			{
				if (check != null && check(template))
				{
					this._reader.ReadNext();
					continue;
				}

				try
				{
					//Add the value
					map.Add(this._reader.LastDxfCode, this._reader.LastValue);
				}
				catch (Exception)
				{
					this._builder.NotificationHandler?.Invoke(
						template.CadObject,
						new NotificationEventArgs($"Code already in the map for the reflection reader\n" +
						$"\tcode : {this._reader.LastCode}\n" +
						$"\ttype : {template.CadObject.ObjectType}"));
				}

				this._reader.ReadNext();
			}

			//Build the table based on the map
			template.CadObject.Build(map);
		}
	}
}
