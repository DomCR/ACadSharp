using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgInsertTemplate : DwgEntityTemplate
	{
		public bool HasAtts { get; set; }

		public int OwnedObjectsCount { get; set; }

		public ulong? BlockHeaderHandle { get; set; }

		public string BlockName { get; set; }

		public ulong? FirstAttributeHandle { get; set; }

		public ulong? EndAttributeHandle { get; set; }

		public ulong SeqendHandle { get; set; }

		public List<ulong> OwnedHandles { get; set; } = new List<ulong>();

		public DwgInsertTemplate(Insert insert) : base(insert) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Insert insert = this.CadObject as Insert;

			if (this.BlockHeaderHandle.HasValue)
			{
				// orig code
				// insert.Block = builder.GetCadObject<Block>(this.BlockHeaderHandle.Value);

				// problem: builder.GetCadObject<Block>(this.BlockHeaderHandle.Value) returns null
				// because, in the case of an Insert object, GetObject fails the type (Block) check.
				// the templates' CadObject is not of type "Block"

				// proposed solution:
				// 1) extract the handle	
				ulong handle = this.BlockHeaderHandle.Value;

				// 2) attempt to use the original GetCadObject
				Block bl = builder.GetCadObject<Block>(handle);

				if (bl != null) 
				{
					// 3) if the original GetCadObject returns a Block, use it
					insert.Block = bl;
				} 
				else 
				{
					// 4) if the original GetCadObject returns null, use the BlockEntity
					// from the BlockRecordTemplate
					DwgBlockRecordTemplate brt = builder.GetObjectTemplate<DwgBlockRecordTemplate>(handle);
					insert.Block = brt.CadObject.BlockEntity;
				}
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

			//TODO: DwgInsertTemplate SeqendHandle??
		}
	}
}
