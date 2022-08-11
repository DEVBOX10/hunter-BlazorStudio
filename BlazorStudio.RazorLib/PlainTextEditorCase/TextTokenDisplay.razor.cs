using BlazorStudio.ClassLib.Store.CSharpKeywords;
using BlazorStudio.ClassLib.Store.PlainTextEditorCase;
using BlazorStudio.ClassLib.Store.RazorKeywords;
using Fluxor;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Text;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.FileSystemApi;
using BlazorStudio.ClassLib.RoslynHelpers;
using BlazorStudio.ClassLib.Sequence;
using BlazorStudio.ClassLib.Store.SolutionCase;
using Fluxor.Blazor.Web.Components;
using Microsoft.CodeAnalysis.Text;

namespace BlazorStudio.RazorLib.PlainTextEditorCase;

public partial class TextTokenDisplay : FluxorComponent
{
    [Inject]
    private IState<CSharpKeywords> CSharpKeywordsWrap { get; set; } = null!;
    [Inject]
    private IState<RazorKeywords> RazorKeywordsWrap { get; set; } = null!;
    [Inject]
    private IStateSelection<TokenSemanticsState, SemanticDescription> TokenSemanticsStateSelector { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [CascadingParameter] 
    public IAbsoluteFilePath AbsoluteFilePath { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public ITextToken TextToken { get; set; } = null!;
    [Parameter, EditorRequired]
    public int StartOfSpanRelativeToRow { get; set; }
    [Parameter, EditorRequired]
    public long StartOfRowSpanRelativeToDocument { get; set; }

    private string TokenClass => GetTokenClass();

    private SequenceKey? _previousSequenceKey;
    private TextTokenKey? _previousTextTokenKey;


    private long GetTextTokenLength()
    {
        // This if statement is a bit hacky by checking copy text for "\t" character.
        // WhitespaceTextToken is a private type so I cannot check the WhitespaceKind without
        // making changes and I don't want to get distracted down a possible rabbit hole right now.
        if (TextToken.Kind == TextTokenKind.Whitespace &&
            TextToken.CopyText == "\t")
        {
            // Do not map roslyn character indices with '\t' representing 4 spaces.
            return TextToken.CopyText.Length;
        }
        else
        {
            return TextToken.PlainText.Length;
        }
    }

    private long StartOfSpan => StartOfRowSpanRelativeToDocument + StartOfSpanRelativeToRow;
    private long EndOfSpan =>  StartOfSpan + GetTextTokenLength();
    
    private string TitleDebuggingInfo =>
        ($"start: {StartOfSpan}" +
        " | " + 
        $"end: {EndOfSpan}" +
        " | " +
        $"end - start: {EndOfSpan - StartOfSpan}" +
        " | " +
        $"GetTextTokenLength(): {GetTextTokenLength()}");
    
    protected override void OnInitialized()
    {
        TokenSemanticsStateSelector
            .Select(x =>
            {
                if (x.SemanticDescriptionsMap.TryGetValue(TextToken.Key, out var semanticDescription))
                {
                    return semanticDescription;
                }

                var defaultSemanticDescription = new SemanticDescription()
                {
                    SyntaxKind = default,
                    SequenceKey = SequenceKey.NewSequenceKey()
                };
                
                return defaultSemanticDescription;
            },
            valueEquals: (previous, next) => previous.SequenceKey == next.SequenceKey);
        
        base.OnInitialized();
    }

    protected override bool ShouldRender()
    {
        var currentTokenSemanticsState = TokenSemanticsStateSelector.Value;

        if (_previousSequenceKey != currentTokenSemanticsState.SequenceKey ||
            _previousTextTokenKey != TextToken.Key)
        {
            return true;
        }

        return false;
        
        return base.ShouldRender();
    }

    private string GetTokenClass()
    {
        if (AbsoluteFilePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.RAZOR_MARKUP)
        {
            // TODO: I am just isolating things to ease development then I will DRY up the code
            return GetRazorClass();
        }
        
        bool isKeyword = false;

        var classBuilder = new StringBuilder();
        
        var startOfSpanInclusive = StartOfRowSpanRelativeToDocument + StartOfSpanRelativeToRow;
        var endOfSpanExclusive =
            StartOfRowSpanRelativeToDocument + StartOfSpanRelativeToRow + TextToken.PlainText.Length;
        
        var absoluteFilePathValue = new AbsoluteFilePathStringValue(AbsoluteFilePath);

        // Check is keyword
        {        
            var localCSharpKeywords = CSharpKeywordsWrap.Value;

            if (TextToken.Kind == TextTokenKind.Default &&
                localCSharpKeywords.Keywords.Any(x => x == TextToken.PlainText))
            {
                isKeyword = true;
                classBuilder.Append("pte_plain-text-editor-text-token-display-keyword");
            }
        }
        
        if (!isKeyword)
        {
            var currentTokenSemanticsState = TokenSemanticsStateSelector.Value;

            classBuilder.Append(currentTokenSemanticsState.CssClassString ?? string.Empty);
            // classBuilder.Append(SyntaxKindToCssStringConverter.Convert(currentTokenSemanticsStateSelector.SyntaxKind));
        }
        
        return classBuilder.ToString();
    }
    
    private string GetRazorClass()
    {
        bool isKeyword = false;

        var classBuilder = new StringBuilder();

        var localCSharpKeywords = CSharpKeywordsWrap.Value;

        if (TextToken.Kind == TextTokenKind.Default &&
            localCSharpKeywords.Keywords.Any(x => x == TextToken.PlainText))
        {
            isKeyword = true;
            classBuilder.Append("pte_plain-text-editor-text-token-display-keyword");
        }

        var localRazorKeywords = RazorKeywordsWrap.Value;

        foreach (var keywordFunc in localRazorKeywords.KeywordFuncs)
        {
            var classResult = keywordFunc.Invoke(TextToken.PlainText);

            if (!string.IsNullOrWhiteSpace(classResult))
            {
                if (!isKeyword)
                {
                    // Don't mark as keyword twice redundantly
                    isKeyword = true;
                    classBuilder.Append("pte_plain-text-editor-text-token-display-keyword");
                }
            }
        }

        if (!isKeyword)
        {
            var startOfSpanInclusive = StartOfRowSpanRelativeToDocument + StartOfSpanRelativeToRow;
            var endOfSpanExclusive =
                StartOfRowSpanRelativeToDocument + StartOfSpanRelativeToRow + TextToken.PlainText.Length;

            var absoluteFilePathValue = new AbsoluteFilePathStringValue(AbsoluteFilePath);

            
        }

        return classBuilder.ToString();
    }
}
