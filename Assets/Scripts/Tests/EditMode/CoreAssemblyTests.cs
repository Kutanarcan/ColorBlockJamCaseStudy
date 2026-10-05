using System.Reflection;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class CoreAssemblyTests
    {
        [Test]
        public void CoreAssembly_Compiles_WithoutUnityEngine()
        {
            Assembly core = Assembly.Load("Game.Core");

            foreach (AssemblyName reference in core.GetReferencedAssemblies())
            {
                Assert.That(reference.Name, Does.Not.StartWith("UnityEngine"));
                Assert.That(reference.Name, Does.Not.StartWith("UnityEditor"));
            }
        }
    }
}
