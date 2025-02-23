using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace ACadSharp.IO.SVG
{
	internal class SvgXmlWriter : XmlTextWriter
	{
		public SvgXmlWriter(TextWriter w) : base(w)
		{
		}

		public SvgXmlWriter(Stream w, Encoding encoding) : base(w, encoding)
		{
		}

		public SvgXmlWriter(string filename, Encoding encoding) : base(filename, encoding)
		{
		}

		public void WriteAttributeString(string localName, double value)
		{
			this.WriteAttributeString(localName, value.ToString(CultureInfo.InvariantCulture));
		}
	}
}
