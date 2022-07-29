﻿using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystemApi;

namespace BlazorStudio.Tests.FileHandleTests;

public class FILE_HANDLE_TESTS : PLAIN_TEXT_EDITOR_STATES_TESTS
{
    [Fact]
    public void READ()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var rowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var content = string.Join(string.Empty, rowContent);

        // TODO: Why does this Assertion not work (it fails and throws an exception)?
        Assert.Equal("import * as vscode from 'vscode';\r\nimport { ActiveDotNetSolutionFileContainer } from './ActiveDotNetSolutionFileContainer';\r\nimport { ConstantsFilePath } from './Constants/ConstantsFilePath';\r\nimport { FilePathStandardizer } from './FileSystem/FilePathStandardizer';\r\nimport { SolutionExplorerMessageTransporter } from './MessageHandlers/SolutionExplorer/SolutionExplorerMessageTransporter';",

content);
        
        fileHandle.Dispose();
    }
    
    [Fact]
    public void SINGLE_INSERT()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var firstRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Insert(0, 0, "'Hello World!' - FileHandle");
        
        var secondRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var secondContent = string.Join(string.Empty, secondRowContent);
        
        fileHandle.Dispose();
    }
    
    [Fact]
    public void SINGLE_REMOVE()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var firstRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Remove(0, 0, 1, characterLengthOfLongestRow);
        
        var secondRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var secondContent = string.Join(string.Empty, secondRowContent);
        
        fileHandle.Dispose();
    }
    
    [Fact]
    public void INSERT_LINE_THEN_REMOVE_INSERTED_LINE()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var firstRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Insert(0, 0, "'Hello World!' - FileHandle\r\n");
        
        var secondRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None); 
        
        var secondContent = string.Join(string.Empty, secondRowContent);
 
        fileHandle.Edit.Remove(0, 0, 1, characterLengthOfLongestRow);
        
        var thirdRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var thirdContent = string.Join(string.Empty, thirdRowContent);
        
        fileHandle.Dispose();
    }
    
    [Fact]
    public void INSERT_LINE_THEN_REMOVE_A_DIFFERENT_LINE()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        var firstRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Insert(0, 0, "'Hello World!' - FileHandle\r\n");
        
        var secondRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None); 
        
        var secondContent = string.Join(string.Empty, secondRowContent);
 
        fileHandle.Edit.Remove(1, 0, 1, characterLengthOfLongestRow);
        
        var thirdRowContent = fileHandle.Read(0, 0, 5, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var thirdContent = string.Join(string.Empty, thirdRowContent);
        
        fileHandle.Dispose();
    }
    
    /// <summary>
    /// Read in a specific line, insert a newline prior to the previously read line.
    /// Now read the same line number and ensure it changed.
    /// </summary>
    [Fact]
    public void READ_INSERT_READ_CHECK_CHANGED()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        // starting
        var startingEntireFileRowContent = fileHandle.Read(0, 0, fileHandle.PhysicalRowCount, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var startingEntireFileContent = string.Join(string.Empty, startingEntireFileRowContent);
        
        // first
        var firstRowContent = fileHandle.Read(1, 0, 1, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Insert(0, 0, "'Hello World!' - FileHandle\r\n");
        
        // second
        var secondRowContent = fileHandle.Read(1, 0, 1, characterLengthOfLongestRow, 
            CancellationToken.None); 
        
        var secondContent = string.Join(string.Empty, secondRowContent);
 
        // ending
        var endingEntireFileRowContent = fileHandle.Read(0, 0, fileHandle.PhysicalRowCount, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var endingEntireFileContent = string.Join(string.Empty, endingEntireFileRowContent);
        
        fileHandle.Dispose();
    }
    
    /// <summary>
    /// Read in a specific line, remove a line prior to the previously read line.
    /// Now read the same line number and ensure it changed.
    /// </summary>
    [Fact]
    public void READ_REMOVE_READ_CHECK_CHANGED()
    {
        var inputAbsoluteFilePath = new AbsoluteFilePath(
            "C:\\Users\\hunte\\source\\repos\\dot-net-ide\\src\\extension.ts",
            false);

        var fileSystemProvider = new FileSystemProvider();

        var fileHandle = fileSystemProvider.Open(inputAbsoluteFilePath);

        var characterLengthOfLongestRow = (Int32) Math.Min(Int32.MaxValue, fileHandle.PhysicalCharacterLengthOfLongestRow);
        
        // starting
        var startingEntireFileRowContent = fileHandle.Read(0, 0, fileHandle.PhysicalRowCount, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var startingEntireFileContent = string.Join(string.Empty, startingEntireFileRowContent);
        
        // first
        var firstRowContent = fileHandle.Read(1, 0, 1, characterLengthOfLongestRow, 
            CancellationToken.None);

        var firstContent = string.Join(string.Empty, firstRowContent);

        fileHandle.Edit.Remove(0, 0, rowCount: 1);
        
        // second
        var secondRowContent = fileHandle.Read(1, 0, 1, characterLengthOfLongestRow, 
            CancellationToken.None); 
        
        var secondContent = string.Join(string.Empty, secondRowContent);
 
        // ending
        var endingEntireFileRowContent = fileHandle.Read(0, 0, fileHandle.PhysicalRowCount, characterLengthOfLongestRow, 
            CancellationToken.None);
        
        var endingEntireFileContent = string.Join(string.Empty, endingEntireFileRowContent);
        
        fileHandle.Dispose();
    }
}