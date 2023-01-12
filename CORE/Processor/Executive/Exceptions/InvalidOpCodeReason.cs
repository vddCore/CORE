namespace CORE.Processor.Executive.Exceptions
{
    public enum InvalidOpCodeReason
    {
        CoreOpCode = 0x00,
        ArithmeticOpCode = 0x01,
        TestOpCode = 0x02,
        FlowControlOpCode = 0x03,
        LogicOpCode = 0x04,
        StackOpCode = 0x05,
        CpuStateOpCode = 0x06
    }
}
