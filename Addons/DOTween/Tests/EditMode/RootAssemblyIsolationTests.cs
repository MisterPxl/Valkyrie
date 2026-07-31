using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.PackageManager;

namespace Valkyrie.DOTween.Tests.EditMode
{
    public sealed class RootAssemblyIsolationTests
    {
        private const string RootPackageName = "com.misterpxl.valkyrie";

        [Test]
        public void RootAssemblyDefinitions_HaveNoDOTweenReferences()
        {
            string rootPackagePath = ResolveRootPackagePath();
            string[] rootAssemblyDefinitionPaths =
            {
                Path.Combine(rootPackagePath, "Runtime", "Valkyrie.Runtime.asmdef"),
                Path.Combine(rootPackagePath, "Editor", "Valkyrie.Editor.asmdef")
            };

            for (int index = 0; index < rootAssemblyDefinitionPaths.Length; index++)
            {
                AssertAssemblyDefinitionHasNoDOTweenReference(rootAssemblyDefinitionPaths[index]);
            }
        }

        private static string ResolveRootPackagePath()
        {
            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();
            for (int index = 0; index < packages.Length; index++)
            {
                PackageInfo package = packages[index];
                if (package.name == RootPackageName)
                {
                    return package.resolvedPath;
                }
            }

            Assembly runtimeAssembly = typeof(TweenSequenceAsset).Assembly;
            PackageInfo addonPackage = PackageInfo.FindForAssembly(runtimeAssembly);
            Assert.That(
                addonPackage,
                Is.Not.Null,
                "The DOTween add-on package could not be resolved from its runtime assembly.");

            DirectoryInfo candidate = new DirectoryInfo(addonPackage.resolvedPath);
            while (candidate != null)
            {
                string runtimeAssemblyDefinition = Path.Combine(
                    candidate.FullName,
                    "Runtime",
                    "Valkyrie.Runtime.asmdef");
                string editorAssemblyDefinition = Path.Combine(
                    candidate.FullName,
                    "Editor",
                    "Valkyrie.Editor.asmdef");
                if (File.Exists(runtimeAssemblyDefinition) && File.Exists(editorAssemblyDefinition))
                {
                    return candidate.FullName;
                }

                candidate = candidate.Parent;
            }

            Assert.Fail(
                "Could not find the root Valkyrie package from registered packages or by walking up " +
                "from the DOTween add-on package root.");
            return string.Empty;
        }

        private static void AssertAssemblyDefinitionHasNoDOTweenReference(string path)
        {
            Assert.That(File.Exists(path), Is.True, "Missing assembly definition: " + path);
            string contents = File.ReadAllText(path);
            StringAssert.DoesNotContain(
                "DOTween",
                contents,
                "The root package must remain usable without DOTween: " + path);
        }
    }
}
