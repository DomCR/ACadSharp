namespace ACadSharp.Extensions
{
	public static class CadObjectExtensions
	{
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <remarks>
		/// The copy will be unattached from the document or any reference.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns>A new object that is a copy of this instance.</returns>
		public static T CloneTyped<T>(this T obj)
			where T : CadObject
		{
			return (T)obj.Clone();
		}
	}
}
