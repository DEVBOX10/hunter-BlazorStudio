using BlazorStudio.ClassLib.Keyboard;

namespace BlazorStudio.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    private partial class StateMachine
    {
        public static async Task<PlainTextEditorRecordBase> HandleDefaultInsertAsync(PlainTextEditorRecordBase focusedPlainTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord,
            CancellationToken cancellationToken)
        {
            if (focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Default)
            {
                // if (active token is a word)

                var previousDefaultToken = focusedPlainTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

                var content = previousDefaultToken.Content
                    .Insert(previousDefaultToken.IndexInPlainText!.Value + 1, keyDownEventRecord.Key);

                if (!keyDownEventRecord.IsForced && 
                    focusedPlainTextEditorRecord is PlainTextEditorRecordMemoryMappedFile editorMemoryMappedFile)
                {
                    var characterIndex = await CalculateCurrentTokenStartingCharacterIndexRespectiveToRowAsync(focusedPlainTextEditorRecord,
                                             cancellationToken)
                                         + previousDefaultToken.IndexInPlainText.Value;

                    await editorMemoryMappedFile.FileHandle.Edit
                        .InsertAsync(focusedPlainTextEditorRecord.CurrentRowIndex,
                            characterIndex,
                            keyDownEventRecord.Key,
                            cancellationToken);
                }

                var nextDefaultToken = previousDefaultToken with
                {
                    Content = content,
                    IndexInPlainText = previousDefaultToken.IndexInPlainText + 1
                };

                focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
                {
                    CurrentCharacterColumnIndex = focusedPlainTextEditorRecord.CurrentCharacterColumnIndex + 1,
                    CurrentPositionIndex = focusedPlainTextEditorRecord.CurrentPositionIndex + 1
                };

                if (!keyDownEventRecord.IsForced)
                {
                    for (int i = focusedPlainTextEditorRecord.CurrentRowIndex + 1; i < focusedPlainTextEditorRecord.FileHandle.VirtualCharacterIndexMarkerForStartOfARow.Count; i++)
                    {
                        focusedPlainTextEditorRecord.FileHandle.VirtualCharacterIndexMarkerForStartOfARow[i] += 1;
                    }
                }

                return await ReplaceCurrentTokenWithAsync(focusedPlainTextEditorRecord, 
                    nextDefaultToken,
                    cancellationToken);
            }
            else
            {
                var nextTokenTuple = await GetNextTokenTupleAsync(focusedPlainTextEditorRecord,
                    cancellationToken);

                if (nextTokenTuple.rowIndex == focusedPlainTextEditorRecord.CurrentRowIndex &&
                    nextTokenTuple.token.Kind == TextTokenKind.Default)
                {
                    // if (active token is not a word, and the next token is a word however then prepend text to that next token)

                    if (!keyDownEventRecord.IsForced &&
                        focusedPlainTextEditorRecord is PlainTextEditorRecordMemoryMappedFile editorMemoryMappedFile)
                    {
                        var characterIndex = await CalculateCurrentTokenStartingCharacterIndexRespectiveToRowAsync(focusedPlainTextEditorRecord,
                            cancellationToken);

                        if (focusedPlainTextEditorRecord.CurrentTextToken is not WhitespaceTextToken whitespace ||
                            whitespace.WhitespaceKind != WhitespaceKind.Tab)
                        {
                            characterIndex += focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText.Value;
                        }

                        await editorMemoryMappedFile.FileHandle.Edit
                            .InsertAsync(focusedPlainTextEditorRecord.CurrentRowIndex,
                                characterIndex,
                                keyDownEventRecord.Key,
                                cancellationToken);
                    }

                    focusedPlainTextEditorRecord = await SetNextTokenAsCurrentAsync(focusedPlainTextEditorRecord,
                        cancellationToken);
                    
                    var previousDefaultToken = focusedPlainTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

                    var content = previousDefaultToken.Content
                        .Insert(0, keyDownEventRecord.Key);

                    var nextDefaultToken = previousDefaultToken with
                    {
                        Content = content,
                        IndexInPlainText = previousDefaultToken.IndexInPlainText
                    };

                    focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
                    {
                        CurrentCharacterColumnIndex = focusedPlainTextEditorRecord.CurrentCharacterColumnIndex + 1,
                        CurrentPositionIndex = focusedPlainTextEditorRecord.CurrentPositionIndex + 1
                    };

                    return await ReplaceCurrentTokenWithAsync(focusedPlainTextEditorRecord, 
                        nextDefaultToken,
                        cancellationToken);
                }
                else
                {
                    var rememberToken = focusedPlainTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>();

                    if (rememberToken.IndexInPlainText!.Value != rememberToken.PlainText.Length - 1)
                    {
                        // if (active token is not a word, but the cursor is NOT at the end of that token the token is split)
                        
                        return await SplitCurrentTokenAsync(
                            focusedPlainTextEditorRecord, 
                            new DefaultTextToken
                            {
                                Content = keyDownEventRecord.Key,
                                IndexInPlainText = 0
                            },
                            keyDownEventRecord.IsForced,
                            cancellationToken
                        );
                    }
                    else
                    {
                        // if (active token is not a word, and the cursor is at the end of that token then insert a new 'word token' after the active one)

                        if (!keyDownEventRecord.IsForced &&
                            focusedPlainTextEditorRecord is PlainTextEditorRecordMemoryMappedFile editorMemoryMappedFile)
                        {
                            var characterIndex = await CalculateCurrentTokenStartingCharacterIndexRespectiveToRowAsync(focusedPlainTextEditorRecord,
                                                     cancellationToken)
                                                 + focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText.Value;

                            await editorMemoryMappedFile.FileHandle.Edit
                                .InsertAsync(focusedPlainTextEditorRecord.CurrentRowIndex,
                                    characterIndex,
                                    keyDownEventRecord.Key,
                                    cancellationToken);
                        }

                        var replacementCurrentToken = focusedPlainTextEditorRecord
                            .GetCurrentTextTokenAs<TextTokenBase>() with
                            {
                                IndexInPlainText = null
                            };

                        focusedPlainTextEditorRecord = await ReplaceCurrentTokenWithAsync(focusedPlainTextEditorRecord, 
                            replacementCurrentToken,
                            cancellationToken);

                        var defaultTextToken = new DefaultTextToken
                        {
                            Content = keyDownEventRecord.Key,
                            IndexInPlainText = 0
                        };

                        focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
                        {
                            CurrentCharacterColumnIndex = focusedPlainTextEditorRecord.CurrentCharacterColumnIndex + 1,
                            CurrentPositionIndex = focusedPlainTextEditorRecord.CurrentPositionIndex + 1
                        };

                        return await InsertNewCurrentTokenAfterCurrentPositionAsync(focusedPlainTextEditorRecord,
                            defaultTextToken,
                            cancellationToken);
                    }
                }
                
            }
        }

        public static async Task<PlainTextEditorRecordBase> HandleDefaultBackspaceAsync(PlainTextEditorRecordBase focusedPlainTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord,
            CancellationToken cancellationToken)
        {
            var previousDefaultTextToken = focusedPlainTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

            if (!keyDownEventRecord.IsForced &&
                focusedPlainTextEditorRecord is PlainTextEditorRecordMemoryMappedFile editorMemoryMappedFile)
            {
                var characterIndex = await CalculateCurrentTokenStartingCharacterIndexRespectiveToRowAsync(focusedPlainTextEditorRecord,
                                         cancellationToken)
                                     + focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText.Value;

                await editorMemoryMappedFile.FileHandle.Edit
                    .RemoveAsync(focusedPlainTextEditorRecord.CurrentRowIndex,
                        characterIndex - 1,
                        characterCount: 1,
                        cancellationToken: cancellationToken);
            }

            var firstSplitContent = previousDefaultTextToken.Content
                .Substring(0, previousDefaultTextToken.IndexInPlainText!.Value);

            var secondSplitContent = string.Empty;

            if (previousDefaultTextToken.IndexInPlainText != previousDefaultTextToken.Content.Length - 1)
            {
                secondSplitContent = previousDefaultTextToken.Content
                    .Substring(previousDefaultTextToken.IndexInPlainText!.Value + 1);
            }

            var nextDefaultToken = previousDefaultTextToken with
                {
                    Content = firstSplitContent + secondSplitContent,
                    IndexInPlainText = previousDefaultTextToken.IndexInPlainText - 1
                };

            if (nextDefaultToken.Content.Length == 0)
                return await RemoveCurrentTokenAsync(focusedPlainTextEditorRecord,
                    cancellationToken);

            focusedPlainTextEditorRecord = await ReplaceCurrentTokenWithAsync(focusedPlainTextEditorRecord, 
                nextDefaultToken,
                cancellationToken);

            if (nextDefaultToken.IndexInPlainText == -1)
                return await SetPreviousTokenAsCurrentAsync(focusedPlainTextEditorRecord,
                    cancellationToken);

            return focusedPlainTextEditorRecord;
        }
    }
}
