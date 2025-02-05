using ACadSharp.Tables;

namespace ACadSharp.XData
{
	public class ExtendedDataLayer : ExtendedDataReference<Layer>
	{
		public ExtendedDataLayer(ulong handle) : base(DxfCode.ExtendedDataLayerName, handle) { }
	}
}
