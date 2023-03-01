﻿using System.Collections.Immutable;
using BlazorCommon.RazorLib.Dialog;
using BlazorCommon.RazorLib.Store.DialogCase;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Classes.FilePath;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.SolutionExplorer;
using BlazorStudio.ClassLib.Store.TerminalCase;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.DotNetSolutionForm;

public partial class DotNetSolutionFormDisplay : FluxorComponent
{
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    [CascadingParameter]
    public DialogRecord DialogRecord { get; set; } = null!;

    private readonly TerminalCommandKey _newDotNetSolutionTerminalCommandKey = 
        TerminalCommandKey.NewTerminalCommandKey();

    private readonly CancellationTokenSource _newDotNetSolutionCancellationTokenSource = new();
    
    private string _solutionName = string.Empty;
    private string _parentDirectoryName = string.Empty;

    private string SolutionName => string.IsNullOrWhiteSpace(_solutionName)
        ? "{enter solution name}"
        : _solutionName;
    
    private string ParentDirectoryName => string.IsNullOrWhiteSpace(_parentDirectoryName)
        ? "{enter parent directory name}"
        : _parentDirectoryName;

    private string InterpolatedCommand => 
        DotNetCliFacts.FormatDotnetNewSln(_solutionName);

    private void RequestInputFileForParentDirectory()
    {
        Dispatcher.Dispatch(
            new InputFileState.RequestInputFileStateFormAction(
                "Directory for new .NET Solution",
                afp =>
                {
                    if (afp is null)
                        return Task.CompletedTask;
                    
                    _parentDirectoryName = afp.GetAbsoluteFilePathString();
                    
                    InvokeAsync(StateHasChanged);

                    return Task.CompletedTask;
                },
                afp =>
                {
                    if (afp is null ||
                        !afp.IsDirectory)
                    {
                        return Task.FromResult(false);
                    }
                    
                    return Task.FromResult(true);
                },
                new []
                {
                    new InputFilePattern(
                        "Directory",
                        afp => afp.IsDirectory)
                }.ToImmutableArray()));
    }
    
    private async Task StartNewDotNetSolutionCommandOnClick()
    {
        var interpolatedCommand = InterpolatedCommand;
        var localSolutionName = _solutionName;
        var localParentDirectoryName = _parentDirectoryName;

        if (string.IsNullOrWhiteSpace(localSolutionName) ||
            string.IsNullOrWhiteSpace(localParentDirectoryName))
        {
            return;
        }
        
        var newDotNetSolutionCommand = new TerminalCommand(
            _newDotNetSolutionTerminalCommandKey,
            interpolatedCommand,
            _parentDirectoryName,
            _newDotNetSolutionCancellationTokenSource.Token, () =>
            {
                // ContinueWith -> Open the newly created solution

                // Close Dialog
                Dispatcher.Dispatch(
                    new DialogRecordsCollection.DisposeAction(
                        DialogRecord.DialogKey));

                localParentDirectoryName = FilePathHelper.StripEndingDirectorySeparatorIfExists(
                    localParentDirectoryName,
                    EnvironmentProvider);
                
                var parentDirectoryAbsoluteFilePath = new AbsoluteFilePath(
                    localParentDirectoryName, 
                    true,
                    EnvironmentProvider);

                var solutionAbsoluteFilePathString =
                    parentDirectoryAbsoluteFilePath.GetAbsoluteFilePathString() +
                    localSolutionName +
                    EnvironmentProvider.DirectorySeparatorChar +
                    localSolutionName +
                    '.' +
                    ExtensionNoPeriodFacts.DOT_NET_SOLUTION;

                var solutionAbsoluteFilePath = new AbsoluteFilePath(
                    solutionAbsoluteFilePathString, 
                    false,
                    EnvironmentProvider);

                Dispatcher.Dispatch(
                    new SolutionExplorerState.RequestSetSolutionExplorerStateAction(
                        solutionAbsoluteFilePath,
                        EnvironmentProvider));
                return Task.CompletedTask;
            });
        
        var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];
        
        await generalTerminalSession
            .EnqueueCommandAsync(newDotNetSolutionCommand);
    }
}