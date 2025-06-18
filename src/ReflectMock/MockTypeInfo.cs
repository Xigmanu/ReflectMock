using System.Reflection;

namespace ReflectMock;

internal sealed record MockTypeInfo(
    string Name,
    TypeAttributes Attributes,
    MockFieldInfo[] MockFields
);
