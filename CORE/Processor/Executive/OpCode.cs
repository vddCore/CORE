namespace CORE.Processor.Executive
{
    public enum OpCode
    {
        Mov = 0x00, // Move
        Ari = 0x01, // Arithmetic [ADD MUL SUB DIV]
        Tst = 0x02, // Test [>= <= == < > !=]
        Flo = 0x03, // Flow Control [JMP TJMP FJMP CALL RET INT IRET]
        Prt = 0x04, // Port operation [IN OUT]
        Log = 0x05, // Logical operation [AND OR XOR NOT]
        Sta = 0x06, // Stack operation [PUSH PUSHW PUSHD POP POPW POPD]
        Cpu = 0x07, // CPU state management [HLT CLI STI RST SETVE]
    }
}
