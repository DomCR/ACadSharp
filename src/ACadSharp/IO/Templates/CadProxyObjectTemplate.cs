using ACadSharp.Entities;
using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadProxyObjectTemplate : CadTemplate<ProxyObject>
{
	public List<ulong> Entries { get; } = new();

	public CadProxyObjectTemplate() : base(new ProxyObject())
	{
	}

	public CadProxyObjectTemplate(ProxyObject obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (var entry in Entries)
		{
			if (builder.TryGetCadObject(entry, out CadObject obj))
			{

			}
		}
	}
}
