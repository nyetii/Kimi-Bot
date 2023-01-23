using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Remoting;
using System.Reflection.Metadata;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kimi.GPT2
{
    public class CacheArgs : EventArgs
    {
        public List<string> GenerationCache { get; set; }
        
        public CacheArgs(List<string> generationCache)
        {
            GenerationCache = generationCache;
        }
    }

    public class Cache
    {
        public static int Count { get; private set; }
        private static readonly List<string> CacheList = new();
        private static readonly CacheArgs Args = new(CacheList);
        private static readonly string Path = IModelData.Path;

        public event EventHandler<CacheArgs>? CacheUpdate;

        public static async Task LoadCacheFile()
        {
            if (!File.Exists($@"{Path}\cache.kimi"))
                return;

            Args.GenerationCache = (await File.ReadAllLinesAsync($@"{Path}\cache.kimi")).ToList();
            Args.GenerationCache.RemoveAll(string.IsNullOrWhiteSpace);
            Count = Args.GenerationCache.Count;
        }

        public async Task<string?> GetFromCache()
        {
            var cache = Args.GenerationCache;
            
            string? content = cache.FirstOrDefault();

            if(content != null)
                cache.Remove(content);

            OnRaiseCacheUpdate(new CacheArgs(cache));
            return await Task.FromResult(content);
        }

        public async Task<string> NewCache()
        {
            OnRaiseCacheUpdate(new CacheArgs(Args.GenerationCache));
            return await Task.FromResult("Sorry, this command is temporarily not available. Try again in a few minutes");
        }

        protected virtual void OnRaiseCacheUpdate(CacheArgs args)
        {

            if (args.GenerationCache.Count is 0)
            {
                Model model = new();
                for (int i = 0; i < 20; i++)
                {
                    var a = model.Generate();
                    if(a.Result.Contains("Traceback"))
                        Console.WriteLine($"{i} - Failed generation.");
                    else
                    {
                        args.GenerationCache.Add(a.Result);
                        Console.WriteLine($"{i} - Generated!");
                    }
                }
            }

            Count = args.GenerationCache.Count;
            File.WriteAllLines($@"{Path}\cache.kimi", args.GenerationCache);
            CacheUpdate?.Invoke(this, args);
        }
    }
}
