using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKApiSchemaParser;

namespace VKontakteApiCodeGen
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (!Directory.Exists("gen"))
            {
                Directory.CreateDirectory("gen");
            }

            var vkApiSchema = await VKApiSchema.ParseAsync();

            Console.WriteLine("Objects parsed\nPreparing objects...");

            var objects = vkApiSchema.Objects.Select(o => o.Value).OrderBy(o => o.Name);

            Console.WriteLine("Preparing source file models...");

            var sourceFilesProcessor = new SourceFilesProcessor();

            foreach (var obj in objects)
            {
                sourceFilesProcessor.AddToSourceFile(obj);
            }

            Console.WriteLine("Generating code for source files...");

            var syntaxGenerator = new SyntaxGenerator();

            foreach (var sourceFile in sourceFilesProcessor.SourceFiles)
            {
                var syntax = syntaxGenerator.GenerateSourceFileSyntax(sourceFile);
                await File.WriteAllTextAsync(sourceFile.Name, syntax);
            }
        }
    }
}
