using System.Reflection;
using System.Reflection.Emit;

namespace ReflectMock.Reflection;

internal sealed class MockTypeGenerator
{
    private readonly Dictionary<MockFieldInfo, FieldBuilder> _nameFieldMap;
    private readonly MockTypeInfo _typeInfo;

    public MockTypeGenerator(MockTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
        _nameFieldMap = [];
    }

    public Type CreateType()
    {
        AssemblyName asmName = new("Mock_Asm");
        AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(
            asmName,
            AssemblyBuilderAccess.Run
        );
        ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name ?? "Mock_Mod");

        TypeBuilder typeBuilder = moduleBuilder.DefineType(_typeInfo.Name, TypeAttributes.Class); // TODO

        DefineFields(typeBuilder);
        DefineCtor(typeBuilder);

        return typeBuilder.CreateType();
    }

    private void DefineCtor(TypeBuilder typeBuilder)
    {
        FieldBuilder[] initOnlyFields =
        [
            .. _nameFieldMap.Where(keyVal => keyVal.Key.IsInitOnly).Select(keyVal => keyVal.Value),
        ];
        ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [.. initOnlyFields.Select(fieldBuilder => fieldBuilder.FieldType)]
        );

        ILGenerator ctorIL = ctorBuilder.GetILGenerator();
        ctorIL.Emit(OpCodes.Ldarg_0);
        ConstructorInfo? objCtor = typeof(object).GetConstructor(Type.EmptyTypes);
        ctorIL.Emit(OpCodes.Call, objCtor!);
        ctorIL.Emit(OpCodes.Ldarg_0);

        for (int i = 0; i < initOnlyFields.Length; i++)
        {
            ctorIL.Emit(OpCodes.Ldarg_S, i);
            ctorIL.Emit(OpCodes.Stfld, initOnlyFields[i]);
        }
        ctorIL.Emit(OpCodes.Ret);
    }

    private void DefineFields(TypeBuilder typeBuilder)
    {
        foreach (MockFieldInfo fieldInfo in _typeInfo.Fields)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField(
                fieldInfo.Name,
                fieldInfo.Type,
                FieldAttributes.InitOnly
            ); // TODO
            _nameFieldMap[fieldInfo] = fieldBuilder;
        }
    }
}
