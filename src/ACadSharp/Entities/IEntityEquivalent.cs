namespace ACadSharp.Entities
{
	public interface IEntityEquivalent<T>
		where T : Entity
	{
		T ToEntity();
	}

	public interface IPolylineEquivalent : IEntityEquivalent<Polyline3D>
	{
	}
}
