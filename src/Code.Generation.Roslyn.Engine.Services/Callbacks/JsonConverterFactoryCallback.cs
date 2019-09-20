namespace Code.Generation.Roslyn
{
    using Newtonsoft.Json;

    public delegate TJsonConverter JsonConverterFactoryCallback<out TJsonConverter>()
        where TJsonConverter : JsonConverter;

    public delegate TJsonConverter JsonConverterFactoryCallback<T, out TJsonConverter>()
        where TJsonConverter : JsonConverter<T>;
}
