using System.Collections.Immutable;
using BlazorStudio.ClassLib.CommandLine;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.Store.DialogCase;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.SolutionExplorer;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.RazorLib.SolutionExplorer;
using BlazorTreeView.RazorLib;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.CSharpProjectForm;

public partial class CSharpProjectFormDisplay : FluxorComponent
{
    [Inject]
    private IState<TerminalSessionsState> TerminalSessionsStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;

    [CascadingParameter]
    public DialogRecord DialogRecord { get; set; } = null!;
    
    [Parameter]
    public NamespacePath? SolutionNamespacePath { get; set; }

    private readonly TerminalCommandKey _newCSharpProjectTerminalCommandKey =
        TerminalCommandKey.NewTerminalCommandKey();

    private readonly CancellationTokenSource _newCSharpProjectCancellationTokenSource = new();

    private string _projectTemplateName = string.Empty;
    private string _cSharpProjectName = string.Empty;
    private string _optionalParameters = string.Empty;
    private string _parentDirectoryName = string.Empty;

    private string ProjectTemplateName => string.IsNullOrWhiteSpace(_projectTemplateName)
        ? "{enter Template name}"
        : _projectTemplateName;
    
    private string CSharpProjectName => string.IsNullOrWhiteSpace(_cSharpProjectName)
        ? "{enter C# Project name}"
        : _cSharpProjectName;
    
    private string OptionalParameters => _optionalParameters;

    private string ParentDirectoryName => string.IsNullOrWhiteSpace(_parentDirectoryName)
        ? "{enter parent directory name}"
        : _parentDirectoryName;

    private string InterpolatedNewCSharpProjectCommand =>
        DotNetCliFacts.FormatDotnetNewCSharpProject(
            _projectTemplateName,
            _cSharpProjectName,
            _optionalParameters);
    
    private string InterpolatedAddExistingProjectToSolutionCommand =>
        DotNetCliFacts.AddExistingProjectToSolution(
            SolutionNamespacePath?.AbsoluteFilePath.GetAbsoluteFilePathString() 
                ?? string.Empty,
            $"{_cSharpProjectName}/{_cSharpProjectName}.csproj");

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
                new[]
                {
                    new InputFilePattern(
                        "Directory",
                        afp => afp.IsDirectory)
                }.ToImmutableArray()));
    }

    private async Task StartNewCSharpProjectCommandOnClick()
    {
        var localInterpolatedNewCSharpProjectCommand = InterpolatedNewCSharpProjectCommand;
        var localInterpolatedAddExistingProjectToSolutionCommand = InterpolatedAddExistingProjectToSolutionCommand;

        var localProjectTemplateName = _projectTemplateName;
        var localCSharpProjectName = _cSharpProjectName;
        var localOptionalParameters = _optionalParameters;
        var localParentDirectoryName = _parentDirectoryName;
        var localSolutionAbsoluteFilePath = SolutionNamespacePath;

        if (string.IsNullOrWhiteSpace(localProjectTemplateName) ||
            string.IsNullOrWhiteSpace(localCSharpProjectName) ||
            string.IsNullOrWhiteSpace(localParentDirectoryName))
        {
            return;
        }

        var newDotNetSolutionCommand = new TerminalCommand(
            _newCSharpProjectTerminalCommandKey,
            localInterpolatedNewCSharpProjectCommand,
            localParentDirectoryName,
            _newCSharpProjectCancellationTokenSource.Token);

        var generalTerminalSession = TerminalSessionsStateWrap.Value.TerminalSessionMap[
            TerminalSessionFacts.GENERAL_TERMINAL_SESSION_KEY];

        await generalTerminalSession
            .EnqueueCommandAsync(newDotNetSolutionCommand);

        if (localSolutionAbsoluteFilePath is not null)
        {
            var addExistingProjectToSolutionCommand = new TerminalCommand(
                _newCSharpProjectTerminalCommandKey,
                localInterpolatedAddExistingProjectToSolutionCommand,
                localParentDirectoryName,
                _newCSharpProjectCancellationTokenSource.Token,
                async () =>
                {
                    // Close Dialog
                    Dispatcher.Dispatch(
                        new DisposeDialogRecordAction(DialogRecord));

                    // Re-open the modified Solution
                    Dispatcher.Dispatch(
                        new SolutionExplorerState.RequestSetSolutionExplorerStateAction(
                            localSolutionAbsoluteFilePath.AbsoluteFilePath));
                });
            
            await generalTerminalSession
                .EnqueueCommandAsync(addExistingProjectToSolutionCommand);
        }
    }
}