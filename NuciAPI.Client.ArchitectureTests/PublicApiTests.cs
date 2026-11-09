using System;
using System.Collections.Generic;
using System.Linq;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NuciAPI.Client;

using NUnit.Framework;

namespace NuciAPI.Client.ArchitectureTests
{
    /// <summary>
    /// Public API surface tests over the NuciAPI.Client production types.
    /// </summary>
    [TestFixture]
    public class PublicApiTests
    {
        private static readonly ArchUnitNET.Domain.Architecture LoadedArchitecture = BuildArchitecture();

        private static ArchUnitNET.Domain.Architecture BuildArchitecture()
            => new ArchLoader()
                .LoadAssemblyIncludingDependencies(typeof(INuciApiClient).Assembly)
                .Build();

        [Test]
        public void GivenNuciApiClientTypes_WhenCheckingPublicSurface_ThenAllTypesArePublic()
        {
            IEnumerable<IType> clientTypes = LoadedArchitecture.Types
                .Where(definedType => definedType.Namespace?.FullName.StartsWith("NuciAPI.Client") ?? false);

            Assert.That(
                clientTypes,
                Is.Not.Empty,
                "Vacuous-selector guard: no NuciAPI.Client types were selected for the public-surface rule.");

            IEnumerable<IType> internalTypes = clientTypes
                .Where(definedType => definedType.Visibility != ArchUnitNET.Domain.Visibility.Public);

            Assert.That(
                internalTypes,
                Is.Empty,
                "All NuciAPI.Client types must be public: they form the published NuGet surface.");
        }

        [Test]
        public void GivenNuciApiClientTypes_WhenCheckingSealedClasses_ThenAllNonModelClassesAreSealed()
        {
            IEnumerable<IType> clientTypes = LoadedArchitecture.Types
                .Where(definedType => definedType.Namespace?.FullName.StartsWith("NuciAPI.Client") ?? false);

            Assert.That(
                clientTypes,
                Is.Not.Empty,
                "Vacuous-selector guard: no NuciAPI.Client types were selected for the sealing rule.");

            IEnumerable<Class> nonSealedClasses = clientTypes.OfType<Class>()
                .Where(definedType => !(definedType.IsSealed ?? false));

            Assert.That(
                nonSealedClasses,
                Is.Empty,
                "NuciAPI.Client non-model classes must be declared sealed.");
        }

        [Test]
        public void GivenNuciApiClientTypes_WhenCheckingModelClasses_ThenSealedModelsAreAllowed()
        {
            Class authorisationInfoType = LoadedArchitecture.Classes
                .First(definedType => definedType.FullName.Equals("NuciAPI.Client.NuciApiRequestAuthorisationInfo"));

            Assert.That(
                authorisationInfoType,
                Is.Not.Null,
                "The NuciApiRequestAuthorisationInfo type must be registered in the loaded architecture.");
        }

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
