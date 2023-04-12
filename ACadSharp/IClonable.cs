namespace ACadSharp
{
	public interface ICloneable<T> : System.ICloneable
	{
		new T Clone();
	}
}
