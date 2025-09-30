using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadInsertTemplate : CadEntityTemplate
	{
		public bool HasAtts { get; set; }

		public int OwnedObjectsCount { get; set; }

		public ulong? BlockHeaderHandle { get; set; }

		public string BlockName { get; set; }

		public ulong? FirstAttributeHandle { get; set; }

		public ulong? EndAttributeHandle { get; set; }

		public ulong? SeqendHandle { get; set; }

		public HashSet<ulong> AttributesHandles { get; set; } = new();

		public CadInsertTemplate() : base(new Insert()) { }

		public CadInsertTemplate(Insert insert) : base(insert) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (!(this.CadObject is Insert insert))
				return;

			BlockRecord block;
			if (this.getTableReference(builder, this.BlockHeaderHandle, this.BlockName, out BlockRecord owner))
			{
				insert.Block = owner;
			}

			if (builder.TryGetCadObject(this.SeqendHandle, out Seqend seqend))
			{
				insert.Attributes.Seqend = seqend;
			}

			if (this.FirstAttributeHandle.HasValue)
			{
				var attributes = getEntitiesCollection<AttributeEntity>(builder, FirstAttributeHandle.Value, EndAttributeHandle.Value);
				insert.Attributes.AddRange(attributes);
			}
			else
			{
				foreach (ulong handle in this.AttributesHandles)
				{
					if (builder.TryGetCadObject(handle, out AttributeEntity att))
					{
						insert.Attributes.Add(att);
					}
				}
			}
		}
	}
}
