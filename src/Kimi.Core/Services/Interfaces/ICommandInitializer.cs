using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kimi.Core.Services.Interfaces
{
    internal interface ICommandInitializer
    {
        private protected static bool IsRegistered;
        Task Initialize();

        Task CreateCommand();
    }
}
