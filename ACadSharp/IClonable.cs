namespace ACadSharp
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICloneable<T> : System.ICloneable
	{
		T CloneT();
	}
}
