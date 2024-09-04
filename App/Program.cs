App.Test.Demo();

namespace App
{
    public partial class Test
    {
        public static void Demo()
        {
            Console.WriteLine("Hello, World!");
            TestMethod();
        }

        [Generator.Demo]
        static partial void TestMethod();
    }
}