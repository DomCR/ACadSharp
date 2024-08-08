using System;

namespace ACadSharp.IO
{
	public interface ICadWriter : IDisposable
	{
		/// <summary>
		/// Write the <see cref="CadDocument"/>
		/// </summary>
		void Write();
	}
}
