namespace RoslynMutationTests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    class IntIncrementingRewriter : CSharpSyntaxRewriter
    {
        public SemanticModel SemanticModel { get; private set; }

        public IntIncrementingRewriter(SemanticModel semanticModel)
        {
            
            SemanticModel = semanticModel;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (!node.IsConst)
            {
                return node;
            }

            VariableDeclaratorSyntax currentConst = node.Declaration.Variables[0];
            if (!IsIntegerType(currentConst))
            {
                return node;
            }

            if (currentConst.Initializer == null)
            {
                return node;
            }

            string declaredName = currentConst.Identifier.ValueText;
            int currentConstVal = int.Parse(currentConst.Initializer.Value.ToString());
            int newConstVal = currentConstVal + 1;

            // http://roslynquoter.azurewebsites.net/
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.IntKeyword).WithTrailingTrivia(SyntaxFactory.Space))
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(declaredName))
                                .WithInitializer(SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(SyntaxFactory.TriviaList(), newConstVal.ToString(), newConstVal, SyntaxFactory.TriviaList())
                                        )))
                            )))
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ConstKeyword).WithTrailingTrivia(SyntaxFactory.Space))
                );
        }

        private bool IsIntegerType(VariableDeclaratorSyntax variable)
        {
            var initialiserInfo = SemanticModel.GetTypeInfo(variable.Initializer.Value);
            var declaredType = initialiserInfo.Type;
            return declaredType.ToString() == "int";
        }
    }
}