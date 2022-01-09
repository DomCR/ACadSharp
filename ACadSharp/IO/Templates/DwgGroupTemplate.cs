using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgGroupTemplate : DwgTemplate<Group>
	{
		public List<ulong> Handles { get; set; } = new List<ulong>();

		public DwgGroupTemplate(Group group) : base(group) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var handle in this.Handles)
			{
				CadObject member = builder.GetCadObject(handle);
				if (member != null)
				{
					this.CadObject.Members.Add(handle, member);
				}
			}
		}
	}
}
