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
}