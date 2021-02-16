using System.Collections.Generic;
using CycloneDX.Models.v1_2;

namespace CycloneDX.CLI.Models
{
    public class DiffItem<T>
    {
        public List<T> Added { get; set; } = new List<T>();
        public List<T> Removed { get; set; } = new List<T>();
        public List<T> Unchanged { get; set; } = new List<T>();
    }

    public class DiffResult
    {
        // default to nulls. A value, even if it is empty is to indicate that this option has been invoked.
        public Dictionary<string,DiffItem<Component>> ComponentVersions { get; set; } = null;
    }
}
