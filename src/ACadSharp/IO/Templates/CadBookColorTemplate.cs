using ACadSharp.Objects;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadBookColorTemplate : CadTemplate
	{
		public string Name { get; set; }
		public string BookName { get; set; }

		public CadBookColorTemplate(BookColor color) : base(color) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			//throw new NotImplementedException();
		}
	}
}
