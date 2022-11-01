﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.Store.WorkspaceCase;
using Fluxor;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.AspNetCore.Components;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace BlazorStudio.ClassLib.Store.SolutionExplorer;

[FeatureState]
public record SolutionExplorerState(
    IAbsoluteFilePath? SolutionAbsoluteFilePath,
    Solution? Solution,
    bool PerformingAsynchronousOperation)
{
    public SolutionExplorerState() : this(
        default, 
        default,
        false)
    {
        
    }
    
    public record RequestSetSolutionExplorerStateAction(
        IAbsoluteFilePath? SolutionAbsoluteFilePath);

    private class SolutionExplorerStateReducer
    {
        [ReducerMethod]
        public SolutionExplorerState ReduceSetSolutionExplorerStateAction(
            SolutionExplorerState previousSolutionExplorerState,
            SolutionExplorerStateEffects.SetSolutionExplorerStateAction setSolutionExplorerStateAction)
        {
            return previousSolutionExplorerState with
            {
                SolutionAbsoluteFilePath = 
                    setSolutionExplorerStateAction.SolutionAbsoluteFilePath,
                Solution = 
                    setSolutionExplorerStateAction.Solution,
                PerformingAsynchronousOperation = setSolutionExplorerStateAction
                    .PerformingAsynchronousOperation ?? previousSolutionExplorerState
                    .PerformingAsynchronousOperation 
            };
        }
        
        [ReducerMethod]
        public SolutionExplorerState ReduceSetPerformingAsynchronousOperationAction(
            SolutionExplorerState previousSolutionExplorerState,
            SolutionExplorerStateEffects.SetPerformingAsynchronousOperationAction setPerformingAsynchronousOperationAction)
        {
            return previousSolutionExplorerState with
            {
                PerformingAsynchronousOperation = setPerformingAsynchronousOperationAction
                    .PerformingAsynchronousOperation,
            };
        }
    }
    
    private class SolutionExplorerStateEffects
    {
        private readonly IState<WorkspaceState> _workspaceStateWrap;

        public SolutionExplorerStateEffects(IState<WorkspaceState> workspaceStateWrap)
        {
            _workspaceStateWrap = workspaceStateWrap;
        }
        
        public record SetSolutionExplorerStateAction(
            IAbsoluteFilePath? SolutionAbsoluteFilePath,
            Solution? Solution,
            bool? PerformingAsynchronousOperation);
        
        public record SetPerformingAsynchronousOperationAction(
            bool PerformingAsynchronousOperation);

        [EffectMethod]
        public async Task HandleSetSolutionExplorerStateAction(
            RequestSetSolutionExplorerStateAction requestSetSolutionExplorerStateAction,
            IDispatcher dispatcher)
        {
            dispatcher.Dispatch(
                new SetPerformingAsynchronousOperationAction(true));
            
            if (_workspaceStateWrap.Value.Workspace is null)
            {
                dispatcher.Dispatch(new SetWorkspaceStateAction());
            }
            
            var solution = await ((MSBuildWorkspace)_workspaceStateWrap.Value.Workspace)
                .OpenSolutionAsync(requestSetSolutionExplorerStateAction
                    .SolutionAbsoluteFilePath
                    .GetAbsoluteFilePathString());
            
            dispatcher.Dispatch(
                new SetSolutionExplorerStateAction(
                    requestSetSolutionExplorerStateAction
                        .SolutionAbsoluteFilePath,
                    solution,
                    false));
        }
    }
    
    public static Task ShowInputFileAsync(
        IDispatcher dispatcher)
    {
        dispatcher.Dispatch(
            new InputFileState.RequestInputFileStateFormAction(
                "SolutionExplorer",
                afp =>
                {
                    // Without Task.Run blocks UI thread I believe
                    Task.Run(() =>
                    {
                        dispatcher.Dispatch(
                            new SolutionExplorerState.RequestSetSolutionExplorerStateAction(
                                afp));
                    });
                    
                    return Task.CompletedTask;
                },
                afp =>
                {
                    if (afp is null ||
                        afp.ExtensionNoPeriod != ExtensionNoPeriodFacts.DOT_NET_SOLUTION)
                    {
                        return Task.FromResult(false);
                    }
                    
                    return Task.FromResult(true);
                },
                new []
                {
                    new InputFilePattern(
                        ".NET Solution",
                        afp => 
                            afp.ExtensionNoPeriod == ExtensionNoPeriodFacts.DOT_NET_SOLUTION)
                }.ToImmutableArray()));
        
        return Task.CompletedTask;
    }
}