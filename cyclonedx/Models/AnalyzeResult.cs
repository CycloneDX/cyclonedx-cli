using System.Collections.Generic;
using CycloneDX.Models.v1_3;

namespace CycloneDX.CLI.Models
{
    public class AnalyzeResult
    {
        public Dictionary<string,List<Component>> MultipleComponentVersions { get; set; } 
    }
}
