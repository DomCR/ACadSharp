namespace ACadSharp.Entities.ProxyGraphics;

public class ProxySubEntityMapper : IProxyGeometry
{
	public int AutoTransform { get; set; }

	public int DummyValue1 { get; set; }

	public int DummyValue2 { get; set; }

	public int DummyValue3 { get; set; }

	public GraphicsType GraphicsType { get { return GraphicsType.SubEntityMapper; } }

	public int Projection { get; set; }

	public int UTiling { get; set; }

	public int VTiling { get; set; }
}