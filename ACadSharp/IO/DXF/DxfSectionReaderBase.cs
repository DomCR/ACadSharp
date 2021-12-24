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
				default:
					Debug.Fail($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}.");
					this._notification?.Invoke(null, new NotificationEventArgs($"Entity not implemented: {this._reader.LastValueAsString} at line {this._reader.Line}."));
					break;
			}

			readCommonObjectCodes(template);

			//Jump subclass marker
			this._reader.ReadNext();

			readCommonEntity(template);

			throw new NotImplementedException();
		}

		protected void readCommonEntity(DwgEntityTemplate template)
		{
			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				//TODO: implement the dxf endblock reader

				this._reader.ReadNext();
			}
		}

		/// <summary>
		/// Util method for a fast implementation.
		/// Read a cad object using the common dxf methods and assign it to the object.
		/// </summary>
		/// <param name="template"></param>
		/// <param name="subclass"></param>
		/// <param name="check">Delegate for specific codes such as handles ore assignation to the template and not the CadObject</param>
		/// <returns></returns>
		protected DwgTemplate readRaw(DwgTemplate template, string subclass, checkDxfCodeValue check)
		{
			Dictionary<DxfCode, object> map = new Dictionary<DxfCode, object>();

			Debug.Assert(this._reader.LastDxfCode == DxfCode.Subclass);
			Debug.Assert(this._reader.LastValueAsString == subclass);

			while (this._reader.LastDxfCode != DxfCode.Start)
			{
				if (check(template))
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
					_builder.NotificationHandler?.Invoke(
						template.CadObject,
						new NotificationEventArgs($"Code already in the map for the reflection reader\n" +
						$"\tcode : {this._reader.LastCode}\n" +
						$"\ttype : {template.CadObject.ObjectType}"));
				}

				this._reader.ReadNext();
			}

			//Build the table based on the map
			template.CadObject.Build(map);

			return template;
		}
	}
}
