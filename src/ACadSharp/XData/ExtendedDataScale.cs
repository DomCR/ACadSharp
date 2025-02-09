namespace ACadSharp.XData
{
	public class ExtendedDataScale : ExtendedDataRecord<double>
	{
		public ExtendedDataScale(double value) : base(DxfCode.ExtendedDataScale, value) { }
	}
}
