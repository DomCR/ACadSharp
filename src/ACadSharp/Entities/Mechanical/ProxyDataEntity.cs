using ACadSharp.Entities.ProxyGraphics;

namespace ACadSharp.Entities.Mechanical;

public abstract class ProxyDataEntity : Entity
{
	public IProxyGraphic[] ProxyGraphics { get; set; }
}
