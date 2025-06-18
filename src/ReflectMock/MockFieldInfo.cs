using System.Reflection;

namespace ReflectMock;

internal sealed record MockFieldInfo(string Name, Type Type, FieldAttributes Attributes);
