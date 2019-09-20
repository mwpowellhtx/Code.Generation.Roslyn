namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;

    public abstract partial class GeneratedSyntaxTreeRegistryJsonConverter<TRegistry> : JsonConverter<TRegistry>
        where TRegistry : GeneratedSyntaxTreeRegistry, new()
    {
    }
}
