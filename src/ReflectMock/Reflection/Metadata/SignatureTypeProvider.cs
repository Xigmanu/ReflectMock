using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;

namespace ReflectMock.Reflection.Metadata;

internal sealed class SignatureTypeProvider : ISignatureTypeProvider<Type, Type[]?>
{
    private readonly Assembly _assembly;

    public SignatureTypeProvider(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Type GetArrayType(Type elementType, ArrayShape shape)
    {
        return elementType.MakeArrayType(shape.Rank);
    }

    public Type GetByReferenceType(Type elementType)
    {
        return elementType.MakeByRefType();
    }

    public Type GetFunctionPointerType(MethodSignature<Type> signature)
    {
        throw new NotSupportedException("Function pointers are not supported");
    }

    public Type GetGenericInstantiation(Type genericType, ImmutableArray<Type> typeArguments)
    {
        return genericType.MakeGenericType([.. typeArguments]);
    }

    public Type GetGenericMethodParameter(Type[]? genericContext, int index)
    {
        return GetGenericTypeParameter(genericContext, index);
    }

    public Type GetGenericTypeParameter(Type[]? genericContext, int index)
    {
        if (genericContext == null || index >= genericContext.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(genericContext),
                "Generic method parameter index out of range"
            );
        }
        return genericContext[index];
    }

    public Type GetModifiedType(Type modifier, Type unmodifiedType, bool isRequired)
    {
        return unmodifiedType;
    }

    public Type GetPinnedType(Type elementType)
    {
        return elementType;
    }

    public Type GetPointerType(Type elementType)
    {
        return elementType.MakePointerType();
    }

    public Type GetPrimitiveType(PrimitiveTypeCode typeCode)
    {
        return typeCode switch
        {
            PrimitiveTypeCode.Boolean => typeof(bool),
            PrimitiveTypeCode.Char => typeof(char),
            PrimitiveTypeCode.SByte => typeof(sbyte),
            PrimitiveTypeCode.Byte => typeof(byte),
            PrimitiveTypeCode.Int16 => typeof(short),
            PrimitiveTypeCode.UInt16 => typeof(ushort),
            PrimitiveTypeCode.Int32 => typeof(int),
            PrimitiveTypeCode.UInt32 => typeof(uint),
            PrimitiveTypeCode.Int64 => typeof(long),
            PrimitiveTypeCode.UInt64 => typeof(ulong),
            PrimitiveTypeCode.Single => typeof(float),
            PrimitiveTypeCode.Double => typeof(double),
            PrimitiveTypeCode.String => typeof(string),
            PrimitiveTypeCode.Object => typeof(object),
            PrimitiveTypeCode.IntPtr => typeof(IntPtr),
            PrimitiveTypeCode.UIntPtr => typeof(UIntPtr),
            PrimitiveTypeCode.Void => typeof(void),
            _ => throw new NotSupportedException($"Unknown primitive type: {typeCode}"),
        };
    }

    public Type GetSZArrayType(Type elementType)
    {
        return elementType.MakeArrayType();
    }

    public Type GetTypeFromDefinition(
        MetadataReader reader,
        TypeDefinitionHandle handle,
        byte rawTypeKind
    )
    {
        TypeDefinition typeDef = reader.GetTypeDefinition(handle);
        string typeName = reader.GetString(typeDef.Name);
        string ns = reader.GetString(typeDef.Namespace);
        return ResolveType($"{ns}.{typeName}");
    }

    public Type GetTypeFromReference(
        MetadataReader reader,
        TypeReferenceHandle handle,
        byte rawTypeKind
    )
    {
        TypeReference typeRef = reader.GetTypeReference(handle);
        string typeName = reader.GetString(typeRef.Name);
        string ns = reader.GetString(typeRef.Namespace);
        return ResolveType($"{ns}.{typeName}");
    }

    public Type GetTypeFromSpecification(
        MetadataReader reader,
        Type[]? genericContext,
        TypeSpecificationHandle handle,
        byte rawTypeKind
    )
    {
        TypeSpecification typeSpec = reader.GetTypeSpecification(handle);
        return typeSpec.DecodeSignature(this, genericContext);
    }

    private Type ResolveType(string fullName)
    {
        return _assembly.GetType(fullName)
            ?? Type.GetType(fullName)
            ?? throw new TypeLoadException($"Could not resolve type: {fullName}");
    }
}
