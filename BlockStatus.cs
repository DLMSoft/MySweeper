#region Using directives
using System;
#endregion

namespace DLMSoft.MySweeper {
    enum BlockStatus : uint {
        Normal = 0,
        Flipped,
        Exploded,
        Marked,
        WrongMark,
        Question
    }
}