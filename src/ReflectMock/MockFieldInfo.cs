namespace ReflectMock;

internal sealed class MockFieldInfo
{
    public AccessModifiers Modifiers { get; internal set; }
    public string Name { get; internal set; }
    public Type Type { get; internal set; }
    public bool IsInitOnly { get; internal set; }
}
