using ACadSharp.Attributes;
using ACadSharp.Classes;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects.Evaluations;

/// <summary>
/// Represents an evaluation graph containing a list of <see cref="Node"/>.
/// objects.
/// </summary>
[DxfName(DxfFileToken.ObjectEvalGraph)]
[DxfSubClass(DxfSubclassMarker.EvalGraph)]
public partial class EvaluationGraph : NonGraphicalObject, IDxfClassDefined
{
	/// <summary>
	/// Gets a list of <see cref="Edge"/> objects.
	/// </summary>
	public IList<Edge> Edges { get; private set; } = new List<Edge>();

	/// <summary>
	/// Gets a list of <see cref="Node"/> objects.
	/// </summary>
	public IEnumerable<Node> Nodes { get { return _nodes; } }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectEvalGraph;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.EvalGraph;

	[DxfCodeValue(96)]
	public int Value96 { get; set; }

	[DxfCodeValue(97)]
	public int Value97 { get; set; }

	/// <summary>
	/// Dictionary entry name for the object <see cref="EvaluationGraph"/>
	/// </summary>
	public const string DictionaryEntryName = "ACAD_ENHANCEDBLOCK";

	private List<Node> _nodes = new List<Node>();

	/// <summary>
	/// Initializes a new instance of the EvaluationGraph class.
	/// </summary>
	public EvaluationGraph()
	{ }

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		EvaluationGraph clone = (EvaluationGraph)base.Clone();

		clone._nodes = new List<Node>();
		foreach (var item in this._nodes)
		{
			clone._nodes.Add((Node)item.Clone());
		}

		clone.Edges = new List<Edge>();
		foreach (var item in this.Edges)
		{
			clone.Edges.Add((Edge)item.Clone());
		}

		return clone;
	}

	/// <summary>
	/// Creates a new <see cref="Node"/> object and adds it to the list of nodes.
	/// </summary>
	/// <returns>The newly created <see cref="Node"/> object.</returns>
	public Node CreateNode()
	{
		Node node = new Node(this);

		this._nodes.Add(node);

		return node;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.EvalGraph,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectEvalGraph,
			ItemClassId = 499,
			MaintenanceVersion = 55,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}

	/// <summary>
	/// Removes a <see cref="Node"/> object from the list of nodes by its ID.
	/// </summary>
	/// <param name="id">The ID of the <see cref="Node"/> to remove.</param>
	/// <returns>True if the node was found and removed; otherwise, false.</returns>
	public bool RemoveNode(int id)
	{
		Node node = this._nodes.FirstOrDefault(n => n.Id == id);
		if (node != null)
		{
			this._nodes.Remove(node);
			return true;
		}

		return false;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		foreach (var item in this._nodes.Where(n => n.Expression != null))
		{
			doc.AddCadObject(item.Expression);
		}
	}

	internal override void UnassignDocument()
	{
		foreach (var item in this._nodes.Where(n => n.Expression != null))
		{
			this.Document.RemoveCadObject(item.Expression);
		}

		base.UnassignDocument();
	}
}