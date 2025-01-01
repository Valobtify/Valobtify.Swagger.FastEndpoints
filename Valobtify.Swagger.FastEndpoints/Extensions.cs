using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema.NewtonsoftJson.Generation;
using System.Reflection;
using Resulver;

namespace Valobtify.Swagger.FastEndpoints;

public static class Extensions
{
    public static IEnumerable<Type> GetSingleValueObjectTypes(this Assembly assembly)
    {
        var baseType = typeof(SingleValueObject<,>);

        return assembly
            .GetTypes()
            .Where(type =>
                type is
                {
                    IsClass: true,
                    IsAbstract: false,
                    BaseType.IsGenericType: true
                } &&
                type.BaseType.GetGenericTypeDefinition() == baseType);
    }


    public static void AddSingleValueObjectMapper(this ICollection<ITypeMapper> mappers, params IEnumerable<Assembly> assemblies)
    {
        var schemaGenerator = new JsonSchemaGenerator(new NewtonsoftJsonSchemaGeneratorSettings());

        assemblies
            .SelectMany(assembly => assembly
                .GetSingleValueObjectTypes())
            .ToList()
            .ForEach(singleValueObjectType =>
            {
                mappers.Add(new PrimitiveTypeMapper(singleValueObjectType,
                    transformer =>
                    {
                        var valueType = singleValueObjectType
                            .GetProperty(nameof(SingleValueObjectTemplate.Value))!
                            .PropertyType;

                        var schema = schemaGenerator.Generate(valueType);

                        transformer.Type = schema.Type;
                    }));
            });
    }

    public class SingleValueObjectTemplate :
        SingleValueObject<SingleValueObjectTemplate, string>,
        ICreatableValueObject<SingleValueObjectTemplate, string>
    {
        public SingleValueObjectTemplate(string value) : base(value)
        {
        }

        public static Result<SingleValueObjectTemplate> Create(string value)
        {
            throw new NotImplementedException();
        }
    }
}
