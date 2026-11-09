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
    /// Roslyn-based source-file convention checks over the handwritten production sources of NuciAPI.Client.
    /// </summary>
    [TestFixture]
    public class SourceFileConventionTests
    {
        private static readonly string[] CheckedProjects = new[]
        {
            Path.Combine(GetRepositoryRoot(), "NuciAPI.Client")
        };

        private static string GetRepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(typeof(SourceFileConventionTests).Assembly.Location);

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
        public void GivenCheckedProjects_WhenCollectingSources_ThenEligibleFilesAreFound()
        {
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "No eligible production source files were found; the checked project list or repository layout is wrong.");
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingFileNames_ThenEachFileMatchesItsSingleDeclaredType()
        {
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();
            List<string> violations = new List<string>();

            Assert.That(
                sourceFiles,
                Is.Not.Empty,
                "Vacuous-selector guard: no production source files were collected.");

            foreach (string sourceFile in sourceFiles)
            {
                CollectFileNameViolations(sourceFile, violations);
            }

            Assert.That(
                violations,
                Is.Empty,
                "One-type-per-file or filename-stem matching was violated:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingRegions_ThenNoneAreUsed()
        {
            CollectPreprocessorTriviaViolations(CollectEligibleSourceFiles());
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingTopLevelStatements_ThenNoneAreUsed()
        {
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();
            List<string> violations = new List<string>();

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxTree tree = ParseSourceFile(sourceFile);
                bool hasTypeDeclaration = tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>().Any();

                if (!hasTypeDeclaration)
                {
                    violations.Add($"{sourceFile}: no type declaration found; top-level statements are prohibited.");
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Top-level statements or type-less source files were found:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void GivenHandwrittenProductionSources_WhenCheckingNullableDirectives_ThenNoneAreUsed()
        {
            IEnumerable<string> sourceFiles = CollectEligibleSourceFiles();
            List<string> violations = new List<string>();

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (SyntaxTrivia directive in root.DescendantTrivia().Where(trivia => trivia.IsDirective))
                {
                    if (directive.ToString().Contains("#nullable"))
                    {
                        violations.Add($"{sourceFile}: #nullable directives are prohibited.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "#nullable directives were found:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
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

        private static void CollectFileNameViolations(string sourceFile, List<string> violations)
        {
            SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();
            List<TypeDeclarationSyntax> typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(declaration => declaration.Parent is not TypeDeclarationSyntax)
                .ToList();

            if (typeDeclarations.Count is 0)
            {
                return;
            }

            string fileNameStem = Path.GetFileNameWithoutExtension(sourceFile);

            if (typeDeclarations.Count > 1)
            {
                violations.Add($"{sourceFile}: declares {typeDeclarations.Count} top-level types; exactly one type per file is required.");
            }

            foreach (TypeDeclarationSyntax declaration in typeDeclarations)
            {
                string typeName = declaration.Identifier.Text;

                if (!string.Equals(typeName, fileNameStem, StringComparison.Ordinal))
                {
                    violations.Add($"{sourceFile}: type name '{typeName}' does not case-sensitively match the file name stem '{fileNameStem}'.");
                }
            }
        }

        private static void CollectPreprocessorTriviaViolations(IEnumerable<string> sourceFiles)
        {
            List<string> violations = new List<string>();

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxNode root = ParseSourceFile(sourceFile).GetRoot();

                foreach (SyntaxTrivia trivia in root.DescendantTrivia())
                {
                    string text = trivia.ToString();

                    if (trivia.IsDirective && (text.Contains("#region") || text.Contains("#endregion")))
                    {
                        violations.Add($"{sourceFile}: #region/#endregion directives are prohibited.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Region directives were found:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }
    }
}
