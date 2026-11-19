using ACadSharp.Entities;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.Templates;

internal class CadModelerGeometryTemplate<T> : CadEntityTemplate<T>
	where T : ModelerGeometry, new()
{
	public List<ulong> MaterialHandles { get; } = new();

	public StringBuilder AcisText { get; } = new();

	public CadModelerGeometryTemplate() : base(new T())
	{
	}

	public CadModelerGeometryTemplate(T entity) : base(entity)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (this.AcisText.Length > 0 && this.CadObject is ModelerGeometry geometry)
		{
			geometry.SetAcisText(this.AcisText.ToString());
		}
	}
}