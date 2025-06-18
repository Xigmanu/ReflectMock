namespace ReflectMock;

public sealed class MockFieldBuilder
{
    private readonly MockFieldInfo _fieldMockInfo;

    public MockFieldBuilder()
    {
        _fieldMockInfo = new();
    }

    public MockFieldBuilder InitOnly()
    {
        _fieldMockInfo.IsInitOnly = true;
        return this;
    }

    public MockFieldBuilder Name(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _fieldMockInfo.Name = name;
        return this;
    }

    public MockFieldBuilder OfType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _fieldMockInfo.Type = type;
        return this;
    }

    public MockFieldBuilder WithModifiers(AccessModifiers modifiers)
    {
        _fieldMockInfo.Modifiers = modifiers;
        return this;
    }

    internal MockFieldInfo Build()
    {
        return _fieldMockInfo;
    }
}
