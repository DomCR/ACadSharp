namespace ACadSharp.XData
{
	public class ExtendedDataDistance : ExtendedDataRecord<double>
	{
		public ExtendedDataDistance(double value) : base(DxfCode.ExtendedDataDist, value) { }
	}
}
