using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services.Interfaces
{
    internal interface ICommandInitializer
    {
        public Task Initialize();
    }
}
