﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.Parsing.C.SyntaxNodes;
using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;

namespace BlazorStudio.ClassLib.Parsing.C;

public class Parser
{
    private readonly ImmutableArray<ISyntaxToken> _tokens;
    private readonly Stack<ISyntaxNode> _nodeStack = new();

    public Parser(ImmutableArray<ISyntaxToken> tokens)
    {
        _tokens = tokens;
    }
    
    public ISyntaxNode Parse()
    {
        foreach (var token in _tokens)
        {
            switch (token.SyntaxKind)
            {
                case SyntaxKind.NumericLiteralToken:
                    ParseNumericLiteralToken((NumericLiteralToken)token);
                    break;
                // case SyntaxKind.LibraryReferenceToken:
                //     ParseLibraryReferenceToken((LibraryReferenceToken)token);
                //     break;
                case SyntaxKind.PlusToken:
                    ParsePlusToken((PlusToken)token);
                    break;
            }
        }

        return _nodeStack.Pop();
    }

    private void ParseNumericLiteralToken(NumericLiteralToken token)
    {
        var literalNumericExpressionNode = new NumericLiteralExpressionNode(
            token);
        
        if (!_nodeStack.Any())
        {
            _nodeStack.Push(literalNumericExpressionNode);
            return;
        }
        
        var poppedNode = _nodeStack.Pop();

        switch (poppedNode.SyntaxKind)
        {
            case SyntaxKind.NumericThreePartExpressionNode:
            {
                var numericThreePartExpressionNode = (NumericThreePartExpressionNode)poppedNode;

                numericThreePartExpressionNode = new NumericThreePartExpressionNode(
                    numericThreePartExpressionNode.LeftNumericExpressionNode,
                    numericThreePartExpressionNode.OperatorNode,
                    literalNumericExpressionNode);

                _nodeStack.Push(numericThreePartExpressionNode);

                return;
            }
            case SyntaxKind.VariableAssignmentExpressionNode:
                throw new NotImplementedException();
            case SyntaxKind.ParenthesizedExpressionNode:
                throw new NotImplementedException();
            default:
                // TODO: Report a diagnostic and return?
                throw new NotImplementedException();
        }
    }
    
    private void ParsePlusToken(PlusToken token)
    {
        var operatorAdditionNode = new OperatorAdditionNode(
            token);
        
        if (!_nodeStack.Any())
        {
            _nodeStack.Push(operatorAdditionNode);
            return;
        }
        
        var poppedNode = _nodeStack.Pop();

        switch (poppedNode.SyntaxKind)
        {
            case SyntaxKind.NumericThreePartExpressionNode:
            {
                var numericThreePartExpressionNode = (NumericThreePartExpressionNode)poppedNode;

                numericThreePartExpressionNode = new NumericThreePartExpressionNode(
                    numericThreePartExpressionNode.LeftNumericExpressionNode,
                    operatorAdditionNode,
                    numericThreePartExpressionNode.RightNumericExpressionNode);

                _nodeStack.Push(numericThreePartExpressionNode);

                return;
            }
            default:
            {
                if (poppedNode is NumericExpressionNode numericExpressionNode)
                {
                    var numericThreePartExpressionNode = new NumericThreePartExpressionNode(
                        numericExpressionNode,
                        operatorAdditionNode,
                        null);
                    
                    _nodeStack.Push(numericThreePartExpressionNode);

                    return;
                }

                // TODO: Report a diagnostic and return?
                throw new NotImplementedException();
            }
        }
    }
    
    private void ParseLibraryReferenceToken(LibraryReferenceToken token)
    {
        throw new NotImplementedException();
    }
}