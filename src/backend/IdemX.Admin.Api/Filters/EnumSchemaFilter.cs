namespace IdemX.Admin.Api.Filters
{
    internal class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum)
            {
                return;
            }

            var underlying = Enum.GetUnderlyingType(context.Type);
            schema.Enum ??= [];
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var enumValue = Enum.Parse(context.Type, name);
                var numeric = Convert.ChangeType(enumValue, underlying);
                schema.Enum.Add(new OpenApiString($"{name} = {numeric}"));
            }
        }
    }
}
