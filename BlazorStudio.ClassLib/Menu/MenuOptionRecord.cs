namespace BlazorStudio.ClassLib.Menu;

public record MenuOptionRecord(
    string DisplayName,
    Action? OnClick = null,
    MenuRecord? SubMenu = null,
    Type? WidgetRendererType = null,
    Dictionary<string, object?>? WidgetParameters = null)
{
    public Guid Id { get; } = Guid.NewGuid();
}