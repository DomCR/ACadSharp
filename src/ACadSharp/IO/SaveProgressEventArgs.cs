using System;

namespace ACadSharp.IO;

/// <summary>
/// Delegate for <see cref="DwgWriter.OnSaveProgress"/> save progress notifications.
/// </summary>
public delegate void SaveProgressEventHandler(object sender, SaveProgressEventArgs e);

/// <summary>
/// Provides save progress data during a DWG write operation (object/entity serialization stage).
/// Emitted only when <see cref="DwgWriter.OnSaveProgress"/> has subscribers (off by default, zero overhead).
/// </summary>
public class SaveProgressEventArgs : EventArgs
{
	/// <summary>Number of in-block entities serialized.</summary>
	public int Completed { get; }

	/// <summary>Total number of in-block entities (progress denominator).</summary>
	public int Total { get; }

	/// <summary>Progress percentage 0~100.</summary>
	public double Percent => this.Total <= 0 ? 100.0 : (double)this.Completed / this.Total * 100.0;

	public SaveProgressEventArgs(int completed, int total)
	{
		this.Completed = completed;
		this.Total = total;
	}
}
