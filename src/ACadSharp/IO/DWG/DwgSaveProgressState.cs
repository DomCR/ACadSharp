using System;
using System.Threading;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// [PATCH] DWG save progress state: thread-safe counters + throttled triggering shared by the main instance and P2 parallel workers.
	/// Only created by the main instance and distributed to workers when <see cref="DwgWriter.OnSaveProgress"/> has subscribers;
	/// zero overhead when off by default (_saveProgress is null in writeEntity).
	/// </summary>
	/// <remarks>
	/// The progress denominator (Total) is the total count of compatible in-block entities counted by the main instance before writing block entities; the numerator (Completed)
	/// is incremented by each worker via <see cref="ReportEntity"/> when writeEntity actually serializes an entity
	/// (items skipped by skipClean are not counted). Throttling (<see cref="ReportInterval"/>) guarantees that with millions of entities
	/// the callback is not fired for every entity: it is triggered once for every ~ReportInterval entities serialized.
	/// </remarks>
	internal sealed class DwgSaveProgressState
	{
		private int _completed;

		private int _lastReported;

		/// <summary>Total number of in-block entities (progress denominator, preset by the main instance).</summary>
		public int Total { get; set; }

		/// <summary>Throttling interval: fire the callback once for every ~N entities serialized (default 5000).</summary>
		public int ReportInterval { get; set; } = 5000;

		/// <summary>Progress callback (injected by DwgWriter; may be invoked from any worker thread, so subscribers must be thread-safe).</summary>
		public SaveProgressEventHandler Callback { get; set; }

		/// <summary>Number of entities serialized (Interlocked-safe).</summary>
		public int Completed => this._completed;

		/// <summary>
		/// Reset the counters to zero (called when P0 dirty-tracking fails and falls back to full re-serialization, so progress does not keep accumulating from a partial count).
		/// </summary>
		public void Reset()
		{
			Interlocked.Exchange(ref this._completed, 0);
			Interlocked.Exchange(ref this._lastReported, 0);
		}

		/// <summary>Progress percentage 0~100.</summary>
		public double Percent => this.Total <= 0 ? 0.0 : (double)this._completed / this.Total * 100.0;

		/// <summary>
		/// Count one serialized entity and fire the callback per throttling. Thread-safe (P2 parallel workers invoke it concurrently).
		/// At the tail (Completed==Total) it is guaranteed to fire once, ensuring subscribers receive 100%.
		/// </summary>
		public void ReportEntity()
		{
			int completed = Interlocked.Increment(ref this._completed);
			var cb = this.Callback;
			if (cb == null || this.Total <= 0)
				return;

			bool isLast = completed >= this.Total;
			if (!isLast)
			{
				int last = Volatile.Read(ref this._lastReported);
				if (completed - last < this.ReportInterval)
					return;
				// Claim the throttle window (a failed CAS means another thread already fired, so this thread skips)
				if (Interlocked.CompareExchange(ref this._lastReported, completed, last) != last)
					return;
			}

			try
			{
				cb(this, new SaveProgressEventArgs(completed, this.Total));
			}
			catch
			{
				// A subscriber exception does not affect the save flow
			}
		}
	}
}
