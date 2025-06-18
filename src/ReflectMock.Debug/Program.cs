namespace ReflectMock.Debug;

internal class Program
{
    static void Main(string[] args)
    {
        Type mockedType = new MockTypeBuilder()
            .Name("Foo")
            .AccessModifiers(AccessModifiers.Public)
            .WithField(new MockFieldBuilder().Name("x").OfType(typeof(float)).InitOnly())
            .Build();

    }
}
