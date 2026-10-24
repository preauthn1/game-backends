using System;

namespace target.Models.ViewModels
{
    public interface IBetStatistic
    {
        int TotalRound { get; set; }
        decimal BetAmount { get; set; }
        decimal ValidAmount { get; set; }
        decimal PayoutAmount { get; set; }
        decimal WinLossAmount { get; set; }

    }

}
