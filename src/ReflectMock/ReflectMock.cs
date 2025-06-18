using System.Reflection;

namespace ReflectMock;
public static class ReflectMock
{
    public static MockTypeBuilder Struct()
    {
        return new MockTypeBuilder(TypeAttributes.AnsiClass);
    }
}
