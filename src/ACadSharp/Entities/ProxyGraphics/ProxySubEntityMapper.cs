namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubEntityMapper : IProxyGeometry
{
	public GraphicsType GraphicsType { get { return GraphicsType.SubEntityMapper; } }

	public int DummyValue1 { get; set; }
	public int DummyValue2 { get; set; }
	public int Projection { get; set; }
	public int UTiling { get; set; }
	public int VTiling { get; set; }
	public int AutoTransform { get; set; }
	public int DummyValue3 { get; set; }
}
