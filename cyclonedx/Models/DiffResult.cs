using System.Collections.Generic;
using CycloneDX.Models.v1_2;
using CycloneDX.Utils;

namespace CycloneDX.CLI.Models
{
    public class DiffResult
    {
        // default to nulls. A value, even if it is empty is to indicate that this option has been invoked.
        public Dictionary<string,DiffItem<Component>> ComponentVersions { get; set; } = null;
    }
}
