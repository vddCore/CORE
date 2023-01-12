using System;

namespace CORE.Processor.Executive
{
    [Flags]
    public enum OperandFlags
    {
        SourceIsPointer = 0x01,
        DestinationIsPointer = 0x02
    }
}
