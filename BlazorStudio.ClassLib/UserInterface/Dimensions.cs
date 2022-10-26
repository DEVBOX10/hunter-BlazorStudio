﻿using System.Text;

namespace BlazorStudio.ClassLib.UserInterface;

public class Dimensions
{
    public const char STYLE_STRING_DELIMITER = ';';
    public const char STYLE_STRING_WHITESPACE = ' ';

    public DimensionsPositionKind DimensionsPositionKind { get; set; } = new();
    public List<DimensionUnit> WidthCalc { get; init; } = new();
    public List<DimensionUnit> HeightCalc { get; init; } = new();
    public List<DimensionUnit> LeftCalc { get; init; } = new();
    public List<DimensionUnit> RightCalc { get; init; } = new();
    public List<DimensionUnit> TopCalc { get; init; } = new();
    public List<DimensionUnit> BottomCalc { get; init; } = new();

    public string DimensionsCssString => GetDimensionsString();

    private string GetDimensionsString()
    {
        var builder = new StringBuilder();

        builder.Append($"position: {DimensionsPositionKind.ToString().ToLower()};");

        DimensionUnitHelper.AppendToStyleString(builder, WidthCalc, "width");
        DimensionUnitHelper.AppendToStyleString(builder, HeightCalc, "height");
        DimensionUnitHelper.AppendToStyleString(builder, LeftCalc, "left");
        DimensionUnitHelper.AppendToStyleString(builder, RightCalc, "right");
        DimensionUnitHelper.AppendToStyleString(builder, TopCalc, "top");
        DimensionUnitHelper.AppendToStyleString(builder, BottomCalc, "bottom");

        return builder.ToString();
    }
}