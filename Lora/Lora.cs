using System;
using WPISPI;
using WPICore;
using System.Threading;
using System.Text;

namespace Lora
{
    public class LoraCore
    {
	    private SPI spi;

	    public LoraCore(){
		    //general initialization
		    Core.WiringPiSetup();
		    Core.PinMode(5, PinMode.OUTPUT);

		    //perform reset
		    Core.DigitalWrite(5, PinState.LOW);
		    Thread.Sleep(10); // should delay for 10 ms
		    Core.DigitalWrite(5, PinState.HIGH);
		    Thread.Sleep(10); // delay for 10 ms

		    //init SPI
		    spi = new(0, 1000000);

		    //check version as per refereing lib
		    var verByte = TxRx(new byte[]{0x42, 0x00});

		    if(verByte[1] != 0x12){
			    throw new Exception("Unable to initialize LoRa module!");
		    }

	    }


	    private byte[] TxRx(byte[] data){
		    HexDump(data);
		    data = spi.SPIRxTx(data);
		    HexDump(data, true);
		    return data;
	    }

	    private void HexDump(byte[] data, bool rx = false){
		    var sb = new StringBuilder();
		    if(rx) {
			    sb.Append(" << ");
		    }else{
			    sb.Append(" >> ");
		    }
		    for(int i = 0; i < data.Length; i++){
			    sb.Append(data[i].ToString("X2"));
			    sb.Append(" ");
		    }
		    var str = sb.ToString().Trim();
		    foreach(var c in str){
			    Console.Write("*");
		    }
		    Console.WriteLine();
		    Console.WriteLine(str);
		    foreach(var c in str){
			    Console.Write("*");
		    }
		    Console.WriteLine();

	    }
    }
}
