using System.Collections.Immutable;
using System.Text;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.Keyboard;
using BlazorStudio.ClassLib.Sequence;
using BlazorStudio.ClassLib.Store.KeyDownEventCase;
using BlazorStudio.ClassLib.Virtualize;

namespace BlazorStudio.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    private record PlainTextEditorRecord(PlainTextEditorKey PlainTextEditorKey,
            SequenceKey SequenceKey,
            ImmutableList<IPlainTextEditorRow> List,
            int CurrentRowIndex,
            int CurrentTokenIndex,
            IFileCoordinateGrid? FileCoordinateGrid,
            RichTextEditorOptions RichTextEditorOptions,
            bool IsReadonly = true,
            bool UseCarriageReturnNewLine = false)
        : IPlainTextEditor
    {
        public PlainTextEditorRecord(PlainTextEditorKey plainTextEditorKey) : this(plainTextEditorKey,
            SequenceKey.NewSequenceKey(),
            ImmutableList<IPlainTextEditorRow>.Empty,
            CurrentRowIndex: 0,
            CurrentTokenIndex: 0,
            null,
            new RichTextEditorOptions())
        {
            List = List.Add(GetEmptyPlainTextEditorRow());
        }

        public IPlainTextEditorRow CurrentPlainTextEditorRow => List[CurrentRowIndex];

        public TextTokenKey CurrentTextTokenKey => CurrentPlainTextEditorRow.List[CurrentTokenIndex].Key;
        public ITextToken CurrentTextToken => CurrentPlainTextEditorRow.List[CurrentTokenIndex];
        public int LongestRowCharacterLength { get; init; }
        public VirtualizeCoordinateSystemMessage VirtualizeCoordinateSystemMessage { get; init; }
        public int RowIndexOffset { get; init; }
        public int CharacterIndexOffsetRelativeToRow { get; init; }
        public List<PlainTextEditorChunk> Cache { get; init; }
        
        public T GetCurrentTextTokenAs<T>()
            where T : class
        {
            return CurrentTextToken as T
                   ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }

        /// <summary>
        /// TODO: Remove this and in its place use <see cref="ConvertIPlainTextEditorRowAs"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public T GetCurrentPlainTextEditorRowAs<T>()
            where T : class
        {
            return CurrentPlainTextEditorRow as T
                   ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }

        public T ConvertIPlainTextEditorRowAs<T>(IPlainTextEditorRow plainTextEditorRow)
            where T : class
        {
            return plainTextEditorRow as T
                   ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }

        public string GetPlainText()
        {
            var builder = new StringBuilder();

            foreach (var row in List)
            {
                foreach (var token in row.List)
                {
                    if (token.Key == List[0].List[0].Key)
                    {
                        // Is first start of row so skip
                        // as it would insert a enter key stroke at start
                        // of document otherwise.

                        continue;
                    }

                    builder.Append(token.CopyText);
                }
            }

            return builder.ToString();
        }
        
        public IPlainTextEditorRow GetEmptyPlainTextEditorRow()
        {
            return new PlainTextEditorRow(null);
        }

        public List<(int index, IPlainTextEditorRow row)> UpdateCache(FileCoordinateGridRequest fileCoordinateGridRequest)
        {
            int lastIndexOfOverlap = -1;
            
            for (var index = 0; index < Cache.Count; index++)
            {
                var cachedChunk = Cache[index];

                // Search chunk for any overlapping characters.
                // Overlapping characters will EXTEND that overlapping chunk
                if (cachedChunk.OverlapsRequest(fileCoordinateGridRequest,
                        out var chunk))
                {
                    Cache[index] = chunk;

                    // In the case that the request is 'sandwiched' between
                    // between two chunks AND overlaps both sandwiching chunks
                    // one cannot return immediately and must allow all overlaps to merge.
                    lastIndexOfOverlap = index;
                }
            }

            PlainTextEditorChunk resultingChunk;

            if (lastIndexOfOverlap >= 0)
            {
                // An existing chunk was overlapping the request

                resultingChunk = Cache[lastIndexOfOverlap];
            }
            else
            {
                // If there are no chunks that overlap then a NEW chunk is made
                var content = FileCoordinateGrid
                    .Request(fileCoordinateGridRequest);

                var constructedPlainTextEditor = this with
                {
                    CurrentRowIndex = 0,
                    CurrentTokenIndex = 0,
                    SequenceKey = SequenceKey.NewSequenceKey(),
                    List = ImmutableList<IPlainTextEditorRow>.Empty
                        .Add(GetEmptyPlainTextEditorRow())
                };

                resultingChunk = ConstructChunk(constructedPlainTextEditor,
                    content,
                    fileCoordinateGridRequest);

                constructedPlainTextEditor.Cache.Add(resultingChunk);
            }

            return resultingChunk.PlainTextEditorRecord.List
                .Select((row, index) => (index, row))
                .ToList();
        }

        private static PlainTextEditorChunk ConstructChunk(PlainTextEditorRecord constructedPlainTextEditor,
            List<string> content,
            FileCoordinateGridRequest fileCoordinateGridRequest)
        {
            foreach (var row in content)
            {
                foreach (var character in row)
                {
                    if (character == '\r')
                    {
                        previousCharacterWasCarriageReturn = true;
                        continue;
                    }

                    currentRowCharacterLength++;

                    var code = character switch
                    {
                        '\t' => KeyboardKeyFacts.WhitespaceKeys.TAB_CODE,
                        ' ' => KeyboardKeyFacts.WhitespaceKeys.SPACE_CODE,
                        '\n' => MutateIfPreviousCharacterWasCarriageReturn(),
                        _ => character.ToString()
                    };

                    var keyDown = new KeyDownEventAction(plainTextEditorRecord.PlainTextEditorKey,
                        new KeyDownEventRecord(
                            character.ToString(),
                            code,
                            false,
                            false,
                            false
                        )
                    );

                    plainTextEditorRecord = PlainTextEditorStates.StateMachine
                            .HandleKeyDownEvent(plainTextEditorRecord, keyDown.KeyDownEventRecord) with
                    {
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                    previousCharacterWasCarriageReturn = false;
                }

                if (row.LastOrDefault() != '\n')
                {
                    var forceNewLine = new KeyDownEventRecord(
                        KeyboardKeyFacts.NewLineCodes.ENTER_CODE,
                        KeyboardKeyFacts.NewLineCodes.ENTER_CODE,
                        false,
                        false,
                        false);

                    plainTextEditorRecord = PlainTextEditorStates.StateMachine
                        .HandleKeyDownEvent(plainTextEditorRecord, forceNewLine) with
                    {
                        SequenceKey = SequenceKey.NewSequenceKey(),
                    };
                }
            }

            return new PlainTextEditorChunk(
                fileCoordinateGridRequest,
                content,
                constructedPlainTextEditor)
        }
    }
}