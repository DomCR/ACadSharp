using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadImageTemplate : CadEntityTemplate
	{
		public ulong? ImgHandle_1 { get; set; }

		public ulong? ImgHandle_2 { get; set; }

		public CadImageTemplate(CadImageBase wipeout) : base(wipeout) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if(builder.TryGetCadObject(ImgHandle_1, out CadObject obj))
			{

			}
			if(builder.TryGetCadObject(ImgHandle_2, out CadObject obj1))
			{

			}
		}
	}
}
