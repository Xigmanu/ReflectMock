using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace ReflectMock.Reflection.Metadata;

internal static class ILInstructionParser
{
    private const byte MultiByteOpCodeHighByte = 0xFE;
    private const int OperandSize = 4;

    #region ByteOpCodeTables

    private static readonly Dictionary<byte, OpCode> MultiByteOpCodes = new()
    {
        [(ushort)ILOpCode.Arglist & 0xFF] = OpCodes.Arglist,
        [(ushort)ILOpCode.Ceq & 0xFF] = OpCodes.Ceq,
        [(ushort)ILOpCode.Cgt & 0xFF] = OpCodes.Cgt,
        [(ushort)ILOpCode.Cgt_un & 0xFF] = OpCodes.Cgt_Un,
        [(ushort)ILOpCode.Clt_un & 0xFF] = OpCodes.Clt_Un,
        [(ushort)ILOpCode.Ldftn & 0xFF] = OpCodes.Ldftn,
        [(ushort)ILOpCode.Ldvirtftn & 0xFF] = OpCodes.Ldvirtftn,
        [(ushort)ILOpCode.Ldarg & 0xFF] = OpCodes.Ldarg,
        [(ushort)ILOpCode.Ldarga & 0xFF] = OpCodes.Ldarga,
        [(ushort)ILOpCode.Starg & 0xFF] = OpCodes.Starg,
        [(ushort)ILOpCode.Ldloc & 0xFF] = OpCodes.Ldloc,
        [(ushort)ILOpCode.Ldloca & 0xFF] = OpCodes.Ldloca,
        [(ushort)ILOpCode.Stloc & 0xFF] = OpCodes.Stloc,
        [(ushort)ILOpCode.Localloc & 0xFF] = OpCodes.Localloc,
        [(ushort)ILOpCode.Endfilter & 0xFF] = OpCodes.Endfilter,
        [(ushort)ILOpCode.Unaligned & 0xFF] = OpCodes.Unaligned,
        [(ushort)ILOpCode.Volatile & 0xFF] = OpCodes.Volatile,
        [(ushort)ILOpCode.Tail & 0xFF] = OpCodes.Tailcall,
        [(ushort)ILOpCode.Initobj & 0xFF] = OpCodes.Initobj,
        [(ushort)ILOpCode.Constrained & 0xFF] = OpCodes.Constrained,
        [(ushort)ILOpCode.Cpblk & 0xFF] = OpCodes.Cpblk,
        [(ushort)ILOpCode.Initblk & 0xFF] = OpCodes.Initblk,
        [(ushort)ILOpCode.Rethrow & 0xFF] = OpCodes.Rethrow,
        [(ushort)ILOpCode.Sizeof & 0xFF] = OpCodes.Sizeof,
        [(ushort)ILOpCode.Refanytype & 0xFF] = OpCodes.Refanytype,
        [(ushort)ILOpCode.Readonly & 0xFF] = OpCodes.Readonly,
    };

    private static readonly Dictionary<byte, OpCode> SingleByteOpCodes = new()
    {
        [(byte)(ushort)ILOpCode.Nop] = OpCodes.Nop,
        [(byte)(ushort)ILOpCode.Break] = OpCodes.Break,
        [(byte)(ushort)ILOpCode.Ldarg_0] = OpCodes.Ldarg_0,
        [(byte)(ushort)ILOpCode.Ldarg_1] = OpCodes.Ldarg_1,
        [(byte)(ushort)ILOpCode.Ldarg_2] = OpCodes.Ldarg_2,
        [(byte)(ushort)ILOpCode.Ldarg_3] = OpCodes.Ldarg_3,
        [(byte)(ushort)ILOpCode.Ldloc_0] = OpCodes.Ldloc_0,
        [(byte)(ushort)ILOpCode.Ldloc_1] = OpCodes.Ldloc_1,
        [(byte)(ushort)ILOpCode.Ldloc_2] = OpCodes.Ldloc_2,
        [(byte)(ushort)ILOpCode.Ldloc_3] = OpCodes.Ldloc_3,
        [(byte)(ushort)ILOpCode.Stloc_0] = OpCodes.Stloc_0,
        [(byte)(ushort)ILOpCode.Stloc_1] = OpCodes.Stloc_1,
        [(byte)(ushort)ILOpCode.Stloc_2] = OpCodes.Stloc_2,
        [(byte)(ushort)ILOpCode.Stloc_3] = OpCodes.Stloc_3,
        [(byte)(ushort)ILOpCode.Ldarg_s] = OpCodes.Ldarg_S,
        [(byte)(ushort)ILOpCode.Ldarga_s] = OpCodes.Ldarga_S,
        [(byte)(ushort)ILOpCode.Starg_s] = OpCodes.Starg_S,
        [(byte)(ushort)ILOpCode.Ldloc_s] = OpCodes.Ldloc_S,
        [(byte)(ushort)ILOpCode.Ldloca_s] = OpCodes.Ldloca_S,
        [(byte)(ushort)ILOpCode.Stloc_s] = OpCodes.Stloc_S,
        [(byte)(ushort)ILOpCode.Ldnull] = OpCodes.Ldnull,
        [(byte)(ushort)ILOpCode.Ldc_i4_m1] = OpCodes.Ldc_I4_M1,
        [(byte)(ushort)ILOpCode.Ldc_i4_0] = OpCodes.Ldc_I4_0,
        [(byte)(ushort)ILOpCode.Ldc_i4_1] = OpCodes.Ldc_I4_1,
        [(byte)(ushort)ILOpCode.Ldc_i4_2] = OpCodes.Ldc_I4_2,
        [(byte)(ushort)ILOpCode.Ldc_i4_3] = OpCodes.Ldc_I4_3,
        [(byte)(ushort)ILOpCode.Ldc_i4_4] = OpCodes.Ldc_I4_4,
        [(byte)(ushort)ILOpCode.Ldc_i4_5] = OpCodes.Ldc_I4_5,
        [(byte)(ushort)ILOpCode.Ldc_i4_6] = OpCodes.Ldc_I4_6,
        [(byte)(ushort)ILOpCode.Ldc_i4_7] = OpCodes.Ldc_I4_7,
        [(byte)(ushort)ILOpCode.Ldc_i4_8] = OpCodes.Ldc_I4_8,
        [(byte)(ushort)ILOpCode.Ldc_i4_s] = OpCodes.Ldc_I4_S,
        [(byte)(ushort)ILOpCode.Ldc_i4] = OpCodes.Ldc_I4,
        [(byte)(ushort)ILOpCode.Ldc_i8] = OpCodes.Ldc_I8,
        [(byte)(ushort)ILOpCode.Ldc_r4] = OpCodes.Ldc_R4,
        [(byte)(ushort)ILOpCode.Ldc_r8] = OpCodes.Ldc_R8,
        [(byte)(ushort)ILOpCode.Dup] = OpCodes.Dup,
        [(byte)(ushort)ILOpCode.Pop] = OpCodes.Pop,
        [(byte)(ushort)ILOpCode.Jmp] = OpCodes.Jmp,
        [(byte)(ushort)ILOpCode.Call] = OpCodes.Call,
        [(byte)(ushort)ILOpCode.Calli] = OpCodes.Calli,
        [(byte)(ushort)ILOpCode.Ret] = OpCodes.Ret,
        [(byte)(ushort)ILOpCode.Br_s] = OpCodes.Br_S,
        [(byte)(ushort)ILOpCode.Brfalse_s] = OpCodes.Brfalse_S,
        [(byte)(ushort)ILOpCode.Brtrue_s] = OpCodes.Brtrue_S,
        [(byte)(ushort)ILOpCode.Beq_s] = OpCodes.Beq_S,
        [(byte)(ushort)ILOpCode.Bge_s] = OpCodes.Bge_S,
        [(byte)(ushort)ILOpCode.Bgt_s] = OpCodes.Bgt_S,
        [(byte)(ushort)ILOpCode.Ble_s] = OpCodes.Ble_S,
        [(byte)(ushort)ILOpCode.Blt_s] = OpCodes.Blt_S,
        [(byte)(ushort)ILOpCode.Bne_un_s] = OpCodes.Bne_Un_S,
        [(byte)(ushort)ILOpCode.Bge_un_s] = OpCodes.Bge_Un_S,
        [(byte)(ushort)ILOpCode.Bgt_un_s] = OpCodes.Bgt_Un_S,
        [(byte)(ushort)ILOpCode.Ble_un_s] = OpCodes.Ble_Un_S,
        [(byte)(ushort)ILOpCode.Blt_un_s] = OpCodes.Blt_Un_S,
        [(byte)(ushort)ILOpCode.Br] = OpCodes.Br,
        [(byte)(ushort)ILOpCode.Brfalse] = OpCodes.Brfalse,
        [(byte)(ushort)ILOpCode.Brtrue] = OpCodes.Brtrue,
        [(byte)(ushort)ILOpCode.Beq] = OpCodes.Beq,
        [(byte)(ushort)ILOpCode.Bge] = OpCodes.Bge,
        [(byte)(ushort)ILOpCode.Bgt] = OpCodes.Bgt,
        [(byte)(ushort)ILOpCode.Ble] = OpCodes.Ble,
        [(byte)(ushort)ILOpCode.Blt] = OpCodes.Blt,
        [(byte)(ushort)ILOpCode.Bne_un] = OpCodes.Bne_Un,
        [(byte)(ushort)ILOpCode.Bge_un] = OpCodes.Bge_Un,
        [(byte)(ushort)ILOpCode.Bgt_un] = OpCodes.Bgt_Un,
        [(byte)(ushort)ILOpCode.Ble_un] = OpCodes.Ble_Un,
        [(byte)(ushort)ILOpCode.Blt_un] = OpCodes.Blt_Un,
        [(byte)(ushort)ILOpCode.Switch] = OpCodes.Switch,
        [(byte)(ushort)ILOpCode.Ldind_i1] = OpCodes.Ldind_I1,
        [(byte)(ushort)ILOpCode.Ldind_u1] = OpCodes.Ldind_U1,
        [(byte)(ushort)ILOpCode.Ldind_i2] = OpCodes.Ldind_I2,
        [(byte)(ushort)ILOpCode.Ldind_u2] = OpCodes.Ldind_U2,
        [(byte)(ushort)ILOpCode.Ldind_i4] = OpCodes.Ldind_I4,
        [(byte)(ushort)ILOpCode.Ldind_u4] = OpCodes.Ldind_U4,
        [(byte)(ushort)ILOpCode.Ldind_i8] = OpCodes.Ldind_I8,
        [(byte)(ushort)ILOpCode.Ldind_i] = OpCodes.Ldind_I,
        [(byte)(ushort)ILOpCode.Ldind_r4] = OpCodes.Ldind_R4,
        [(byte)(ushort)ILOpCode.Ldind_r8] = OpCodes.Ldind_R8,
        [(byte)(ushort)ILOpCode.Ldind_ref] = OpCodes.Ldind_Ref,
        [(byte)(ushort)ILOpCode.Stind_ref] = OpCodes.Stind_Ref,
        [(byte)(ushort)ILOpCode.Stind_i1] = OpCodes.Stind_I1,
        [(byte)(ushort)ILOpCode.Stind_i2] = OpCodes.Stind_I2,
        [(byte)(ushort)ILOpCode.Stind_i4] = OpCodes.Stind_I4,
        [(byte)(ushort)ILOpCode.Stind_i8] = OpCodes.Stind_I8,
        [(byte)(ushort)ILOpCode.Stind_r4] = OpCodes.Stind_R4,
        [(byte)(ushort)ILOpCode.Stind_r8] = OpCodes.Stind_R8,
        [(byte)(ushort)ILOpCode.Add] = OpCodes.Add,
        [(byte)(ushort)ILOpCode.Sub] = OpCodes.Sub,
        [(byte)(ushort)ILOpCode.Mul] = OpCodes.Mul,
        [(byte)(ushort)ILOpCode.Div] = OpCodes.Div,
        [(byte)(ushort)ILOpCode.Div_un] = OpCodes.Div_Un,
        [(byte)(ushort)ILOpCode.Rem] = OpCodes.Rem,
        [(byte)(ushort)ILOpCode.Rem_un] = OpCodes.Rem_Un,
        [(byte)(ushort)ILOpCode.And] = OpCodes.And,
        [(byte)(ushort)ILOpCode.Or] = OpCodes.Or,
        [(byte)(ushort)ILOpCode.Xor] = OpCodes.Xor,
        [(byte)(ushort)ILOpCode.Shl] = OpCodes.Shl,
        [(byte)(ushort)ILOpCode.Shr] = OpCodes.Shr,
        [(byte)(ushort)ILOpCode.Shr_un] = OpCodes.Shr_Un,
        [(byte)(ushort)ILOpCode.Neg] = OpCodes.Neg,
        [(byte)(ushort)ILOpCode.Not] = OpCodes.Not,
        [(byte)(ushort)ILOpCode.Conv_i1] = OpCodes.Conv_I1,
        [(byte)(ushort)ILOpCode.Conv_i2] = OpCodes.Conv_I2,
        [(byte)(ushort)ILOpCode.Conv_i4] = OpCodes.Conv_I4,
        [(byte)(ushort)ILOpCode.Conv_i8] = OpCodes.Conv_I8,
        [(byte)(ushort)ILOpCode.Conv_r4] = OpCodes.Conv_R4,
        [(byte)(ushort)ILOpCode.Conv_r8] = OpCodes.Conv_R8,
        [(byte)(ushort)ILOpCode.Conv_u4] = OpCodes.Conv_U4,
        [(byte)(ushort)ILOpCode.Conv_u8] = OpCodes.Conv_U8,
        [(byte)(ushort)ILOpCode.Callvirt] = OpCodes.Callvirt,
        [(byte)(ushort)ILOpCode.Cpobj] = OpCodes.Cpobj,
        [(byte)(ushort)ILOpCode.Ldobj] = OpCodes.Ldobj,
        [(byte)(ushort)ILOpCode.Ldstr] = OpCodes.Ldstr,
        [(byte)(ushort)ILOpCode.Newobj] = OpCodes.Newobj,
        [(byte)(ushort)ILOpCode.Castclass] = OpCodes.Castclass,
        [(byte)(ushort)ILOpCode.Isinst] = OpCodes.Isinst,
        [(byte)(ushort)ILOpCode.Conv_r_un] = OpCodes.Conv_R_Un,
        [(byte)(ushort)ILOpCode.Unbox] = OpCodes.Unbox,
        [(byte)(ushort)ILOpCode.Throw] = OpCodes.Throw,
        [(byte)(ushort)ILOpCode.Ldfld] = OpCodes.Ldfld,
        [(byte)(ushort)ILOpCode.Ldflda] = OpCodes.Ldflda,
        [(byte)(ushort)ILOpCode.Stfld] = OpCodes.Stfld,
        [(byte)(ushort)ILOpCode.Ldsfld] = OpCodes.Ldsfld,
        [(byte)(ushort)ILOpCode.Ldsflda] = OpCodes.Ldsflda,
        [(byte)(ushort)ILOpCode.Stsfld] = OpCodes.Stsfld,
        [(byte)(ushort)ILOpCode.Stobj] = OpCodes.Stobj,
        [(byte)(ushort)ILOpCode.Conv_ovf_i1_un] = OpCodes.Conv_Ovf_I1_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_i2_un] = OpCodes.Conv_Ovf_I2_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_i4_un] = OpCodes.Conv_Ovf_I4_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_i8_un] = OpCodes.Conv_Ovf_I8_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_u1_un] = OpCodes.Conv_Ovf_U1_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_u2_un] = OpCodes.Conv_Ovf_U2_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_u4_un] = OpCodes.Conv_Ovf_U4_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_u8_un] = OpCodes.Conv_Ovf_U8_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_i_un] = OpCodes.Conv_Ovf_I_Un,
        [(byte)(ushort)ILOpCode.Conv_ovf_u_un] = OpCodes.Conv_Ovf_U_Un,
        [(byte)(ushort)ILOpCode.Box] = OpCodes.Box,
        [(byte)(ushort)ILOpCode.Newarr] = OpCodes.Newarr,
        [(byte)(ushort)ILOpCode.Ldlen] = OpCodes.Ldlen,
        [(byte)(ushort)ILOpCode.Ldelema] = OpCodes.Ldelema,
        [(byte)(ushort)ILOpCode.Ldelem_i1] = OpCodes.Ldelem_I1,
        [(byte)(ushort)ILOpCode.Ldelem_u1] = OpCodes.Ldelem_U1,
        [(byte)(ushort)ILOpCode.Ldelem_i2] = OpCodes.Ldelem_I2,
        [(byte)(ushort)ILOpCode.Ldelem_u2] = OpCodes.Ldelem_U2,
        [(byte)(ushort)ILOpCode.Ldelem_i4] = OpCodes.Ldelem_I4,
        [(byte)(ushort)ILOpCode.Ldelem_u4] = OpCodes.Ldelem_U4,
        [(byte)(ushort)ILOpCode.Ldelem_i8] = OpCodes.Ldelem_I8,
        [(byte)(ushort)ILOpCode.Ldelem_i] = OpCodes.Ldelem_I,
        [(byte)(ushort)ILOpCode.Ldelem_r4] = OpCodes.Ldelem_R4,
        [(byte)(ushort)ILOpCode.Ldelem_r8] = OpCodes.Ldelem_R8,
        [(byte)(ushort)ILOpCode.Ldelem_ref] = OpCodes.Ldelem_Ref,
        [(byte)(ushort)ILOpCode.Stelem_i] = OpCodes.Stelem_I,
        [(byte)(ushort)ILOpCode.Stelem_i1] = OpCodes.Stelem_I1,
        [(byte)(ushort)ILOpCode.Stelem_i2] = OpCodes.Stelem_I2,
        [(byte)(ushort)ILOpCode.Stelem_i4] = OpCodes.Stelem_I4,
        [(byte)(ushort)ILOpCode.Stelem_i8] = OpCodes.Stelem_I8,
        [(byte)(ushort)ILOpCode.Stelem_r4] = OpCodes.Stelem_R4,
        [(byte)(ushort)ILOpCode.Stelem_r8] = OpCodes.Stelem_R8,
        [(byte)(ushort)ILOpCode.Stelem_ref] = OpCodes.Stelem_Ref,
        [(byte)(ushort)ILOpCode.Ldelem] = OpCodes.Ldelem,
        [(byte)(ushort)ILOpCode.Stelem] = OpCodes.Stelem,
        [(byte)(ushort)ILOpCode.Unbox_any] = OpCodes.Unbox_Any,
        [(byte)(ushort)ILOpCode.Conv_ovf_i1] = OpCodes.Conv_Ovf_I1,
        [(byte)(ushort)ILOpCode.Conv_ovf_u1] = OpCodes.Conv_Ovf_U1,
        [(byte)(ushort)ILOpCode.Conv_ovf_i2] = OpCodes.Conv_Ovf_I2,
        [(byte)(ushort)ILOpCode.Conv_ovf_u2] = OpCodes.Conv_Ovf_U2,
        [(byte)(ushort)ILOpCode.Conv_ovf_i4] = OpCodes.Conv_Ovf_I4,
        [(byte)(ushort)ILOpCode.Conv_ovf_u4] = OpCodes.Conv_Ovf_U4,
        [(byte)(ushort)ILOpCode.Conv_ovf_i8] = OpCodes.Conv_Ovf_I8,
        [(byte)(ushort)ILOpCode.Conv_ovf_u8] = OpCodes.Conv_Ovf_U8,
        [(byte)(ushort)ILOpCode.Refanyval] = OpCodes.Refanyval,
        [(byte)(ushort)ILOpCode.Ckfinite] = OpCodes.Ckfinite,
        [(byte)(ushort)ILOpCode.Mkrefany] = OpCodes.Mkrefany,
        [(byte)(ushort)ILOpCode.Ldtoken] = OpCodes.Ldtoken,
        [(byte)(ushort)ILOpCode.Conv_u2] = OpCodes.Conv_U2,
        [(byte)(ushort)ILOpCode.Conv_u1] = OpCodes.Conv_U1,
        [(byte)(ushort)ILOpCode.Conv_i] = OpCodes.Conv_I,
        [(byte)(ushort)ILOpCode.Conv_ovf_i] = OpCodes.Conv_Ovf_I,
        [(byte)(ushort)ILOpCode.Conv_ovf_u] = OpCodes.Conv_Ovf_U,
        [(byte)(ushort)ILOpCode.Add_ovf] = OpCodes.Add_Ovf,
        [(byte)(ushort)ILOpCode.Add_ovf_un] = OpCodes.Add_Ovf_Un,
        [(byte)(ushort)ILOpCode.Mul_ovf] = OpCodes.Mul_Ovf,
        [(byte)(ushort)ILOpCode.Mul_ovf_un] = OpCodes.Mul_Ovf_Un,
        [(byte)(ushort)ILOpCode.Sub_ovf] = OpCodes.Sub_Ovf,
        [(byte)(ushort)ILOpCode.Sub_ovf_un] = OpCodes.Sub_Ovf_Un,
        [(byte)(ushort)ILOpCode.Endfinally] = OpCodes.Endfinally,
        [(byte)(ushort)ILOpCode.Leave] = OpCodes.Leave,
        [(byte)(ushort)ILOpCode.Leave_s] = OpCodes.Leave_S,
        [(byte)(ushort)ILOpCode.Stind_i] = OpCodes.Stind_I,
        [(byte)(ushort)ILOpCode.Conv_u] = OpCodes.Conv_U,
    };

    #endregion

    public static ParsedILInstruction ParseILBytes(ReadOnlyMemory<byte> il)
    {
        List<OpCode> instructions = [];
        List<OperandInfo> operands = [];
        int bytesToSkip = 0;
        int instructionPosition = 0;
        ReadOnlySpan<byte> ilSpan = il.Span;

        for (int i = 0; i < il.Length; i++)
        {
            byte op = ilSpan[i];
            if (bytesToSkip != 0 || i != 0 && ilSpan[i - 1] == MultiByteOpCodeHighByte)
            {
                bytesToSkip--;
                continue;
            }

            OpCode opCode =
                op != MultiByteOpCodeHighByte ? SingleByteOpCodes[op] : MultiByteOpCodes[op];

            instructions.Add(opCode);
            if (opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
            {
                OperandInfo operandInfo = new(instructionPosition, opCode, il.Slice(i + 1, OperandSize));
                operands.Add(operandInfo);
                bytesToSkip += OperandSize;
            }
            instructionPosition++;
        }

        return new ParsedILInstruction([.. instructions], [.. operands]);
    }
}
