using System;
using System.Collections.Generic;

namespace ACadSharp
{
	/// <summary>
	/// Holds general info and metadata about a CAD document. Like title, author, and timestamps.
	/// </summary>
	public class CadSummaryInfo
	{
		/// <summary>
		/// Title of the document.
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// A short description or subject for for the document.
		/// </summary>
		public string Subject { get; set; } = string.Empty;

		/// <summary>
		/// Name of the person or organization that created the document.
		/// </summary>
		public string Author { get; set; } = string.Empty;

		/// <summary>
		/// Keywords to help categorize or search for the document.
		/// </summary>
		public string Keywords { get; set; } = string.Empty;

		/// <summary>
		/// Any notes or comments about the document.
		/// </summary>
		public string Comments { get; set; } = string.Empty;

		/// <summary>
		/// Name of the last person who saved the document.
		/// </summary>
		public string LastSavedBy { get; set; } = string.Empty;

		/// <summary>
		/// Revision number, useful for tracking changes or versions.
		/// </summary>
		public string RevisionNumber { get; set; } = string.Empty;

		/// <summary>
		/// Base URL for hyperlinks in the document.
		/// </summary>
		public string HyperlinkBase { get; set; } = string.Empty;

		/// <summary>
		/// When the document was first created.
		/// </summary>
		public DateTime CreatedDate { get; set; } = DateTime.Now;

		/// <summary>
		/// When the document was last modified.
		/// </summary>
		public DateTime ModifiedDate { get; set; } = DateTime.Now;

		/// <summary>
		/// Custom properties defined by the user or application.
		/// </summary>
		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
	}
}