using System;

namespace ACadSharp.IO
{
	public interface ICadWriter : IDisposable
	{
		/// <summary>
		/// Write the <see cref="CadDocument"/>
		/// </summary>
		/// <param name="validate">Validates the document to avoid issues when is being used by another software</param>
		/// <remarks>
		/// Validating the document may cause some changes in the different objects like dictionaries, handles, extended data or references.
		/// </remarks>
		void Write(bool validate);

		/// <summary>
		/// Write the <see cref="CadDocument"/>
		/// </summary>
		void Write();
	}
}
