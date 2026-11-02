using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadProxyEntityTemplate: CadEntityTemplate<ProxyEntity>
{
	public List<ulong> ReferencedObjectHandles { get; set; } = new List<ulong>();

	public CadProxyEntityTemplate() : base(new ProxyEntity())
	{
	}

	public CadProxyEntityTemplate(ProxyEntity entity) : base(entity)
	{
	}
} 