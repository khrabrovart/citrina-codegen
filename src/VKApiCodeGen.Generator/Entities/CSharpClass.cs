﻿using System;
using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpClass : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public string Interface { get; set; }

        public IList<CSharpProperty> Properties { get; set; }

        public CSharpMethod[] Methods { get; set; }

        public IList<CSharpEnum> NestedEnums { get; set; }

        public static CSharpClass Map(ApiObject obj)
        {
            var propertyObjects = ExtractProperties(obj);

            return new CSharpClass
            {
                Name = obj.Name.ToBeautifiedName(),
                Summary = string.IsNullOrWhiteSpace(obj.Description) ? null : new CSharpSummary(obj.Description),
                Properties = propertyObjects.Where(o => !o.IsEnum()).Select(CSharpProperty.Map).ToList(),
                NestedEnums = propertyObjects.Where(o => o.IsEnum()).Select(o => CSharpEnum.FromObject(o)).ToList(),
                Methods = Array.Empty<CSharpMethod>()
            };
        }

        public static CSharpClass FromMethods(ApiMethod[] methods)
        {
            var name = methods[0].Category.ToBeautifiedName();

            return new CSharpClass
            {
                Name = name,
                Interface = 'I' + name,
                Methods = methods.SelectMany(m => CSharpMethod.FromMethod(m, false)).ToArray(),
                Properties = Array.Empty<CSharpProperty>(),
                NestedEnums = Array.Empty<CSharpEnum>(),
            };
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            var interfaceString = !string.IsNullOrWhiteSpace(Interface) ? $" : {Interface}" : string.Empty;

            builder.Line($"public class {Name}{interfaceString}");
            builder.Block(() =>
            {
                var notEmpty = false;

                if (NestedEnums.Any())
                {
                    WriteIterableSyntax(builder, NestedEnums.ToArray());
                    notEmpty = true;
                }

                if (Properties.Any())
                {
                    if (notEmpty)
                    {
                        builder.Line();
                    }

                    WriteIterableSyntax(builder, Properties.ToArray());
                    notEmpty = true;
                }

                if (Methods.Any())
                {
                    if (notEmpty)
                    {
                        builder.Line();
                    }

                    WriteIterableSyntax(builder, Methods);
                }
            });
        }

        private static IEnumerable<ApiObject> ExtractProperties(ApiObject obj)
        {
            if (obj.AllOf != null)
            {
                return obj.AllOf.SelectMany(o => ExtractProperties(o));
            }
            
            if (obj.OneOf != null)
            {
                return obj.OneOf.SelectMany(o => ExtractProperties(o));
            }
            
            if (obj.Properties != null)
            {
                return obj.Properties;
            }

            // TODO: Think about pattern properties "patternProperties".
            return Enumerable.Empty<ApiObject>();
        }

        private static void WriteIterableSyntax(SyntaxBuilder builder, ISyntaxEntity[] syntaxEntities)
        {
            for (int i = 0; i < syntaxEntities.Length; i++)
            {
                syntaxEntities[i].WriteSyntax(builder);

                if (i != syntaxEntities.Length - 1)
                {
                    builder.Line();
                }
            }
        }
    }
}
