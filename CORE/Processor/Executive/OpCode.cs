namespace CORE.Processor.Executive
{
    public enum OpCode
    {
        Mov = 0x00, // Move
        Ari = 0x01, // Arithmetic [ADD MUL SUB DIV ROR ROL SHR SHL]
        Tst = 0x02, // Test [>= <= == < > !=]
        Flo = 0x03, // Flow Control [JMP TJMP FJMP CALL RET INT IRET]
        Log = 0x04, // Logical operation [AND OR XOR NOT]
        Sta = 0x05, // Stack operation [PUSH POP]
        Cpu = 0x06, // CPU state management [HLT CLI STI RST SETVE]
    }
}
