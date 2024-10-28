namespace Domain.Exceptions;

public class AlreadyExistException : Exception
{
    public string Name { get; init; }

    public string Key { get; init; }

    public AlreadyExistException(string name, string key)
        : base($"Entity {name} is already exist with the value {key}")
    {
        Name = name;
        Key = key;
    }
}