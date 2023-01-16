using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Kimi.Core.Services.Interfaces;

namespace Kimi.Core.Services
{
    internal class CategoryQuery : ICommandQuery
    {
        protected readonly string? Key = null;
        protected readonly Dictionary<string, dynamic> Commands;

        public CategoryQuery()
        {
            Commands = Info.CommandInfo;
        }

        public CategoryQuery(string key)
        {
            Key = key;
            Commands = Info.CommandInfo;
        }

        public virtual async Task<string[]> GetKeys()
        {
            return await Task.FromResult(Commands.Keys.ToArray());
        }

        public virtual async Task<dynamic?> GetValue(string key)
        {
            return await Task.FromResult(Commands[key]);
        }
    }

    internal class SubcategoryQuery : CategoryQuery
    {
        protected new readonly string? Key = null;

        public SubcategoryQuery()
        {
        }

        public SubcategoryQuery(string key)
        {
            Key = key;
        }

        public override async Task<string[]> GetKeys()
        {
            foreach (var item in base.Commands)
            {

                Type t = item.Value.GetType();
                if (t == typeof(Dictionary<string, dynamic>))
                {
                    Dictionary<string, dynamic> subDictionary = item.Value;
                    foreach (var kv in subDictionary)
                    {
                        return await Task.FromResult(subDictionary.Keys.ToArray());
                    }
                }
            }
            return await Task.FromResult(Commands.Keys.ToArray());
        }

        public override async Task<dynamic?> GetValue(string key)
        {
            foreach (var item in base.Commands)
            {
                Type t = item.Value.GetType();
                if (t == typeof(Dictionary<string, dynamic>))
                {
                    Dictionary<string, dynamic> subDictionary = item.Value;
                    foreach (var kv in subDictionary)
                    {
                        if (subDictionary.ContainsKey(key))
                            return await Task.FromResult(subDictionary[key]);
                    }
                }
            }
            return await Task.FromResult(Commands[key]);
        }
    }

    internal class ParameterQuery : CategoryQuery
    {
        protected new readonly string? Key = null;

        public ParameterQuery()
        {
        }

        public ParameterQuery(string key)
        {
            Key = key;
        }

        public override async Task<string[]> GetKeys()
        {
            foreach (var item in base.Commands)
            {
                //string k = null;
                //dynamic val = null;
                //item.Deconstruct(out k, out val);

                Type t = item.Value.GetType();
                if (t == typeof(Dictionary<string, dynamic>))
                {
                    Dictionary<string, dynamic> subDictionary = item.Value;
                    foreach (var subItem in subDictionary)
                    {
                        t = subItem.Value.GetType();

                        if (t == typeof(Dictionary<string, string>))
                        {
                            Dictionary<string, string> paramDictionary = subItem.Value;
                            return await Task.FromResult(paramDictionary.Keys.ToArray());
                        }
                    }
                }
            }
            return await Task.FromResult(Commands.Keys.ToArray());
        }

        public override async Task<dynamic?> GetValue(string key)
        {
            foreach (var item in base.Commands)
            {

                Type t = item.Value.GetType();
                if (t == typeof(Dictionary<string, dynamic>))
                {
                    Dictionary<string, dynamic> subDictionary = item.Value;
                    foreach (var subItem in subDictionary)
                    {
                        t = subItem.Value.GetType();

                        if (t == typeof(Dictionary<string, string>))
                        {
                            Dictionary<string, string> paramDictionary = subItem.Value;
                            if(paramDictionary.ContainsKey(key))
                                return await Task.FromResult(paramDictionary[key]);
                        }
                    }
                }
            }
            return await Task.FromResult(Commands[key]);
        }
    }
}
