using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpInterface : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpMethod[] Methods { get; set; }

        public static CSharpInterface FromMethods(ApiMethod[] methods)
        {
            return new CSharpInterface
            {
                Name = 'I' + methods[0].Category.ToBeautifiedName(),
                Methods = methods.SelectMany(m => CSharpMethod.FromMethod(m, true)).ToArray()
            };
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            builder.Line($"public interface {Name}");
            builder.Block(() =>
            {
                for (int i = 0; i < Methods.Length; i++)
                {
                    Methods[i].WriteSyntax(builder);

                    if (i != Methods.Length - 1)
                    {
                        builder.Line();
                    }
                }
            });
        }
    }
}
