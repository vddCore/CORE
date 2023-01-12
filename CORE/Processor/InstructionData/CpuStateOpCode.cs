namespace CORE.Processor.InstructionData
{
    public enum CpuStateOpCode
    {
        Hlt = 0x00,
        Cli = 0x01,
        Sti = 0x02,
        Rst = 0x03,
        SetVe = 0x04
    }
}
