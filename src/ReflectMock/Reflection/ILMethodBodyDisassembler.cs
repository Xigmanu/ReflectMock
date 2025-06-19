using System.Reflection;
using ReflectMock.Reflection.Metadata;

namespace ReflectMock.Reflection;

internal class ILMethodBodyDisassembler
{
    private readonly Assembly _assembly;

    public ILMethodBodyDisassembler(Assembly assembly)
    {
        _assembly = assembly;
    }

    public MethodBodyInfo Disassemble(MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        MethodBody methodBody =
            methodInfo.GetMethodBody()
            ?? throw new ArgumentException("Method has no definition", nameof(methodInfo));

        byte[] ilBytes = methodBody.GetILAsByteArray()!;
        ParsedILInstruction parsedInstruction = ILInstructionParser.ParseILBytes(
            new ReadOnlyMemory<byte>(ilBytes)
        );

        ILMemberResolver memberResolver = new(_assembly);
        Dictionary<int, MemberInfo> memberReferences = [];

        for (int i = 0; i < parsedInstruction.Operands.Length; i++)
        {
            OperandInfo operandInfo = parsedInstruction.Operands[i];
            MemberInfo? memberInfo = memberResolver.GetMemberInfo(operandInfo);
            if (memberInfo is MethodInfo method)
            {
                memberReferences[operandInfo.Position] = method;
            }
        }

        return new MethodBodyInfo(parsedInstruction.OpCodes, memberReferences);
    }
}
