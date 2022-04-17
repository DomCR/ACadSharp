using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfDocumentBuilder : CadDocumentBuilder
	{
		public Dictionary<ulong, ICadTableTemplate> TableTemplates { get; } = new Dictionary<ulong, ICadTableTemplate>();

		public DxfDocumentBuilder(CadDocument document, NotificationEventHandler notification = null) : base(document, notification) { }

		public override void BuildDocument()
		{
			//Assign the owners for the different objects
			foreach (CadTemplate template in this.Templates.Values)
			{
				this.assignOwners(template);
			}

			base.BuildDocument();
		}

		private void assignOwners(CadTemplate template)
		{
			if (template.CadObject.Owner != null)
				return;

			if (!template.OwnerHandle.HasValue)
			{
				this.NotificationHandler?.Invoke(null, new NotificationEventArgs($"Owner not found for {template.GetType().FullName} with handle {template.CadObject.Handle}"));
			}
			else if (this.TryGetCadObject(template.OwnerHandle.Value, out CadObject co))
			{
				switch (co)
				{
					case BlockRecord record when template.CadObject is Entity entity:
						record.Entities.Add(entity);
						break;
					case Polyline pline when template.CadObject is Vertex v:
						pline.Vertices.Add(v);
						break;
					case Insert insert when template.CadObject is AttributeEntity att:
						insert.Attributes.Add(att);
						break;
					default:
						this.NotificationHandler?.Invoke(null, new NotificationEventArgs($"Owner {co.GetType().Name} with handle {co.Handle} assignation not implemented for {template.CadObject.GetType().Name} with handle {template.CadObject.Handle}"));
						break;
				}
			}
			else
			{
				this.NotificationHandler?.Invoke(null, new NotificationEventArgs($"Owner {template.OwnerHandle} not found for {template.GetType().FullName} with handle {template.CadObject.Handle}"));
			}
		}
	}
}
