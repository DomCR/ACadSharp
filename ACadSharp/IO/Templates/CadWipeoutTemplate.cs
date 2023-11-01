using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadWipeoutTemplate : CadEntityTemplate
	{
		public ulong? ImgHandle_1 { get; set; }

		public ulong? ImgHandle_2 { get; set; }

		public CadWipeoutTemplate(Wipeout wipeout) : base(wipeout) { }
	}
}
