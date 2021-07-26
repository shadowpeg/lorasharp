using System;
using Lora;
using System.Threading;
using System.Text;

namespace LoraDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
	    var l = new LoraCore();
	    l.Begin((long)433E6);
	    var sbx = new StringBuilder();
	    for(;;){
		    var packetSize = l.ParsePacket();
		    sbx.Clear();
		    if(packetSize > 0){
			    //Console.WriteLine(packetSize);
			    while(l.Available()){
				    sbx.Append((char)l.Read());
			    }
			    sbx.AppendLine($"RSSI : {l.PacketRssi()}");
			    Console.WriteLine(sbx);

		    }
		    Thread.Sleep(1);
	    }


	    //Console.WriteLine(frq);
        }
    }
}
