using System.Reflection.Emit;

namespace ReflectMock.Reflection.Metadata;

internal readonly record struct OperandInfo(
    int Position,
    OpCode OpCode,
    ReadOnlyMemory<byte> Token
);
