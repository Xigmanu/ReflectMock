namespace ReflectMock.Debug;

internal class Program
{
    static void Main(string[] args)
    {
        Type mockedType = ReflectMock
            .Struct()
            .Name("Vector2")
            .Public()
            .WithField((f) => f.Name("x").OfType(typeof(float)).InitOnly().AutoProperty())
            .WithField((f) => f.Name("y").OfType(typeof(float)).InitOnly().AutoProperty())
            .Build();
    }
}
