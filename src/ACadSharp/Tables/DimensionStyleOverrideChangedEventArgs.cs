using System;

namespace ACadSharp.Tables;

public class DimensionStyleOverrideChangedEventArgs : EventArgs
{
    public DimensionStyleOverrideType Key { get; }
    public DimensionStyleOverride Override { get; }

    public DimensionStyleOverrideChangedEventArgs(
        DimensionStyleOverrideType key,
        DimensionStyleOverride @override)
    {
        Key = key;
        Override = @override;
    }
}