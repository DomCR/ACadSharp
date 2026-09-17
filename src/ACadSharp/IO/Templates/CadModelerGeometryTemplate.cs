using ACadSharp.Entities;
using System.Text;

namespace ACadSharp.IO.Templates
{
	/// <summary>
	/// Collects the SAT text chunks of a modeler geometry entity while it is read.
	/// </summary>
	internal interface IAcisDataTemplate
	{
		/// <summary>
		/// Appends a chunk that starts a new SAT line (DXF group code 1).
		/// </summary>
		void AppendAcisLine(string chunk);

		/// <summary>
		/// Appends a chunk that continues the previous SAT line (DXF group code 3).
		/// </summary>
		void AppendAcisContinuation(string chunk);
	}

	internal class CadModelerGeometryTemplate<T> : CadEntityTemplate<T>, IAcisDataTemplate
		where T : ModelerGeometry, new()
	{
		private readonly StringBuilder _acisText = new();

		private bool? _isEncoded;

		public CadModelerGeometryTemplate() : base(new T())
		{
		}

		public CadModelerGeometryTemplate(T entity) : base(entity)
		{
		}

		/// <inheritdoc/>
		public void AppendAcisLine(string chunk)
		{
			if (this._acisText.Length > 0)
			{
				this._acisText.Append('\n');
			}

			this.append(chunk);
		}

		/// <inheritdoc/>
		public void AppendAcisContinuation(string chunk)
		{
			this.append(chunk);
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			//The ACDSDATA section (R2013+) may carry the payload for the same
			//entity; in that case the section data is applied after the build and
			//takes precedence over anything accumulated here.
			if (this._acisText.Length > 0 && this.CadObject is ModelerGeometry geometry && geometry.AcisData == null)
			{
				geometry.AcisData = Encoding.ASCII.GetBytes(this._acisText.ToString());
			}
		}

		private void append(string chunk)
		{
			if (string.IsNullOrEmpty(chunk))
			{
				return;
			}

			//Pre-2013 writers store the SAT text with the documented character
			//swap; the payload is plain when it comes from other sources. Decide
			//once, on the first chunk: plain SAT always starts with the numeric
			//header line.
			if (!this._isEncoded.HasValue)
			{
				this._isEncoded = AcisTextCodec.IsEncoded(chunk);
			}

			this._acisText.Append(this._isEncoded.Value ? AcisTextCodec.Decode(chunk) : chunk);
		}
	}
}
