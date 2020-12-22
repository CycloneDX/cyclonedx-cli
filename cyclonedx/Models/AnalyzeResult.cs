using System.Collections.Generic;
using CycloneDX.Models.v1_2;

namespace CycloneDX.CLI.Models
{
    public class AnalyzeResult
    {
        public List<List<Component>> MultipleComponentVersions { get; set; } 
    }
}
