namespace ReflectMock;

internal sealed class MockTypeInfo
{
    public AccessModifiers AccessModifiers { get; internal set; }
    public List<MockFieldInfo> Fields { get; } = [];
    public string Name { get; internal set; }
}
