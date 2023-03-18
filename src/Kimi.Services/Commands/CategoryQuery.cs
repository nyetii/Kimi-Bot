using Kimi.Services.Commands.Interfaces;
using Kimi.Services.Core;

namespace Kimi.Services.Commands
{
    public class CategoryQuery : ICommandQuery
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
                    return await Task.FromResult(subDictionary.Keys.ToArray());

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
                    if (subDictionary.TryGetValue(key, out dynamic? value))
                        return await Task.FromResult(value);
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
                            if (paramDictionary.TryGetValue(key, out string? value))
                                return await Task.FromResult(value);
                        }
                    }
                }
            }
            return await Task.FromResult(Commands[key]);
        }
    }
}
