using System;
namespace Test.HelloWorld
{
    public class A
    {
        public static void F()
        {
            F();
            Console.WriteLine('X');
            new A().H("");
        }

        void H(string s)
        {
            Console.WriteLine(s);
        }
    }
}
