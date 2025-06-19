using System.Reflection;
using System.Reflection.Emit;

namespace ReflectMock.Reflection;

public sealed record MethodBodyInfo(OpCode[] OpCodes, Dictionary<int, MemberInfo> MemberReferences);
