using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.Templates
{
	internal class DwgTemplate<T> : DwgTemplate
		where T : CadObject
	{
		public new T CadObject { get { return (T)base.CadObject; } set { base.CadObject = value; } }
		public DwgTemplate(T cadObject) : base(cadObject) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);
		}
	}
}
