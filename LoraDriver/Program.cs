using System;
using Lora;
using System.Threading;

namespace LoraDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
	    var l = new LoraCore();
	    l.Begin((long)433E6);

	    for(;;){
		    var packetSize = l.ParsePacket();
		    if(packetSize > 0){
			    //Console.WriteLine(packetSize);
			    while(l.Available()){
				    Console.Write((char)l.Read());
			    }
		    }
		    Thread.Sleep(1);
	    }


	    //Console.WriteLine(frq);
        }
    }
}
