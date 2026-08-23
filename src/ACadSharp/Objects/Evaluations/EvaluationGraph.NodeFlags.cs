using System;

namespace ACadSharp.Objects.Evaluations;

public partial class EvaluationGraph
{
	[Flags]
	public enum NodeFlags
	{
		/// <summary>
		/// No flags are set.
		/// </summary>
		None = 0x00,

		/// <summary>
		/// This bit is used internally by the xref detach code to keep track of xrefs that have been visited, and can also be used for similar purposes by other callers.
		/// </summary>
		Visited = 0x01,

		/// <summary>
		/// This bit is used internally by the xref detach code to keep track of xrefs that have multiple references, and can also be used for similar purposes by other callers.
		/// </summary>
		OutsideRefed = 0x02,

		/// <summary>
		/// This bit is used to indicate that the node is selected.
		/// </summary>
		Selected = 0x04,

		/// <summary>
		/// This bit is used to indicate that the node is in a list.
		/// </summary>
		InList = 0x08,

		/// <summary>
		/// This bit is used to indicate that the node is in all lists.
		/// </summary>
		ListAll = 0x0E,

		/// <summary>
		/// This bit is used to indicate that the node is a first-level node.
		/// </summary>
		FirstLevel = 0x10,

		/// <summary>
		/// This bit is used to indicate that the node is an unreserved tree node.
		/// </summary>
		UnresTree = 0x20,

		/// <summary>
		/// All flags are set.
		/// </summary>
		All = 0x2F
	};
}