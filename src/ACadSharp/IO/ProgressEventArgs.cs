using System;

namespace ACadSharp.IO;

/// <summary>
/// Represents the method that handles a progress update event.
/// </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">A ProgressEventArgs object that contains the event data.</param>
public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

/// <summary>
/// Provides data for a progress update event during a CAD file read operation.
/// </summary>
public class ProgressEventArgs : EventArgs
{
	/// <summary>
	/// Gets the current CAD object data being processed.
	/// </summary>
	public CadObjectData Current { get; }

	/// <summary>
	/// Gets the current stage of the read operation.
	/// </summary>
	public ReadStage Stage { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
	/// </summary>
	/// <param name="stage">The current stage of the read operation.</param>
	/// <param name="current">The current CAD object data being processed.</param>
	public ProgressEventArgs(ReadStage stage, CadObjectData current)
	{
		this.Stage = stage;
		this.Current = current;
	}
}