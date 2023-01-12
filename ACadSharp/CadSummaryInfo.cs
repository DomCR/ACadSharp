using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public class CadSummaryInfo
	{
		public string Title { get; set; } = string.Empty;

		public string Subject { get; set; } = string.Empty;

		public string Author { get; set; } = string.Empty;

		public string Keywords { get; set; } = string.Empty;

		public string Comments { get; set; } = string.Empty;

		public string LastSavedBy { get; set; } = string.Empty;

		public string RevisionNumber { get; set; } = string.Empty;

		public string HyperlinkBase { get; set; } = string.Empty;

		public DateTime CreatedDate { get; set; } = DateTime.Now;

		public DateTime ModifiedDate { get; set; } = DateTime.Now;

		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
	}
}
