﻿namespace BlazorStudio.ClassLib.Parsing.C;

public enum SyntaxKind
{
    // Tokens
    CommentMultiLineToken,
    CommentSingleLineToken,
    IdentifierToken,
    KeywordToken,
    NumericLiteralToken,
    StringLiteralToken,
    TriviaToken,
    PreprocessorDirectiveToken,
    LibraryReferenceToken,
    PlusToken,
    EqualsToken,
    StatementDelimiterToken,
    EndOfFileToken,

    // Nodes
    LiteralExpressionNode,
    BoundLiteralExpressionNode,
    BoundBinaryOperatorNode,
    BoundBinaryExpressionNode,
    PreprocessorDirectiveStatement,
    BoundTypeNode,
}