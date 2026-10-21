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
	/// Gets the number of objects processed so far (completed).
	/// <para>[PATCH] Only incremented in the <see cref="ReadStage.Read"/> stage (once per object);
	/// the Build stage does not double-count, so it is on the same basis as the denominator <see cref="Total"/> (total objects in the file).</para>
	/// </summary>
	public int Completed { get; }

	/// <summary>
	/// Gets the total number of objects expected (denominator).
	/// <para>[PATCH] -1 means unknown (e.g. lazy/partial read, where the decoded set cannot be precisely known before reading);
	/// the consumer should then show an indeterminate progress indicator.</para>
	/// </summary>
	public int Total { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
	/// </summary>
	/// <param name="stage">The current stage of the read operation.</param>
	/// <param name="current">The current CAD object data being processed.</param>
	public ProgressEventArgs(ReadStage stage, CadObjectData current)
		: this(stage, current, 0, -1)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressEventArgs"/> class with progress totals.
	/// </summary>
	/// <param name="stage">The current stage of the read operation.</param>
	/// <param name="current">The current CAD object data being processed.</param>
	/// <param name="completed">Number of objects processed so far.</param>
	/// <param name="total">Total number of objects expected (-1 = unknown).</param>
	public ProgressEventArgs(ReadStage stage, CadObjectData current, int completed, int total)
	{
		this.Stage = stage;
		this.Current = current;
		this.Completed = completed;
		this.Total = total;
	}
}