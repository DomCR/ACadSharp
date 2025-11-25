using System;

namespace ACadSharp.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DimOverrideXDataAttribute : Attribute
{
    public short GroupCode { get; }
    public XDataValueKind Kind { get; }
    public string Description { get; }

    public DimOverrideXDataAttribute(short groupCode, XDataValueKind kind, string description)
    {
        GroupCode = groupCode;
        Kind = kind;
        Description = description;
    }
}

public enum XDataValueKind
{
    Short,
    Bool,
    Int16,
    String,
    Double,
    LineType,
    LineWeightType,
    Color,
    DimensionTextVerticalAlignment,
    DimensionTextHorizontalAlignment,
    BlockRecord,
    LinearUnitFormat,
    Char,
    TextMovement,
    ToleranceAlignment,
    ZeroHandling
}