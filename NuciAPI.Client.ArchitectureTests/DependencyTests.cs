using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NuciAPI.Client;

using NUnit.Framework;

namespace NuciAPI.Client.ArchitectureTests
{
    /// <summary>
    /// Dependency and boundary tests over the loaded production architecture of NuciAPI.Client.
    /// </summary>
    [TestFixture]
    public class DependencyTests
    {
        private static readonly ArchUnitNET.Domain.Architecture LoadedArchitecture = BuildArchitecture();

        private static ArchUnitNET.Domain.Architecture BuildArchitecture()
            => new ArchLoader()
                .LoadAssemblyIncludingDependencies(typeof(INuciApiClient).Assembly)
                .Build();

        [Test]
        public void GivenProductionAssemblies_WhenLoadingArchitecture_ThenArchitectureIsBuilt() => Assert.That(
                LoadedArchitecture.Types,
                Is.Not.Empty,
                "No NuciAPI.Client types were loaded; the production assembly inventory is incomplete.");

        [Test]
        public void GivenNuciApiClientTypes_WhenLoadingArchitecture_ThenNoInternalsVisibleTo()
        {
            foreach (System.Reflection.Assembly productionAssembly in LoadedAssemblies())
            {
                Assert.That(
                    productionAssembly.GetCustomAttributes(
                        typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute),
                        inherit: false),
                    Is.Empty,
                    $"Production assembly {productionAssembly.FullName} must not declare InternalsVisibleTo.");
            }
        }

        [Test]
        public void GivenNuciApiClientTypes_WhenCheckingIllegalNamespaces_ThenNoneAreUsed()
        {
            IArchRule noInterfacesNamespaceRule = ArchRuleDefinition.Types()
                .Should().NotResideInNamespaceMatching(@"NuciAPI\.Client\..*\.Interfaces")
                .Because("The '.Interfaces' namespace pattern is prohibited; interfaces must be colocated with their types.");

            IEnumerable<IType> clientTypes = LoadedArchitecture.Types
                .Where(definedType => definedType.Namespace?.FullName.StartsWith("NuciAPI.Client") ?? false);

            Assert.That(
                clientTypes,
                Is.Not.Empty,
                "Vacuous-selector guard: no NuciAPI.Client types were selected for the namespace rule.");

            CheckRulePassed(noInterfacesNamespaceRule);
        }

        [Test]
        public void GivenNuciApiClientTypes_WhenCheckingNoThirdPartyPersistenceDependencies_ThenNoneAreUsed()
        {
            IArchRule noPersistenceRule = ArchRuleDefinition.Types()
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"Microsoft\.EntityFrameworkCore\..*")
                .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"Amazon\.DynamoDBv2\..*")
                .Because("NuciAPI.Client must not depend on persistence frameworks.");

            IEnumerable<IType> clientTypes = LoadedArchitecture.Types
                .Where(definedType => definedType.Namespace?.FullName.StartsWith("NuciAPI.Client") ?? false);

            Assert.That(
                clientTypes,
                Is.Not.Empty,
                "Vacuous-selector guard: no NuciAPI.Client types were selected for the persistence-dependency rule.");

            CheckRulePassed(noPersistenceRule);
        }

        private static IEnumerable<System.Reflection.Assembly> LoadedAssemblies() =>
        [
            typeof(INuciApiClient).Assembly
        ];

        private static void CheckRulePassed(IArchRule archRule)
        {
            IEnumerable<EvaluationResult> failures = archRule
                .Evaluate(LoadedArchitecture)
                .Where(evaluationResult => !evaluationResult.Passed);

            Assert.That(
                failures,
                Is.Empty,
                "Architecture rule violated: " + archRule.Description);
        }
    }
}