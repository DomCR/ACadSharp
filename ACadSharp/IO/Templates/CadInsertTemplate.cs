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

		public List<ulong> OwnedHandles { get; set; } = new List<ulong>();

		public CadInsertTemplate(Insert insert) : base(insert) { }

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 2:
					this.BlockName = name;
					value = true;
					break;
				default:
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Insert insert = this.CadObject as Insert;

			if (builder.TryGetCadObject(this.BlockHeaderHandle, out BlockRecord block))
			{
				insert.Block = block;
			}

			if (this.FirstAttributeHandle.HasValue)
			{
				var attributes = getEntitiesCollection<Entities.AttributeEntity>(builder, FirstAttributeHandle.Value, EndAttributeHandle.Value);
				insert.Attributes.AddRange(attributes);
			}
			else
			{
				foreach (ulong handle in this.OwnedHandles)
				{
					var att = builder.GetCadObject<Entities.AttributeEntity>(handle);
					insert.Attributes.Add(att);
				}
			}

			if(builder.TryGetCadObject<Seqend>(this.SeqendHandle, out Seqend seqend))
			{
				insert.Attributes.Seqend = seqend;
			}
		}
	}
}
