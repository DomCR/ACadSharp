namespace ACadSharp.Tables.Collections
{
	internal class ViewportEntityControl : Table<ViewportEntityHeader>
	{
		public override ObjectType ObjectType { get => ObjectType.VP_ENT_HDR_CTRL_OBJ; }

		protected override string[] defaultEntries { get; }

		public ViewportEntityControl(CadDocument document) : base(document)
		{
		}
	}
}