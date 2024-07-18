using ACadSharp.Header;
using System;

namespace ACadSharp.IO
{
	/// <summary>
	/// Common interface for the different Cad readers.
	/// </summary>
	public interface ICadReader : IDisposable
	{
		/// <summary>
		/// Read the Cad header section of the file.
		/// </summary>
		CadHeader ReadHeader();

		/// <summary>
		/// Read the cad document.
		/// </summary>
		CadDocument Read();
	}
}
