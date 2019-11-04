#region Using directives
using System;
#endregion

namespace MySweeper {
    enum BlockStatus : uint {
        Normal = 0,
        Flipped,
        Exploded,
        Marked,
        WrongMark,
        Question
    }
}