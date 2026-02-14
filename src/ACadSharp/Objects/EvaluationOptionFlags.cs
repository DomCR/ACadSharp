namespace ACadSharp.Objects;

[System.Flags]
public enum EvaluationOptionFlags
{
	Never = 0,
	OnOpen = 1,
	OnSave = 2,
	OnPlot = 4,
	OnPackedETransmit = 8,
	OnRegeneration = 0x10,
	OnDemand = 0x20
}
