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
            Console.WriteLine("Waiting...");
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
			    var received = sbx.ToString();
			    sbx.AppendLine($"RSSI : {l.PacketRssi()}");
			    Console.WriteLine(sbx);
			    Console.WriteLine("Sending Back >> ");
			    l.BeginPacket();
			    var written = l.WriteString(received);
			    Console.WriteLine($"Written {written} bytes");
			    l.EndPacket();
			    Console.WriteLine();

		    }
		    Thread.Sleep(1);
	    }


	    //Console.WriteLine(frq);
        }
    }
}
