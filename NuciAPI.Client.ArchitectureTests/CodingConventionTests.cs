using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciAPI.Client.ArchitectureTests
{
    /// <summary>
    /// Roslyn-based coding-convention checks over the handwritten production sources of NuciAPI.Client.
    /// </summary>
    [TestFixture]
    public class CodingConventionTests
    {
        private static readonly string[] CheckedProjects = new[]
        {
            Path.Combine(GetRepositoryRoot(), "NuciAPI.Client")
        };

        private static string GetRepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(typeof(CodingConventionTests).Assembly.Location);

            while (directory is not null &&
                !File.Exists(Path.Combine(directory.FullName, "NuciAPI.Client.slnx")) &&
                !File.Exists(Path.Combine(directory.FullName, "NuciAPI.Client.sln")))
            {
                directory = directory.Parent;
            }

            return directory is null
                ? string.Empty
                : directory.FullName;
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingForbiddenPatterns_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (SyntaxTrivia trivia in root.DescendantTrivia())
                {
                    string text = trivia.ToString();

                    if (text.Contains("#nullable"))
                    {
                        violations.Add($"{sourceFile}: #nullable directives are prohibited.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Forbidden directives were found:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoVarUsage_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (VariableDeclarationSyntax declaration in root.DescendantNodes().OfType<VariableDeclarationSyntax>())
                {
                    if (declaration.Type.IsVar)
                    {
                        violations.Add($"{sourceFile}: 'var' is prohibited; use explicit types.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Explicit-type convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoTernary_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (ConditionalExpressionSyntax conditional in root.DescendantNodes().OfType<ConditionalExpressionSyntax>())
                {
                    violations.Add($"{sourceFile}: ternary conditional expressions are prohibited.");
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "No-ternary convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoTuples_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (TupleTypeSyntax tuple in root.DescendantNodes().OfType<TupleTypeSyntax>())
                {
                    violations.Add($"{sourceFile}: tuple types are prohibited; return a dedicated type for multiple values.");
                }

                foreach (TupleExpressionSyntax tuple in root.DescendantNodes().OfType<TupleExpressionSyntax>())
                {
                    violations.Add($"{sourceFile}: tuple expressions are prohibited; return a dedicated type for multiple values.");
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "No-tuple convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoRefParameters_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (ParameterSyntax parameter in root.DescendantNodes().OfType<ParameterSyntax>())
                {
                    if (parameter.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.RefKeyword)))
                    {
                        violations.Add($"{sourceFile}: 'ref' parameters are prohibited.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "No-ref convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoOptionalParameters_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (ParameterSyntax parameter in root.DescendantNodes().OfType<ParameterSyntax>())
                {
                    if (parameter.Default is not null)
                    {
                        violations.Add($"{sourceFile}: optional/default parameters are prohibited; use overloads.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "No-optional-parameters convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingControlFlowBodies_ThenAllUseBraces()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (IfStatementSyntax ifStatement in root.DescendantNodes().OfType<IfStatementSyntax>())
                {
                    if (ifStatement.Statement is not BlockSyntax)
                    {
                        violations.Add($"{sourceFile}: 'if' bodies must use braces; line {ifStatement.GetLocation().GetLineSpan().StartLinePosition.Line + 1}.");
                    }
                }

                foreach (ForEachStatementSyntax forEach in root.DescendantNodes().OfType<ForEachStatementSyntax>())
                {
                    if (forEach.Statement is not BlockSyntax)
                    {
                        violations.Add($"{sourceFile}: 'foreach' bodies must use braces; line {forEach.GetLocation().GetLineSpan().StartLinePosition.Line + 1}.");
                    }
                }

                foreach (ForStatementSyntax forStatement in root.DescendantNodes().OfType<ForStatementSyntax>())
                {
                    if (forStatement.Statement is not BlockSyntax)
                    {
                        violations.Add($"{sourceFile}: 'for' bodies must use braces; line {forStatement.GetLocation().GetLineSpan().StartLinePosition.Line + 1}.");
                    }
                }

                foreach (WhileStatementSyntax whileStatement in root.DescendantNodes().OfType<WhileStatementSyntax>())
                {
                    if (whileStatement.Statement is not BlockSyntax)
                    {
                        violations.Add($"{sourceFile}: 'while' bodies must use braces; line {whileStatement.GetLocation().GetLineSpan().StartLinePosition.Line + 1}.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Braced-control-flow convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingParameterCounts_ThenAtMostSevenParameters()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (method.ParameterList.Parameters.Count > 7)
                    {
                        violations.Add($"{sourceFile}: method '{method.Identifier.Text}' has {method.ParameterList.Parameters.Count} parameters; at most 7 are allowed.");
                    }
                }

                foreach (ConstructorDeclarationSyntax constructor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
                {
                    if (constructor.ParameterList.Parameters.Count > 7)
                    {
                        violations.Add($"{sourceFile}: constructor has {constructor.ParameterList.Parameters.Count} parameters; at most 7 are allowed.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Seven-parameter limit was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNoPreviewFeatures_ThenNoneAreUsed()
        {
            List<string> violations = new List<string>();
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (RecordDeclarationSyntax record in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
                {
                    violations.Add($"{sourceFile}: record types are preview features and are prohibited.");
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Preview-language-feature convention was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        private static IEnumerable<string> CollectEligibleSourceFiles()
        {
            List<string> sourceFiles = new List<string>();

            foreach (string projectDirectory in CheckedProjects)
            {
                if (!Directory.Exists(projectDirectory))
                {
                    continue;
                }

                sourceFiles.AddRange(Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                    .Where(sourceFile => !sourceFile.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                    .Where(sourceFile => !sourceFile.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                    .Where(sourceFile => !sourceFile.EndsWith(".Designer.cs"))
                    .Where(sourceFile => !sourceFile.EndsWith(".AssemblyInfo.cs")));
            }

            return sourceFiles;
        }

        private static SyntaxTree ParseSourceFile(string sourceFile)
            => CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile, Encoding.UTF8));
    }
}
