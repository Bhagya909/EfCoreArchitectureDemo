using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums;

public enum AiSummaryStatus
{
    NotRequested = 1,

    Pending = 2,

    Completed = 3,

    Failed = 4,

    Skipped = 5
}