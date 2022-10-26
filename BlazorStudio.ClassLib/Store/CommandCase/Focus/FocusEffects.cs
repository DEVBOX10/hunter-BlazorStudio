using System.Collections.Immutable;
using BlazorStudio.ClassLib.Contexts;
using BlazorStudio.ClassLib.Renderer;
using BlazorStudio.ClassLib.Store.ContextCase;
using BlazorStudio.ClassLib.Store.DialogCase;
using BlazorStudio.ClassLib.Store.FooterWindowCase;
using BlazorStudio.ClassLib.Store.NugetPackageManagerCase;
using BlazorStudio.ClassLib.Store.QuickSelectCase;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.CommandCase.Focus;

public class FocusEffects
{
    private readonly IState<ContextState> _contextStateWrap;
    private readonly IDefaultErrorRenderer _defaultErrorRenderer;
    private readonly IState<DialogStates> _dialogStatesWrap;
    private readonly IState<QuickSelectState> _quickSelectStateWrap;

    public FocusEffects(IState<ContextState> contextStateWrap,
        IState<DialogStates> dialogStatesWrap,
        IState<QuickSelectState> quickSelectStateWrap,
        IDefaultErrorRenderer defaultErrorRenderer)
    {
        _contextStateWrap = contextStateWrap;
        _dialogStatesWrap = dialogStatesWrap;
        _quickSelectStateWrap = quickSelectStateWrap;
        _defaultErrorRenderer = defaultErrorRenderer;
    }

    [EffectMethod(typeof(FocusMainLayoutAction))]
    public Task HandleFocusMainLayoutAction(IDispatcher dispatcher)
    {
        _contextStateWrap.Value.ContextRecords[ContextFacts.GlobalContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusFolderExplorerAction))]
    public Task HandleFocusFolderExplorerAction(IDispatcher dispatcher)
    {
        _contextStateWrap.Value.ContextRecords[ContextFacts.FolderExplorerContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusSolutionExplorerAction))]
    public Task HandleFocusSolutionExplorerAction(IDispatcher dispatcher)
    {
        _contextStateWrap.Value.ContextRecords[ContextFacts.SolutionExplorerContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusToolbarDisplayAction))]
    public Task HandleFocusToolbarDisplayAction(IDispatcher dispatcher)
    {
        _contextStateWrap.Value.ContextRecords[ContextFacts.ToolbarDisplayContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusEditorDisplayAction))]
    public Task HandleFocusEditorDisplayAction(IDispatcher dispatcher)
    {
        _contextStateWrap.Value.ContextRecords[ContextFacts.EditorDisplayContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusTerminalDisplayAction))]
    public Task HandleFocusTerminalDisplayAction(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetActiveFooterWindowKindAction(FooterWindowKind.Terminal));

        _contextStateWrap.Value.ContextRecords[ContextFacts.TerminalDisplayContext.ContextKey]
            .InvokeOnFocusRequestedEventHandler();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusDialogQuickSelectDisplayAction))]
    public Task HandleFocusDialogQuickSelectDisplayAction(IDispatcher dispatcher)
    {
        if (_quickSelectStateWrap.Value.IsDisplayed)
        {
            // var registerNotificationAction = new RegisterNotificationAction(new NotificationRecord(
            //     NotificationKey.NewNotificationKey(), 
            //     "ERROR: Quick Select was busy",
            //     _defaultErrorRenderer.GetType(),
            //     null));
            //
            // dispatcher.Dispatch(registerNotificationAction);

            return Task.CompletedTask;
        }

        var quickSelectItems = _dialogStatesWrap.Value.List
            .Select(x => (IQuickSelectItem)new QuickSelectItem<DialogRecord>(x.Title, x))
            .ToImmutableArray();

        var quickSelectState = new QuickSelectState
        {
            IsDisplayed = true,
            QuickSelectItems = quickSelectItems,
            OnItemSelectedFunc = dialogRecord =>
            {
                ((DialogRecord)dialogRecord.ItemNoType).InvokeOnFocusRequestedEventHandler();
                return Task.CompletedTask;
            },
            OnHoveredItemChangedFunc = item => Task.CompletedTask,
        };

        dispatcher.Dispatch(new SetQuickSelectStateAction(quickSelectState));
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(FocusNugetPackageManagerDisplayAction))]
    public Task HandleFocusNugetPackageManagerDisplayAction(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetActiveFooterWindowKindAction(FooterWindowKind.NugetPackageManager));
        dispatcher.Dispatch(new RequestFocusOnNugetPackageManagerAction());
        return Task.CompletedTask;
    }
}