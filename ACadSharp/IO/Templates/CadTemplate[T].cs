namespace ACadSharp.IO.Templates
{
	internal class CadTemplate<T> : CadTemplate
		where T : CadObject
	{
		public new T CadObject { get { return (T)base.CadObject; } set { base.CadObject = value; } }

		public CadTemplate(T cadObject) : base(cadObject) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
