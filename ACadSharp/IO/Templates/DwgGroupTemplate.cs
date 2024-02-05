using ACadSharp.Entities;
using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgGroupTemplate : CadTemplate<Group>
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();

		public DwgGroupTemplate(Group group) : base(group) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var handle in this.Handles)
			{
				if (builder.TryGetCadObject<Entity>(handle, out Entity e))
				{
					this.CadObject.Entities.Add(handle, e);
				}
				else
				{
					CadObject cad = builder.GetCadObject(handle);
					if (cad != null)
					{
						builder.Notify($"CadObject with handle {cad.GetType().FullName}:{handle} is not an entity and could not be added in group {this.CadObject.Handle}", NotificationType.Warning);
					}
					else
					{
						builder.Notify($"Entity with handle {handle} not found for group {this.CadObject.Handle}", NotificationType.Warning);
					}
				}
			}
		}
	}
}
