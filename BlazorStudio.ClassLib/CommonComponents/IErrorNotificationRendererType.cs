﻿namespace BlazorStudio.ClassLib.CommonComponents;

public interface IErrorNotificationRendererType
{
    public const string CSS_CLASS_STRING = "bcrl_error";
    public string Message { get; set; }
}