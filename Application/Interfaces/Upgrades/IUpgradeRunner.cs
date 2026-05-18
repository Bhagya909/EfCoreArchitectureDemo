using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Upgrades
{
    public interface IUpgradeRunner
    {
        Task RunUpgradesAsync();
    }
}
