namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyLwPolyine : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.LwPolyine; } }
	public LwPolyline Entity { get; set; }
	public byte Unknown1 { get; set; }
	public byte Unknown2 { get; set; }
	public byte Unknown3 { get; set; }
}
