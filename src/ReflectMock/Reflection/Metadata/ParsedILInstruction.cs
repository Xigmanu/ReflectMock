using System.Reflection.Emit;

namespace ReflectMock.Reflection.Metadata;

internal readonly record struct ParsedILInstruction(OpCode[] OpCodes, OperandInfo[] Operands);
