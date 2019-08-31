namespace Code.Generation.Roslyn
{
    public delegate TDataTransferObject ObjectToDataTransferObjectCallback<in T, out TDataTransferObject>(T obj);

    public delegate T DataTransferObjectToObjectCallback<in TDataTransferObject, out T>(TDataTransferObject dataTransferObj);
}
