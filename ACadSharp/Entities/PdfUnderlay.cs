namespace ACadSharp.Entities
{
	public class PdfUnderlay : UnderlayEntity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityPdfUnderlay;
	}
}
