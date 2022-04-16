using ACadSharp.Entities;
using ACadSharp.Exceptions;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.DXF
{
	internal class DxfEntitiesSectionReader : DxfSectionReaderBase
	{
		public DxfEntitiesSectionReader(IDxfStreamReader reader, DxfDocumentBuilder builder, NotificationEventHandler notification = null)
			: base(reader, builder, notification)
		{
		}

		public override void Read()
		{
			//Advance to the first value in the section
			this._reader.ReadNext();

			//Loop until the section ends
			while (this._reader.LastValueAsString != DxfFileToken.EndSection)
			{
				CadEntityTemplate template = this.readEntity();

				if (template == null)
					continue;

				if (this._builder.TryGetCadObject(template.OwnerHandle.Value, out CadObject co))
				{
					switch (co)
					{
						case BlockRecord record:
							record.Entities.Add(template.CadObject);
							break;
						case Polyline pline when template.CadObject is Vertex v:
							pline.Vertices.Add(v);
							break;
						case Insert insert when template.CadObject is AttributeEntity att:
							insert.Attributes.Add(att);
							break;
						default:
							this._notification?.Invoke(null, new NotificationEventArgs($"The owner of entity {template.CadObject.Handle} is a {co.GetType().FullName} with handle {co.Handle}"));
							break;
					}
				}
				else
				{
					this._notification?.Invoke(null, new NotificationEventArgs($"Block record owner {template.OwnerHandle} not found for entity {template.CadObject.Handle}"));
				}

				//Add the object and the template to the builder
				this._builder.Templates[template.CadObject.Handle] = template;
			}
		}
	}
}
