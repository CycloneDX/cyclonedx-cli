using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using CycloneDX.Json;
using CycloneDX.Models;
using CycloneDX.Xml;

namespace CycloneDX.CLI
{
    public enum ExitCode
    {
        Ok,
        OkFail,
        IOError,
        ParameterValidationError,
        UnsupportedFormat
    }
}
