#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public class CadSummaryInfo
	{
		public string Title { get; set; }
		public string Subject { get; set; }
		public string Author { get; set; }
		public string Keywords { get; set; }
		public string Comments { get; set; }
		public string LastSavedBy { get; set; }
		public string RevisionNumber { get; set; }
		public string HyperlinkBase { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
	}
}
