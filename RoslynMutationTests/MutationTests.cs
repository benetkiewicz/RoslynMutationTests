namespace RoslynMutationTests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Reflection;

    [TestFixture]
    public class MutationTests
    {
        [Test]
        public void OriginalCodeSimpleTest()
        {
            int result = Calculator.MultiplyByTwo(3);
            Assert.AreEqual(6, result);
        }

        [Test]
        public void OriginalCalculatorCodeShouldWork()
        {
            string calcCode = File.OpenText("Calculator.cs").ReadToEnd();
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(calcCode);

            var initialCompilation = CreateCompilationForSyntaxTree(syntaxTree);
            Assembly initialAssembly = CreateAssemblyForCompilation(initialCompilation);
            RunCalcTestOnAssembly(initialAssembly);
        }

        [Test]
        public void MutatedCalculatorCodeShouldWork()
        {
            string calcCode = File.OpenText("Calculator.cs").ReadToEnd();
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(calcCode);
            var syntaxTreeRoot = (CompilationUnitSyntax)syntaxTree.GetRoot();

            var initialCompilation = CreateCompilationForSyntaxTree(syntaxTree);
            SemanticModel semanticModel = initialCompilation.GetSemanticModel(syntaxTree);
            var calcCodeRewriter = new IntIncrementingRewriter(semanticModel);
            var mutatedSyntaxTreeRoot = calcCodeRewriter.Visit(syntaxTreeRoot);

            var mutatedCompilation = CreateCompilationForSyntaxTree(mutatedSyntaxTreeRoot.SyntaxTree);
            Assembly mutatedAssembly = CreateAssemblyForCompilation(mutatedCompilation);
            RunCalcTestOnAssembly(mutatedAssembly);
        }

        private void RunCalcTestOnAssembly(Assembly assembly)
        {
            Type calculator = assembly.GetType("RoslynMutationTests.Calculator");
            MethodInfo evaluate = calculator.GetMethod("MultiplyByTwo");
            object result = evaluate.Invoke(null, new[] { (object)3 });

            Assert.AreEqual(6, result, string.Format("Expected 6, the result was {0}", result));
        }

        private CSharpCompilation CreateCompilationForSyntaxTree(SyntaxTree syntaxTree)
        {
            return CSharpCompilation.Create(
                "calculator.dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { syntaxTree },
                references: new[] { new MetadataFileReference(typeof(object).Assembly.Location) });
        }

        private Assembly CreateAssemblyForCompilation(CSharpCompilation compilation)
        {
            Assembly compiledAssembly;
            using (var stream = new MemoryStream())
            {
                compilation.Emit(stream);
                compiledAssembly = Assembly.Load(stream.GetBuffer());
            }

            return compiledAssembly;
        }
    }
}
