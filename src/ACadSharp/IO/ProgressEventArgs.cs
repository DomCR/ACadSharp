using System;

namespace ACadSharp.IO;

public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

public class ProgressEventArgs : EventArgs
{
	public ReadStage Stage { get; }

	public CadObjectData Current { get; }

	public ProgressEventArgs(ReadStage stage, CadObjectData current)
	{
		this.Stage = stage;
		this.Current = current;
	}
}
