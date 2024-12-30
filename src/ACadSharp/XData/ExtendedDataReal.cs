namespace ACadSharp.XData
{
	public class ExtendedDataReal : ExtendedDataRecord<double>
	{
		public ExtendedDataReal(double value) : base(DxfCode.ExtendedDataReal, value) { }
	}
}
