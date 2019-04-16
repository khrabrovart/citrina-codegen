using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKApiSchemaParser;

namespace CitrinaCodeGeneration
{
    public class Program
    {


        public static async Task Main(string[] args)
        {
            var vkApiSchema = await VKApiSchema.ParseAsync();
            var codeGen = new SimpleCodeGenerator();
            var sourceFilesProcessor = new SourceFilesProcessor();

            if (!Directory.Exists("gen"))
            {
                Directory.CreateDirectory("gen");
            }

            var objects = vkApiSchema.Objects.Select(o => o.Value).OrderBy(o => o.Name);

            foreach (var obj in objects)
            {
                sourceFilesProcessor.AddToSourceFile(obj);
            }

            foreach (var sourceFile in sourceFilesProcessor.SourceFiles)
            {
                await codeGen.CreateSourceFileAsync(sourceFile);
            }
        }
    }
}
