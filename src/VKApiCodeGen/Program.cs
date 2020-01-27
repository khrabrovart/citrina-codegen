using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKApiCodeGen.Extensions;
using VKApiCodeGen.Generator.Entities;
using VKApiSchemaParser;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen
{
    // Проверить объекты ответов от методов
    // Сделать, чтобы из int? делался DateTime? для Юникстайма
    public class Program
    {
        private const string ParentDirectory = "Generated";
        private const string ObjectsDirectory = "Objects";
        private const string ResponsesDirectory = "Responses";
        private const string MethodsDirectory = "Methods";
        private const string InterfacesDirectory = "Contracts";
        private const string CrossMethodEnumsDirectory = "CrossMethodEnums";

        private static readonly SourcesManager sourcesManager = new SourcesManager();

        public static async Task Main()
        {
            if (Directory.Exists(ParentDirectory))
            {
                Directory.Delete(ParentDirectory, true);
            }

            try
            {
                var vkApiSchema = await VKApiSchema.ParseAsync();

                Console.WriteLine("Schema parsed");
                Console.WriteLine("Preparing objects");

                var objects = vkApiSchema.Objects.Select(o => o.Value);
                var responses = vkApiSchema.Responses.Select(o => o.Value);
                var methods = vkApiSchema.Methods.Select(m => m.Value);

                Console.WriteLine("Generating syntax");

                AddObjects(objects, ObjectsDirectory);
                AddObjects(responses, ResponsesDirectory);
                AddMethods(methods, InterfacesDirectory, MethodsDirectory);
                AddCrossMethodEnums(CSharpMethod.CrossMethodEnumParameters, CrossMethodEnumsDirectory);

                Console.WriteLine("Writing files");

                var writingTasks = sourcesManager.SourceGroups.SelectMany(sg => sg.Value.Select(sf => WriteSourceFile(sf, $"{sg.Key}")));
                await Task.WhenAll(writingTasks);

                Console.WriteLine($"\nComplete! Check \"{ParentDirectory}\" directory for output files.\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static void AddObjects(IEnumerable<ApiObject> objects, string directory)
        {
            foreach (var obj in objects)
            {
                var objectClass = CSharpSourceFile.FromObject(obj);
                sourcesManager.AddToSourceGroup(Path.Combine(directory, obj.Name.Split('_')[0].ToBeautifiedName()), objectClass);
            }
        }

        private static void AddMethods(IEnumerable<ApiMethod> methods, string interfacesDirectory, string methodsDirectory)
        {
            var methodGroups = methods.GroupBy(m => m.Category);

            foreach (var methodGroup in methodGroups)
            {
                var methodsInterface = CSharpSourceFile.FromMethods(methodGroup.ToArray(), asInterface: true);
                var methodsClass = CSharpSourceFile.FromMethods(methodGroup.ToArray(), asInterface: false);

                sourcesManager.AddToSourceGroup(interfacesDirectory, methodsInterface);
                sourcesManager.AddToSourceGroup(methodsDirectory, methodsClass);
            }
        }

        private static void AddCrossMethodEnums(IEnumerable<CSharpEnum> enums, string directory)
        {
            foreach (var e in enums)
            {
                var enumSourceFile = new CSharpSourceFile
                {
                    Name = e.Name,
                    Enum = e
                };

                sourcesManager.AddToSourceGroup(Path.Combine(directory), enumSourceFile);
            }
        }

        private static Task WriteSourceFile(CSharpSourceFile sourceFile, string directory)
        {
            if (string.IsNullOrWhiteSpace(sourceFile.Name))
            {
                throw new Exception("No file name");
            }

            var sourceCode = sourceFile.GetSourceCode();
            var genDirectory = $@"{ParentDirectory}\{directory}";

            if (!Directory.Exists(genDirectory))
            {
                Directory.CreateDirectory(genDirectory);
            }

            var sourceFileName = sourceFile.Name.EndsWith(".cs") ? sourceFile.Name : sourceFile.Name + ".cs";
            return File.WriteAllTextAsync($@"{genDirectory}\{sourceFileName}", sourceCode);
        }
    }
}
