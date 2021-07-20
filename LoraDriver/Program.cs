using System;
using Lora;

namespace LoraDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
	    var l = new LoraCore();
	    l.Begin(23000);
        }
    }
}
