namespace CORE.Processor.InstructionData
{
    public enum FlowControlOpCode
    {
        Jmp = 0x00,
        Tjmp = 0x01,
        Fjmp = 0x02,
        Rjmp = 0x03,
        Call = 0x04,
        Int = 0x05,
        Ret = 0x06,
        Iret = 0x07
    }
}
