using System.Collections.Generic;
using CycloneDX.Models.v1_2;

namespace CycloneDX.CLI.Models
{
    public class ModifiedDiffItem<T>
    {
        public T From { get; set; }
        public T To { get; set; }
    }

    public class DiffItem<T>
    {
        public List<T> Added { get; set; } = new List<T>();
        public List<ModifiedDiffItem<T>> Modified { get; set; } = new List<ModifiedDiffItem<T>>();
        public List<T> Removed { get; set; } = new List<T>();
    }

    public class DiffResult
    {
        public DiffItem<Component> ComponentVersions { get; set; } 
    }
}
