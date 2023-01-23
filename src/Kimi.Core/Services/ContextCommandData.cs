﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kimi.Core.Services.Interfaces;

namespace Kimi.Core.Services
{
    public class ContextCommandData : ICommandQuery
    {
        private readonly SocketSlashCommand _command;
        public ContextCommandData(SocketSlashCommand command)
        {
            _command = command;
        }

        public async Task<string[]> GetKeys()
        {
            string[] curitiba = new string[_command.Data.Options.First().Options.Count];

            int i = 0;
            foreach (var item in _command.Data.Options)
            {
                foreach (var subItem in item.Options)
                {
                    curitiba[i] = subItem.Name;
                    i++;
                }
            }

            return await Task.FromResult(curitiba);
        }

        public async Task<dynamic?> GetValue(string key)
        {
            return await Task.FromResult(_command.Data.Options.First()           // Subcommand
                .Options.FirstOrDefault(x => x.Name == key)?      // Option=
                .Value);
        }
}
}