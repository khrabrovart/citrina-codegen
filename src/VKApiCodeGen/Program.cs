using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKApiCodeGen.Extensions;
using VKApiCodeGen.Generator.Entities;
using VKApiSchemaParser;

namespace VKApiCodeGen
{
    // Проверить объекты ответов от методов
    // Создать enum-ы для параметров методов
    // Сделать, чтобы из int? делался DateTime? для Юникстайма
    public class Program
    {
        private const string ParentDirectory = "gen";
        private const string ObjectsDirectory = "Objects";
        private const string ResponsesDirectory = "Responses";
        private const string MethodsDirectory = "Methods";
        private const string InterfacesDirectory = "Contracts";

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

                Console.WriteLine("Creating objects classes");

                foreach (var obj in objects)
                {
                    var objectClass = CSharpSourceFile.FromObject(obj);
                    sourcesManager.AddToSourceGroup(Path.Combine(ObjectsDirectory, obj.Name.Split('_')[0].ToBeautifiedName()), objectClass);
                }

                Console.WriteLine("Creating responses classes");

                foreach (var response in responses)
                {
                    var responseClass = CSharpSourceFile.FromObject(response);
                    sourcesManager.AddToSourceGroup(Path.Combine(ResponsesDirectory, response.Name.Split('_')[0].ToBeautifiedName()), responseClass);
                }

                Console.WriteLine("Creating methods interfaces and classes");

                var methodGroups = methods.GroupBy(m => m.Category);

                foreach (var methodGroup in methodGroups)
                {
                    var methodsInterface = CSharpSourceFile.FromMethods(methodGroup.ToArray(), asInterface: true);
                    var methodsClass = CSharpSourceFile.FromMethods(methodGroup.ToArray(), asInterface: false);

                    sourcesManager.AddToSourceGroup(Path.Combine(InterfacesDirectory), methodsInterface);
                    sourcesManager.AddToSourceGroup(Path.Combine(MethodsDirectory), methodsClass);
                }

                foreach (var sourceCategory in sourcesManager.SourceGroups)
                {
                    foreach (var sourceFile in sourceCategory.Value)
                    {
                        await WriteSourceFile(sourceFile, $"{sourceCategory.Key}");
                    }
                }

                Console.WriteLine($"\nComplete! Check \"{ParentDirectory}\" directory for output files.\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
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
