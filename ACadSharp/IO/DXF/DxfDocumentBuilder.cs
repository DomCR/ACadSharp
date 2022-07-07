using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfDocumentBuilder : CadDocumentBuilder
	{
		protected Dictionary<ulong, ICadTableTemplate> tableTemplates = new Dictionary<ulong, ICadTableTemplate>();

		public DxfDocumentBuilder(CadDocument document, NotificationEventHandler notification = null) : base(document, notification) { }

		public override void BuildDocument()
		{
			foreach (ICadTableTemplate table in tableTemplates.Values)
			{
				table.Build(this);
			}

			//Assign the owners for the different objects
			foreach (CadTemplate template in this.templates.Values)
			{
				this.assignOwners(template);
			}

			base.BuildDocument();
		}

		public void AddTableTemplate(ICadTableTemplate tableTemplate)
		{
			this.tableTemplates[tableTemplate.CadObject.Handle] = tableTemplate;
			this.cadObjects[tableTemplate.CadObject.Handle] = tableTemplate.CadObject;
		}

		private void assignOwners(CadTemplate template)
		{
			if (template.CadObject.Owner != null || template.CadObject is CadDictionary)
				return;

			if (this.TryGetCadObject(template.OwnerHandle, out CadObject owner))
			{
				switch (owner)
				{
					case CadDictionary:
						//Entries of the dictionary are assigned in the template
						break;
					case BlockRecord record when template.CadObject is Entity entity:
						record.Entities.Add(entity);
						break;
					case Polyline pline when template.CadObject is Vertex v:
						pline.Vertices.Add(v);
						break;
					case Polyline pline when template.CadObject is Seqend seqend:
						pline.Vertices.Seqend = seqend;
						break;
					case Insert insert when template.CadObject is AttributeEntity att:
						insert.Attributes.Add(att);
						break;
					case Insert insert when template.CadObject is Seqend seqend:
						insert.Attributes.Seqend = seqend;
						break;
					default:
						this.Notify(new NotificationEventArgs($"Owner {owner.GetType().Name} with handle {owner.Handle} assignation not implemented for {template.CadObject.GetType().Name} with handle {template.CadObject.Handle}"));
						break;
				}
			}
			else
			{
				this.Notify(new NotificationEventArgs($"Owner {template.OwnerHandle} not found for {template.GetType().FullName} with handle {template.CadObject.Handle}"));
			}
		}
	}
}
