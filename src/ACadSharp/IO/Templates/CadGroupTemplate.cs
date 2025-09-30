using ACadSharp.Entities;
using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadGroupTemplate : CadTemplate<Group>
	{
		public HashSet<ulong> Handles { get; set; } = new();

		public CadGroupTemplate() : base(new Group()) { }

		public CadGroupTemplate(Group group) : base(group) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var handle in this.Handles)
			{
				if (builder.TryGetObjectTemplate(handle, out CadEntityTemplate e))
				{
					e.Build(builder);
					this.CadObject.Add(e.CadObject);
				}
				else
				{
					builder.Notify($"Entity with handle {handle} not found for group {this.CadObject.Handle}", NotificationType.Warning);
				}
			}
		}
	}
}
