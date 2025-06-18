using System.Reflection;
using ReflectMock.Reflection;

namespace ReflectMock;

public sealed class MockTypeBuilder
{
    private string _name;
    private TypeAttributes _typeAttributes;
    private List<MockFieldInfo> _mockFields;

    internal MockTypeBuilder(TypeAttributes typeAttributes)
    {
        _name = string.Empty;
        _typeAttributes = typeAttributes;
        _mockFields = [];
    }

    public Type Build()
    {
        MockTypeInfo typeInfo = new(_name, _typeAttributes, [.. _mockFields]);
        return new MockTypeGenerator(typeInfo).CreateType();
    }

    public MockTypeBuilder Name(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _name = name;
        return this;
    }

    public MockTypeBuilder Public()
    {
        _typeAttributes |= TypeAttributes.Public;
        return this;
    }

    public MockTypeBuilder WithField(
        Func<MockFieldBuilder, MockFieldBuilder> fieldBuilderConfigurator
    )
    {
        MockFieldBuilder mockFieldBuilder = fieldBuilderConfigurator(new MockFieldBuilder());
        _mockFields.Add(mockFieldBuilder.Build());
        return this;
    }
}
