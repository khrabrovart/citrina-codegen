namespace CitrinaCodeGeneration
{
    public class Program
    {
        private const string TestFieldName = "_testField";
        private const string TestPropertyName = "TestField";

        private const string OtherFieldName = "_otherField";
        private const string OtherPropertyName = "OtherField";

        public static void Main(string[] args)
        {
            var cg = new CodeGenerator();

            var property = cg.NewProperty("Test1", "string");
            var newClass = cg.NewClass("TestClass", null, property);
            var newNamespace = cg.NewNamespace("TestNamespace", newClass);
            cg.NewSourceFile("test.cs", newNamespace, new[] { "System", "System.Text" });
        }
    }
}
