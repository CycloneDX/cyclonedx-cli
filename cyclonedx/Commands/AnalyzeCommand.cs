﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CycloneDX.Models.v1_2;
using CycloneDX.Json;
using CycloneDX.CLI.Commands;
using CycloneDX.CLI.Models;
using CycloneDX.Utils;

namespace CycloneDX.CLI
{
    partial class Program
    {
        internal static void ConfigureAnalyzeCommand(RootCommand rootCommand)
        {
            var subCommand = new Command("analyze", "Analyze an SBOM file");
            subCommand.Add(new Option<string>("--input-file", "Input SBOM filename, will read from stdin if no value provided."));
            subCommand.Add(new Option<InputFormat>("--input-format", "Specify input file format."));
            subCommand.Add(new Option<StandardOutputFormat>("--output-format", "Specify output format (defaults to text)."));
            subCommand.Add(new Option<bool>("--multiple-component-versions", "Report components that have multiple versions in use."));
            subCommand.Handler = CommandHandler.Create<string, InputFormat, StandardOutputFormat, bool>(Analyze);
            rootCommand.Add(subCommand);
        }

        public static async Task<int> Analyze(
            string inputFile, InputFormat inputFormat, StandardOutputFormat outputFormat,
            bool multipleComponentVersions)
        {
            var inputBomFormat = InputFormatHelper(inputFile, inputFormat);
            if (inputBomFormat == BomFormat.Unsupported) return (int)ExitCode.ParameterValidationError;

            var inputBomString = await InputFileHelper(inputFile);
            if (inputBomString == null) return (int)ExitCode.ParameterValidationError;
            
            var inputBom = CLIUtils.BomDeserializer(inputBomString, inputBomFormat);

            var result = new AnalyzeResult();

            if (multipleComponentVersions)
            {
                result.MultipleComponentVersions = CycloneDXUtils.MultipleComponentVersions(inputBom);
            }

            if (outputFormat == StandardOutputFormat.json)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                };

                options.Converters.Add(new Json.v1_2.Converters.ComponentTypeConverter());
                options.Converters.Add(new Json.v1_2.Converters.DataFlowConverter());
                options.Converters.Add(new Json.v1_2.Converters.DateTimeConverter());
                options.Converters.Add(new Json.v1_2.Converters.DependencyConverter());
                options.Converters.Add(new Json.v1_2.Converters.ExternalReferenceTypeConverter());
                options.Converters.Add(new Json.v1_2.Converters.HashAlgorithmConverter());
                options.Converters.Add(new Json.v1_2.Converters.IssueClassificationConverter());
                options.Converters.Add(new Json.v1_2.Converters.LicenseConverter());
                options.Converters.Add(new Json.v1_2.Converters.PatchClassificationConverter());

                Console.WriteLine(JsonSerializer.Serialize(result, options));
            }
            else
            {
                if (inputBom.Metadata?.Component != null)
                {
                    var component = inputBom.Metadata.Component;
                    Console.WriteLine($"Analysis results for {component.Name}@{component.Version}:");
                }
                else
                {
                    Console.WriteLine("Analysis results:");
                }
                Console.WriteLine();

                if (result.MultipleComponentVersions != null)
                {
                    Console.WriteLine("Components with multiple versions:");
                    Console.WriteLine();
                    if (result.MultipleComponentVersions.Count == 0)
                    {
                        Console.WriteLine("None");
                    }
                    else
                    {
                        foreach (var componentEntry in result.MultipleComponentVersions)
                        {
                            Console.Write(componentEntry.Key);
                            Console.Write(" versions:");
                            foreach (var component in componentEntry.Value)
                            {
                                Console.Write(" ");
                                Console.Write(component.Version);
                            }
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine();
                }
            }

            return (int)ExitCode.Ok;
        }
    }
}