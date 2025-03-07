﻿using BlazorTextEditor.RazorLib.Lexing;

namespace BlazorStudio.ClassLib.CodeAnalysis.C.Syntax.SyntaxTokens;

public class OpenParenthesisToken : ISyntaxToken
{
    public OpenParenthesisToken(
        TextEditorTextSpan textEditorTextSpan)
    {
        TextEditorTextSpan = textEditorTextSpan;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public SyntaxKind SyntaxKind => SyntaxKind.OpenParenthesisToken;
}