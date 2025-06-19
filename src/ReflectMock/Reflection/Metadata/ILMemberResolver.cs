using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace ReflectMock.Reflection.Metadata;

internal sealed class ILMemberResolver
{
    private readonly Assembly _assembly;
    private readonly ISignatureTypeProvider<Type, Type[]?> _signatureTypeProvider;
    private readonly Dictionary<EntityHandle, Type> _handleTypeCache;

    public ILMemberResolver(Assembly assembly)
    {
        _assembly = assembly;
        _signatureTypeProvider = new SignatureTypeProvider(assembly);
        _handleTypeCache = [];
    }

    public unsafe MemberInfo? GetMemberInfo(OperandInfo operandInfo)
    {
        if (!_assembly.TryGetRawMetadata(out byte* blob, out int length))
        {
            return null;
        }

        int token = BitConverter.ToInt32(operandInfo.Token.Span);
        MetadataReader reader = new(blob, length);

        if (CheckTokenHighByteByTableIdx(operandInfo.Token.Span, TableIndex.MemberRef))
        {
            MemberReference? nMemberRef = GetMethodMemberReference(reader, token);
            return nMemberRef is MemberReference memberRef
                ? GetMemberInfoFromMemberRef(reader, memberRef)
                : null;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckTokenHighByteByTableIdx(
        in ReadOnlySpan<byte> tkn,
        TableIndex tableIndex
    )
    {
        return tkn[^1] == ((byte)tableIndex);
    }

    private static MemberReference? GetMethodMemberReference(MetadataReader reader, int token)
    {
        MemberReferenceHandle tknHandle = MetadataTokens.MemberReferenceHandle(token);
        if (!tknHandle.IsNil)
        {
            MemberReference memberRef = reader.GetMemberReference(tknHandle);
            if (memberRef.GetKind() == MemberReferenceKind.Method)
            {
                return memberRef;
            }
        }
        return null;
    }

    private MethodInfo? GetMemberInfoFromMemberRef(MetadataReader reader, MemberReference memberRef)
    {
        string memberRefName = reader.GetString(memberRef.Name);
        if (!_handleTypeCache.TryGetValue(memberRef.Parent, out Type? parentType))
        {
            parentType = GetTypeFromHandle(reader, memberRef.Parent);
            if (parentType == null)
            {
                return null;
            }
            _handleTypeCache[memberRef.Parent] = parentType;
        }

        MethodSignature<Type> methodSignature = memberRef.DecodeMethodSignature(
            _signatureTypeProvider,
            genericContext: null
        );

        foreach (MethodInfo runtimeMethod in parentType.GetRuntimeMethods())
        {
            if (
                runtimeMethod.Name != memberRefName
                || runtimeMethod.ReturnType != methodSignature.ReturnType
            )
            {
                continue;
            }

            ParameterInfo[] rMParameters = runtimeMethod.GetParameters();
            if (rMParameters.Length != methodSignature.ParameterTypes.Length)
            {
                continue;
            }

            for (int i = 0; i < rMParameters.Length; i++)
            {
                Type rMParamType = rMParameters[i].ParameterType;
                Type? mSigType = Type.GetTypeFromHandle(
                    methodSignature.ParameterTypes[i].TypeHandle
                );
                Debug.Assert(mSigType != null);
                if (rMParamType == mSigType)
                {
                    return runtimeMethod;
                }
            }
        }

        return null;
    }

    private Type? GetTypeFromHandle(MetadataReader reader, EntityHandle handle)
    {
        if (handle.Kind != HandleKind.TypeReference)
        {
            return null;
        }

        TypeReference typeRef = reader.GetTypeReference((TypeReferenceHandle)handle);
        string name = reader.GetString(typeRef.Name);
        string ns = reader.GetString(typeRef.Namespace);
        string fullName = $"{ns}.{name}";

        return _assembly.GetType(fullName) ?? Type.GetType(fullName) ?? null;
    }
}
