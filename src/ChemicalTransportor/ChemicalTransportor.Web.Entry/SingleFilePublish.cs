using System.Reflection;
using Furion;

namespace ChemicalTransportor.Web.Entry
{
    public class SingleFilePublish : ISingleFilePublish
    {
        public Assembly[] IncludeAssemblies()
        {
            return Array.Empty<Assembly>();
        }

        public string[] IncludeAssemblyNames()
        {
            return new[]
            {
                "ChemicalTransportor.Application",
                "ChemicalTransportor.Core",
                "ChemicalTransportor.EntityFramework.Core",
                "ChemicalTransportor.Web.Core"
            };
        }
    }
}