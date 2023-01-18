using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services.Interfaces
{
    internal interface ICommandQuery
    {
        /// <summary>
        /// Gets all the keys of the determined scope.
        /// </summary>
        /// <returns>An array of strings, being a specific key each.</returns>
        Task<string[]> GetKeys();

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single dynamic object symbolizing the content of the key.</returns>
        Task<dynamic?> GetValue(string key);
    }
}
