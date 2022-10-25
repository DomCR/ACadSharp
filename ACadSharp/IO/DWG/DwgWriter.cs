using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	public class DwgWriter : CadWriterBase
	{
		private CadDocument _document;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="document"></param>
		/// <exception cref="NotImplementedException">Binary writer not implemented</exception>
		public DwgWriter(string filename, CadDocument document)
			: this(File.Create(filename), document)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="document"></param>
		/// <exception cref="NotImplementedException">Binary writer not implemented</exception>
		public DwgWriter(Stream stream, CadDocument document)
		{
			this._document = document;
		}

		public void Write()
		{

		}

		/// <inheritdoc/>
		public override void Dispose()
		{

		}
	}
}
