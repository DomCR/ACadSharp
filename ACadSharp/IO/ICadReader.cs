using ACadSharp.Header;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO
{
	public interface ICadReader : IDisposable
	{
		/// <summary>
		/// Read the Cad header section of the file
		/// </summary>
		CadHeader ReadHeader();

		/// <summary>
		/// Read the cad document
		/// </summary>
		CadDocument Read();
	}
}
