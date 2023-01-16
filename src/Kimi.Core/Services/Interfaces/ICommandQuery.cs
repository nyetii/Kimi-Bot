using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services.Interfaces
{
    internal interface ICommandQuery
    {
        Task<string[]> GetKeys();
        Task<dynamic?> GetValue(string key);
    }
}
