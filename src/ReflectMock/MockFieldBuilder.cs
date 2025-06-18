using System.Reflection;

namespace ReflectMock;

public sealed class MockFieldBuilder
{
    private FieldAttributes _attributes;
    private string _name;
    private Type? _type;

    internal MockFieldBuilder()
    {
        _name = string.Empty;
    }

    public MockFieldBuilder AutoProperty()
    {
        throw new NotImplementedException();
    }

    public MockFieldBuilder InitOnly()
    {
        _attributes |= FieldAttributes.InitOnly;
        return this;
    }

    public MockFieldBuilder Name(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
        return this;
    }

    public MockFieldBuilder OfType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _type = type;
        return this;
    }

    public MockFieldBuilder Public()
    {
        _attributes |= FieldAttributes.Public;
        return this;
    }

    internal MockFieldInfo Build()
    {
        return new MockFieldInfo(_name, _type, _attributes);
    }
}
