namespace Domain.Exceptions;

public class NotFoundException : Exception
{
    public string Name { get; init; }

    public string Key { get; init; }

    public NotFoundException(string name, string key)
        : base($"Entity {name} was not found with value {key}")
    {
        Name = name;
        Key = key;
    }
}
