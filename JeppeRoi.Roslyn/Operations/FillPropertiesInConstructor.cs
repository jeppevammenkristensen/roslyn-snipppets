using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JeppeRoi.Roslyn.Operations
{
    public class FillPropertiesInConstructor
    {
        public async Task<SyntaxNode> GenerateAsync(SyntaxTree tree, SemanticModel model)
        {
            var root = await tree.GetRootAsync();
            var visitor = new FillPropertiesPropertyVisitor(model);
            return visitor.Visit(root);
        }
    }

    public class FillPropertiesPropertyVisitor : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;

        public FillPropertiesPropertyVisitor(SemanticModel model)
        {
            _model = model;
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var result = (INamedTypeSymbol)_model.GetSymbolInfo(node.Type).Symbol;
            var properties = result.GetMembers().OfType<IPropertySymbol>().Where(x => x.DeclaredAccessibility == Accessibility.Public);

            var missingProperties = properties.Where(x => !ExistingPropertiesSet(node).Contains(x.Name));
            var expression = CreateExpressions(missingProperties).ToArray();
            return node.ReplaceNode(node.Initializer, node.Initializer.AddExpressions(expression));      
        }

        private ExpressionSyntax Get(ITypeSymbol type)
        {
            if (type.SpecialType != SpecialType.System_String &&  type.AllInterfaces.Any(x => x.SpecialType == SpecialType.System_Collections_IEnumerable))
            {
                if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
          
                {
                    return SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("List"))
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList())
                        .WithInitializer(
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.CollectionInitializerExpression,
                                SyntaxFactory.SingletonSeparatedList(
                                    Get(namedType.TypeArguments.First()))));

                }
            }   

            switch (type.SpecialType)
            {
                case SpecialType.None:
                case SpecialType.System_Object:
                case SpecialType.System_Enum:
                case SpecialType.System_MulticastDelegate:
                case SpecialType.System_Delegate:
                case SpecialType.System_ValueType:
                case SpecialType.System_Void:
                    break;
                case SpecialType.System_Boolean:
                    return SyntaxFactory.LiteralExpression(
                        SyntaxKind.TrueLiteralExpression);
                    //yield return SyntaxFactory.AssignmentExpression(
                    //    SyntaxKind.SimpleAssignmentExpression,
                      //  SyntaxFactory.IdentifierName(missingProperty.Name),
                       // );
                    break;
                case SpecialType.System_Char:
                    break;
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(50));
                    //yield return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    //    SyntaxFactory.IdentifierName(missingProperty.Name),
                    //   );
                    //break;
                case SpecialType.System_Decimal:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(50.234m));
                    break;
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(50.234));
                    break;
                case SpecialType.System_String:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal("Jeppe"));
                    break;
                case SpecialType.System_IntPtr:
                    break;
                case SpecialType.System_UIntPtr:
                    break;
                case SpecialType.System_Array:
                    break;
                case SpecialType.System_Collections_IEnumerable:
                    break;
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_ICollection_T:
                    int i = 0;
                    break;
                case SpecialType.System_Collections_IEnumerator:
                    break;
                case SpecialType.System_Collections_Generic_IEnumerator_T:
                    break;
                case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                    break;
                case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                    break;
                case SpecialType.System_Nullable_T:
                    break;
                case SpecialType.System_DateTime:
                    break;
                case SpecialType.System_Runtime_CompilerServices_IsVolatile:
                    break;
                case SpecialType.System_IDisposable:
                    break;
                case SpecialType.System_TypedReference:
                    break;
                case SpecialType.System_ArgIterator:
                    break;
                case SpecialType.System_RuntimeArgumentHandle:
                    break;
                case SpecialType.System_RuntimeFieldHandle:
                    break;
                case SpecialType.System_RuntimeMethodHandle:
                    break;
                case SpecialType.System_RuntimeTypeHandle:
                    break;
                case SpecialType.System_IAsyncResult:
                    break;
                case SpecialType.System_AsyncCallback:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private IEnumerable<ExpressionSyntax> CreateExpressions(IEnumerable<IPropertySymbol> missingProperties)
        {
            foreach (var missingProperty in missingProperties)
            {

                var type = missingProperty.Type;
                var expression = Get(type);

                if (expression != null)
                {
                    yield return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(missingProperty.Name), expression);
                }
            }

            yield break;
        }

        public IEnumerable<string> ExistingPropertiesSet(ObjectCreationExpressionSyntax node)
        {
            foreach (var assignment in node.Initializer.Expressions.OfType<AssignmentExpressionSyntax>())
            {
                if (assignment.Left is IdentifierNameSyntax identifier)
                {
                    yield return identifier.Identifier.Text;
                }
            }
        }
    }
}

 