using System.Collections.Immutable;
using BlazorStudio.ClassLib.Commands;
using BlazorStudio.ClassLib.Keyboard;
using BlazorStudio.ClassLib.Store.CommandCase.Focus;

namespace BlazorStudio.ClassLib.Contexts;

public static class ContextFacts
{
    public static readonly ContextRecord GlobalContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "Global",
        "global",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Global", "Test Global", () => Console.WriteLine("Test Global"))
            },
            {
                new KeyDownEventRecord("g", "KeyG", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Focus -> Main Layout", "set-focus_main-layout", new FocusMainLayoutAction())
            },
            {
                new KeyDownEventRecord("f", "KeyF", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Focus -> Folder Explorer", "set-focus_folder-explorer", new FocusFolderExplorerAction())
            },
            {
                new KeyDownEventRecord("s", "KeyS", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Focus -> Solution Explorer", "set-focus_solution-explorer", new FocusSolutionExplorerAction())
            },
            {
                new KeyDownEventRecord("d", "KeyD", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Focus -> Dialog Display", "set-focus_dialog-display", new FocusDialogDisplayAction())
            },
            {
                new KeyDownEventRecord("t", "KeyT", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Focus -> Toolbar Display", "set-focus_toolbar-display", new FocusToolbarDisplayAction())
            },
        }.ToImmutableDictionary()));
    
    public static readonly ContextRecord PlainTextEditorContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "PlainTextEditor",
        "plain-text-editor",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Plain Text Editor", "Test Plain Text Editor", () => Console.WriteLine("Test Plain Text Editor"))
            }
        }.ToImmutableDictionary()));
    
    public static readonly ContextRecord SolutionExplorerContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "SolutionExplorer",
        "solution-explorer",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Solution Explorer", "Test Solution Explorer", () => Console.WriteLine("Test Solution Explorer"))
            }
        }.ToImmutableDictionary()));
    
    public static readonly ContextRecord FolderExplorerContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "FolderExplorer",
        "folder-explorer",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Folder Explorer", "Test Folder Explorer", () => Console.WriteLine("Test Folder Explorer"))
            }
        }.ToImmutableDictionary()));
    
    public static readonly ContextRecord DialogDisplayContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "DialogDisplay",
        "dialog-display",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Dialog Display", "Test Dialog Display", () => Console.WriteLine("Test Dialog Display"))
            }
        }.ToImmutableDictionary()));
    
    public static readonly ContextRecord ToolbarDisplayContext = new ContextRecord(
        ContextKey.NewContextKey(), 
        "ToolbarDisplay",
        "toolbar-display",
        new Keymap(new Dictionary<KeyDownEventRecord, CommandRecord>
        {
            {
                new KeyDownEventRecord("a", "KeyA", false, false, true),
                new CommandRecord(CommandKey.NewCommandKey(), "Test Toolbar Display", "Test Toolbar Display", () => Console.WriteLine("Test Toolbar Display"))
            }
        }.ToImmutableDictionary()));
}