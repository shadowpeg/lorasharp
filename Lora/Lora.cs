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

	    private long Frequency {get; set;}
	    private int PacketIndex {get; set;}
	    private bool ImplicitHeader {get; set;}

	    private static readonly byte IRQ_TX_DONE_MASK 		= 0x08;
	    private static readonly byte IRQ_PAYLOAD_CRC_ERROR_MASK 	= 0x20;
	    private static readonly byte IRQ_RX_DONE_MASK 		= 0x40;


	    private enum Register {
		    REG_FIFO 			= (byte)0x00,
		    REG_OP_MODE 		= (byte)0x01,
		    REG_FRF_MSB 		= (byte)0x06,
		    REG_FRF_MID 		= (byte)0x07,
		    REG_FRF_LSB 		= (byte)0x08,
		    REG_PA_CONFIG 		= (byte)0x09,
		    REG_OCP 			= (byte)0x0b,
		    REG_LNA 			= (byte)0x0c,
		    REG_FIFO_ADDR_PTR 		= (byte)0x0d,
		    REG_FIFO_TX_BASE_ADDR	= (byte)0x0e,
		    REG_FIFO_RX_BASE_ADDR	= (byte)0x0f,
		    REG_FIFO_RX_CURRENT_ADDR	= (byte)0x10,
		    REG_IRQ_FLAGS		= (byte)0x12,
		    REG_RX_NB_BYTES		= (byte)0x13,
		    REG_PKT_SNR_VALUE		= (byte)0x19,
		    REG_PKT_RSSI_VALUE 		= (byte)0x1a,
		    REG_RSSI_VALUE 		= (byte)0x1b,
		    REG_MODEM_CONFIG_1 		= (byte)0x1d,
		    REG_MODEM_CONFIG_2 		= (byte)0x1e,
		    REG_PREAMBLE_MSB 		= (byte)0x20,
		    REG_PREAMBLE_LSB 		= (byte)0x21,
		    REG_PAYLOAD_LENGTH 		= (byte)0x22,
		    REG_MODEM_CONFIG_3 		= (byte)0x26,
		    REG_FREQ_ERROR_MSB 		= (byte)0x28,
		    REG_FREQ_ERROR_MID 		= (byte)0x29,
		    REG_FREQ_ERROR_LSB 		= (byte)0x2a,
		    REG_RSSI_WIDEBAND 		= (byte)0x2c,
		    REG_DETECTION_OPTIMIZE 	= (byte)0x31,
		    REG_INVERTIQ 		= (byte)0x33,
		    REG_DETECTION_THRESHOLD 	= (byte)0x37,
		    REG_SYNC_WORD 		= (byte)0x39,
		    REG_INVERTIQ2 		= (byte)0x3b,
		    REG_DIO_MAPPING_1 		= (byte)0x40,
		    REG_VERSION 		= (byte)0x42,
		    REG_PA_DAC 			= (byte)0x4d
	    }

	    private enum Mode {
		    MODE_LONG_RANGE_MODE 	= (byte)0x80,
		    MODE_SLEEP 			= (byte)0x00,
		    MODE_STDBY 			= (byte)0x01,
		    MODE_TX 			= (byte)0x03,
		    MODE_RX_CONTINUOUS 		= (byte)0x05,
		    MODE_RX_SINGLE 		= (byte)0x06
	    }

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
		    var verByte = ReadRegister(Register.REG_VERSION);

		    if(verByte != 0x12){
			    throw new Exception("Unable to initialize LoRa module!");
		    }

	    }

	    public void Begin(long frequency){
		    Sleep();
		    SetFrequency(frequency);

		    WriteRegister(Register.REG_FIFO_TX_BASE_ADDR, 0x00);
		    WriteRegister(Register.REG_FIFO_RX_BASE_ADDR, 0x00);

		    WriteRegister(Register.REG_LNA,(byte)(ReadRegister(Register.REG_LNA) | 0x03));

		    WriteRegister(Register.REG_MODEM_CONFIG_3, 0x04);

		    SetTxPower(17);

		    Idle();
	    }

	    public void End(){
		    throw new NotImplementedException();
	    }


	    public int BeginPacket(bool implicitHeader = false){
		    throw new NotImplementedException();
	    }

	    public int EndPacket(bool asnc = false){
		    throw new NotImplementedException();
	    }


	    public int ParsePacket(int size = 0){
		    var packetLength = 0;
		    var irqFlags = (byte)ReadRegister(Register.REG_IRQ_FLAGS);

		    if (size > 0){
			    ImplicitHeaderMode();
			    WriteRegister(Register.REG_PAYLOAD_LENGTH, (byte)size);
		    } else {
			    ExplicitHeaderMode();
		    }
		    WriteRegister(Register.REG_IRQ_FLAGS, irqFlags);

		    if ((irqFlags & IRQ_RX_DONE_MASK) == IRQ_RX_DONE_MASK && (irqFlags & IRQ_PAYLOAD_CRC_ERROR_MASK) == 0) {
			    PacketIndex = 0;

			    if(ImplicitHeader){
				    packetLength = ReadRegister(Register.REG_PAYLOAD_LENGTH);
			    }else{
				    packetLength = ReadRegister(Register.REG_RX_NB_BYTES);
			    }
			    WriteRegister(Register.REG_FIFO_ADDR_PTR, ReadRegister(Register.REG_FIFO_RX_CURRENT_ADDR));
			    Idle();
		    } else if (ReadRegister(Register.REG_OP_MODE) != (byte)((byte)Mode.MODE_LONG_RANGE_MODE | (byte)Mode.MODE_RX_SINGLE)) {
			    WriteRegister(Register.REG_FIFO_ADDR_PTR, 0);

			    WriteRegister(Register.REG_OP_MODE, (byte)((byte)Mode.MODE_LONG_RANGE_MODE | (byte)Mode.MODE_RX_SINGLE));
		    }


		    return packetLength;
	    }

	    public int PacketRssi(){
		    throw new NotImplementedException();
	    }

	    public float PacketSnr(){
		    throw new NotImplementedException();
	    }

	    public long PacketFrequencyError(){
		    throw new NotImplementedException();
	    }


	    public int Rssi(){
		    throw new NotImplementedException();
	    }


	    public int Write(byte _byte){
		    throw new NotImplementedException();
	    }

	    public int Write(byte[] buffer){
		    throw new NotImplementedException();
	    }


	    public bool Available(){
		    return (ReadRegister(Register.REG_RX_NB_BYTES) - PacketIndex) > 0;
	    }

	    public byte Read(){
		    if(!Available()){
			    return (byte)0xFF;
		    }
		    PacketIndex++;

		    return ReadRegister(Register.REG_FIFO);
	    }

	    public int Peak(){
		    throw new NotImplementedException();
	    }

	    public void Flush(){
		    throw new NotImplementedException();
	    }


	    public void Idle(){
		    WriteRegister(Register.REG_OP_MODE, (byte)Mode.MODE_LONG_RANGE_MODE | (byte)Mode.MODE_STDBY);
	    }

	    public void Sleep(){
		    WriteRegister(Register.REG_OP_MODE, (byte)Mode.MODE_LONG_RANGE_MODE | (byte)Mode.MODE_SLEEP);
	    }


	    public void SetTxPower(int level){
		    if(level > 17){
			    level = level > 20 ? 20 : level;

			    level -= 3;
			    WriteRegister(Register.REG_PA_DAC, 0x87);
			    SetOCP(140);
		    }else{
			    level = level < 2 ? 2 : level;
			    WriteRegister(Register.REG_PA_DAC, 0x84);
			    SetOCP(100);
		    }

		    WriteRegister(Register.REG_PA_CONFIG, (byte)(0x80 | (level - 2)));
	    }

	    public void SetFrequency(long frequency){
		    Frequency = frequency;
		    ulong frf = ((ulong)frequency << 19) / 32000000;
		    WriteRegister(Register.REG_FRF_MSB, (byte)(frf >> 16));
		    WriteRegister(Register.REG_FRF_MID, (byte)(frf >> 8));
		    WriteRegister(Register.REG_FRF_LSB, (byte)(frf >> 0));
	    }

	    public void SetSpreadingFactor(int spreadingFactor){
		    throw new NotImplementedException();
	    }

	    public void SetSignalBandWidth(int signalBandwidth){
		    throw new NotImplementedException();
	    }

	    public void SetCodingRate4(int denominator){
		    throw new NotImplementedException();
	    }

	    public void SetPreambleLength(long length){
		    throw new NotImplementedException();
	    }

	    public void SetSyncWord(int syncWord){
		     throw new NotImplementedException();
	    }

	    public void EnableCrc(){
		     throw new NotImplementedException();
	    }

	    public void DisableCrc(){
		     throw new NotImplementedException();
	    }

	    public void EnableInvertIQ(){
		     throw new NotImplementedException();
	    }

	    public void DisableInvertIQ(){
		     throw new NotImplementedException();
	    }

	    public void SetOCP(byte mA){
		    byte ocpTrim = 27;

		    if(mA <= 120){
			    ocpTrim = (byte)((mA - 45) / 5);
		    } else if(mA <= 240){
			    ocpTrim = (byte)((mA + 30) / 10);
		    }
		    WriteRegister(Register.REG_OCP, (byte)(0x20 | (0x1F & ocpTrim)));
	    }

	    public void SetGain(byte gain){
		    throw new NotImplementedException();
	    }

	    public byte Random(){
		    throw new NotImplementedException();
	    }






	    private void ExplicitHeaderMode(){
		     ImplicitHeader = false;
		     WriteRegister(Register.REG_MODEM_CONFIG_1,(byte)(ReadRegister(Register.REG_MODEM_CONFIG_1) & 0xfe));
	    }

	    private void ImplicitHeaderMode(){
		     throw new NotImplementedException();
	    }

	    private void HandleDio0Rise(){
		     throw new NotImplementedException();
	    }

	    private bool IsTransmitting(){
		     throw new NotImplementedException();
	    }

	    
	    private int GetSpreadingFactor(){
		     throw new NotImplementedException();
	    }

	    private long GetSignalBandwidth(){
		     throw new NotImplementedException();
	    }

	    
	    private void SetLDOFlag(){
		     throw new NotImplementedException();
	    }


	    private byte ReadRegister(Register reg){
		    return SingleTransfer((byte)((byte)reg & 0x7F) /*Set the MSB to 0*/, 0x00);
	    }

	    private void WriteRegister(Register reg, byte val){
		     SingleTransfer((byte)((byte)reg | 0x80) /*Set the MSB to 1*/, val);

	    }

	    private byte SingleTransfer(byte address, byte val){
		    var ret = TxRx(new byte[]{address, val});
		    return ret[1];
	    }

	    private static void OnDio0Rise(){
		    throw new NotImplementedException();
	    }



	    private byte[] TxRx(byte[] data){
		    //HexDump(data);
		    data = spi.SPIRxTx(data);
		    //HexDump(data, true);
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
		    if(!rx){
		    	foreach(var c in str){
			    	Console.Write("*");
		    	}
		    	Console.WriteLine();
		    }
		    Console.WriteLine(str);
		    if(rx){
		    	foreach(var c in str){
			    	Console.Write("*");
		    	}
		    	Console.WriteLine();
		    }

	    }
    }
}
