namespace ACadSharp.Attributes
{
	public interface ICodeValueAttribute
	{
		/// <summary>
		/// Dxf codes binding the property
		/// </summary>
		public DxfCode[] ValueCodes { get; }

		/// <summary>
		/// Reference type for this dxf property
		/// </summary>
		public DxfReferenceType ReferenceType { get; }
	}
}
