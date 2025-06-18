
namespace ReflectMock.Reflection;

internal sealed class MockTypeGenerator
{
    private readonly MockTypeInfo _typeInfo;

    public MockTypeGenerator(MockTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
    }

    internal Type CreateType()
    {
        throw new NotImplementedException();
    }
}
