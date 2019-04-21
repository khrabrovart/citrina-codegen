using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKApiSchemaParser;
using VKontakteApiCodeGen.CSharpCode;

namespace VKontakteApiCodeGen
{
    public class Program
    {
        private const string ObjectsDirectory = "Objects";
        private const string ResponsesDirectory = "Responses";

        private static readonly SyntaxGenerator Genreator = new SyntaxGenerator();

        public static async Task Main(string[] args)
        {
            if (!Directory.Exists("gen"))
            {
                Directory.CreateDirectory("gen");
            }

            try
            {
                var vkApiSchema = await VKApiSchema.ParseAsync();

                Console.WriteLine("Schema parsed\nPreparing objects...");

                var objects = vkApiSchema.Objects.Select(o => o.Value).OrderBy(o => o.Name);
                var responses = vkApiSchema.Responses.Select(o => o.Value).OrderBy(o => o.Name);

                Console.WriteLine("Preparing source file models...");

                var objectsFilesProcessor = new SourceFilesProcessor();
                var responsesFilesProcessor = new SourceFilesProcessor();

                foreach (var obj in objects)
                {
                    objectsFilesProcessor.AddToSourceFile(obj);
                }

                foreach (var res in responses)
                {
                    responsesFilesProcessor.AddToSourceFile(res);
                }

                Console.WriteLine("Creating source files...");

                foreach (var sourceFile in objectsFilesProcessor.SourceFiles)
                {
                    await CreateSourceFileAsync(sourceFile, ObjectsDirectory);
                }

                foreach (var sourceFile in responsesFilesProcessor.SourceFiles)
                {
                    await CreateSourceFileAsync(sourceFile, ResponsesDirectory);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private static async Task CreateSourceFileAsync(CSharpSourceFile sourceFile, string directory)
        {
            var syntax = Genreator.GenerateSourceFileSyntax(sourceFile);

            if (string.IsNullOrWhiteSpace(sourceFile.Name))
            {
                throw new Exception("No file name");
            }

            var genDirectory = $@"gen\{directory}";

            if (!Directory.Exists(genDirectory))
            {
                Directory.CreateDirectory(genDirectory);
            }

            var sourceFileName = sourceFile.Name.EndsWith(".cs") ? sourceFile.Name : sourceFile.Name + ".cs";
            await File.WriteAllTextAsync($@"{genDirectory}\{sourceFileName}", syntax);
        }
    }
}
