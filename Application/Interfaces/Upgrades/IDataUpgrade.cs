using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Upgrades
{
    public interface IDataUpgrade
    {
        string Name { get; }

        Task ExecuteAsync();
    }
}
