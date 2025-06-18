using ReflectMock.Reflection;

namespace ReflectMock;

public sealed class MockTypeBuilder
{
    private readonly MockTypeInfo _mockInfo;

    public MockTypeBuilder()
    {
        _mockInfo = new();
    }

    public MockTypeBuilder AccessModifiers(AccessModifiers modifiers)
    {
        _mockInfo.AccessModifiers = modifiers;
        return this;
    }

    public Type Build()
    {
        return new MockTypeGenerator(_mockInfo).CreateType();
    }

    public MockTypeBuilder Name(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _mockInfo.Name = name;
        return this;
    }

    public MockTypeBuilder WithField(MockFieldBuilder fieldBuilder)
    {
        ArgumentNullException.ThrowIfNull(fieldBuilder);
        _mockInfo.Fields.Add(fieldBuilder.Build());
        return this;
    }
}
