using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ACadSharp.IO.DXF
{
	internal abstract class DxfSectionReaderBase
	{
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

		protected void readCommonObjectData(out string name, out ulong handle, out ulong? ownerHandle)
		{
			name = null;
			handle = 0;
			ownerHandle = null;

			bool handleNotFound = true;

			//Loop until the common data end
			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
				{
					//Table name
					case 0:
					case 2:
						name = this._reader.LastValueAsString;
						break;
					//Handle
					case 5:
					case 105:
						handle = this._reader.LastValueAsHandle;
						handleNotFound = false;
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
						ownerHandle = this._reader.LastValueAsHandle;
						break;
					case 71:
					//Number of entries for dimension style table
					case 340:
					//Dimension table has the handles of the styles at the begining
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Unhandeled dxf code {this._reader.LastCode} at line {this._reader.Line}."));
						break;
				}

				this._reader.ReadNext();
			}

			if (handleNotFound) //TODO: Set exception for no handle
				throw new Exception();
		}

		protected void readCommonObjectData(CadTemplate template)
		{
			while (this._reader.LastDxfCode != DxfCode.Subclass)
			{
				switch (this._reader.LastCode)
				{
					//object name
					case 0:
						Debug.Assert(template.CadObject.ObjectName == this._reader.LastValueAsString);
						break;
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

		protected CadEntityTemplate readEntity()
		{
			CadEntityTemplate template = null;

			switch (this._reader.LastValueAsString)
			{
				case DxfFileToken.EntityAttribute:
					template = new DwgTextEntityTemplate(new AttributeEntity());
					break;
				case DxfFileToken.EntityAttributeDefinition:
					template = new DwgTextEntityTemplate(new AttributeDefinition());
					break;
				case DxfFileToken.EntityArc:
					template = new CadEntityTemplate(new Arc());
					break;
				case DxfFileToken.EntityCircle:
					template = new CadEntityTemplate(new Circle());
					break;
				case DxfFileToken.EntityEllipse:
					template = new CadEntityTemplate(new Ellipse());
					break;
				case DxfFileToken.EntityLine:
					template = new CadEntityTemplate(new Line());
					break;
				case DxfFileToken.EntityInsert:
					template = new DwgInsertTemplate(new Insert());
					break;
				case DxfFileToken.EntityMText:
					template = new DwgTextEntityTemplate(new MText());
					break;
				case DxfFileToken.EntityPoint:
					template = new CadEntityTemplate(new Point());
					break;
				case DxfFileToken.EntityPolyline:
					template = new CadPolyLineTemplate();
					break;
				case DxfFileToken.EntityRay:
					template = new CadEntityTemplate(new Ray());
					break;
				case DxfFileToken.EntitySolid:
					template = new CadEntityTemplate(new Solid());
					break;
				case DxfFileToken.EntityText:
					template = new CadEntityTemplate(new TextEntity());
					break;
				case DxfFileToken.EntityVertex:
					template = new CadEntityTemplate(new Vertex2D());
					break;
				case DxfFileToken.EntityXline:
					template = new CadEntityTemplate(new XLine());
					break;
				default:
					this._notification?.Invoke(null, new NotificationEventArgs($"Entity not implemented: {this._reader.LastValueAsString}"));
					do
					{
						this._reader.ReadNext();
					}
					while (this._reader.LastDxfCode != DxfCode.Start);
					return null;
			}

			//Jump the 0 marker
			this._reader.ReadNext();

			this.readCommonObjectData(template);

			while (this._reader.LastDxfCode == DxfCode.Subclass)
			{
				switch (this._reader.LastValueAsString)
				{
					case DxfSubclassMarker.Attribute:
						this.readMapped<AttributeEntity>(template.CadObject, template);
						break;
					case DxfSubclassMarker.AttributeDefinition:
						this.readMapped<AttributeDefinition>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Arc:
						this.readMapped<Arc>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Circle:
						this.readMapped<Circle>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Ellipse:
						this.readMapped<Ellipse>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Entity:
						this.readMapped<Entity>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Insert:
						this.readMapped<Insert>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Line:
						this.readMapped<Line>(template.CadObject, template);
						break;
					case DxfSubclassMarker.MText:
						this.readMapped<MText>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Point:
						this.readMapped<Point>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Polyline:
						(template as CadPolyLineTemplate).SetPolyLineObject(new PolyLine2D());
						this.readMapped<PolyLine2D>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Polyline3d:
						(template as CadPolyLineTemplate).SetPolyLineObject(new PolyLine3D());
						this.readMapped<PolyLine3D>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Text:
						this.readMapped<TextEntity>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Trace:
						this.readMapped<Solid>(template.CadObject, template);
						break;
					case DxfSubclassMarker.Vertex:
						this.readMapped<Vertex>(template.CadObject, template);
						break;
					default:
						this._notification?.Invoke(null, new NotificationEventArgs($"Unhandeled dxf entity subclass {this._reader.LastValueAsString}"));
						while (this._reader.LastDxfCode != DxfCode.Start)
							this._reader.ReadNext();
						break;
				}
			}

			return template;
		}

		protected void readMapped<T>(CadObject cadObject, CadTemplate template)
			where T : CadObject
		{
			DxfClassMap map = DxfClassMap.Create<T>();

			Debug.Assert(map.Name == this._reader.LastValueAsString);
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.Start
				&& this._reader.LastDxfCode != DxfCode.Subclass)
			{
				//Check for an extended data code
				if (this._reader.LastDxfCode >= DxfCode.ExtendedDataAsciiString)
				{
					this.readExtendedData(cadObject);
					this._reader.ReadNext();
					continue;
				}
				else if(this._reader.LastDxfCode == DxfCode.ControlString)
				{
					this.readReactors();
					continue;
				}

				if (!map.DxfProperties.TryGetValue(this._reader.LastCode, out DxfProperty dxfProperty))
				{
					if (!template.CheckDxfCode(this._reader.LastCode, this._reader.LastValue))
						this._notification?.Invoke(null, new NotificationEventArgs($"Dxf code {this._reader.LastCode} not found in map for {typeof(T)} | value : {this._reader.LastValueAsString}"));

					this._reader.ReadNext();
					continue;
				}

				if (dxfProperty.ReferenceType == DxfReferenceType.Handle)
				{
					//TODO: references may be also names in case of layers, blocks...
					if (!template.AddHandle(this._reader.LastCode, this._reader.LastValueAsHandle))
						this._notification?.Invoke(null, new NotificationEventArgs($"Dxf referenced code {this._reader.LastCode} not implemented in the {template.GetType().Name} for {typeof(T)} | value : {this._reader.LastValueAsHandle}"));
				}
				else if (dxfProperty.ReferenceType == DxfReferenceType.Name)
				{
					//TODO: references may be also names in case of layers, blocks...
					if (!template.AddName(this._reader.LastCode, this._reader.LastValueAsString))
						this._notification?.Invoke(null, new NotificationEventArgs($"Dxf named referenced code {this._reader.LastCode} not implemented in the {template.GetType().Name} for {typeof(T)} | value : {this._reader.LastValueAsHandle}"));
				}
				else if (dxfProperty.ReferenceType == DxfReferenceType.Count)
				{
					//Do nothing just marks the amount
				}
				else
				{
					switch (this._reader.LastGroupCodeValue)
					{
						case GroupCodeValueType.String:
						case GroupCodeValueType.Point3D:
						case GroupCodeValueType.Double:
						case GroupCodeValueType.Int16:
						case GroupCodeValueType.Int32:
						case GroupCodeValueType.Int64:
						case GroupCodeValueType.Chunk:
						case GroupCodeValueType.Bool:
							dxfProperty.SetValue(cadObject, this._reader.LastValue);
							break;
						case GroupCodeValueType.Comment:
							this._notification?.Invoke(null, new NotificationEventArgs($"Comment in the file :  {this._reader.LastValueAsString}"));
							break;
						case GroupCodeValueType.Handle:
						case GroupCodeValueType.ObjectId:
						case GroupCodeValueType.None:
						default:
							this._notification?.Invoke(null, new NotificationEventArgs($"Group Code not handled {this._reader.LastGroupCodeValue} for {typeof(T)}, code : {this._reader.LastCode} | value : {this._reader.LastValueAsString}"));
							break;
					}
				}

				this._reader.ReadNext();
			}
		}

		private void readExtendedData(CadObject cadObject)
		{
			//TODO: Handle extended data 
		}

		private void readReactors()
		{
			this._reader.ReadNext();

			while (this._reader.LastDxfCode != DxfCode.ControlString)
			{
				this._reader.ReadNext();
			}

			this._reader.ReadNext();
		}
	}
}
