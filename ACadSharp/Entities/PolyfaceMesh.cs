namespace ACadSharp.Entities
{
	public class PolyfaceMesh : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.POLYLINE_PFACE; } }

		public SeqendCollection<Vertex3D> Vertices { get; }

		public PolyfaceMesh()
		{
			this.Vertices = new SeqendCollection<Vertex3D>(this);
		}
	}
}
