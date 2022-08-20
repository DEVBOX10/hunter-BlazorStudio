﻿using BlazorStudio.ClassLib.Mouse;
using BlazorStudio.ClassLib.Store.DragCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.Drag;

public partial class DragDisplay : FluxorComponent
{
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    private string StyleCss => DragStateWrap.Value.IsDisplayed
        ? string.Empty
        : "display: none;";

    private SetDragStateAction ConstructClearDragStateAction() => 
        new(false, null);

    private void DispatchNotifyMouseEventOnMouseMove(MouseEventArgs mouseEventArgs)
    {
        // Buttons is a bit flag
        // '& 1' gets if left mouse button is held 
        if ((mouseEventArgs.Buttons & 1) != 1)
        {
            Dispatcher.Dispatch(ConstructClearDragStateAction());
        }
        else
        {
            Dispatcher.Dispatch(new SetDragStateAction(true, mouseEventArgs));
        }
    }
    
    private void DispatchHideOnMouseUp()
    {
        Dispatcher.Dispatch(ConstructClearDragStateAction());
    }
}