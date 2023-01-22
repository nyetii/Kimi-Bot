using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Kimi.GPT2
{
    internal class ModelData : IModelData
    {
        private readonly string _path;

        public ModelData()
        {
            _path = IModelData.Path;
        }
        public async Task DownloadPythonFile()
        {
            HttpClient client = new();
            await File.WriteAllTextAsync($@"{_path}\generate.py",
                await client.GetStringAsync(@"https://thingsfrom.net/wp-content/uploads/2023/01/generate.txt"));

            await Task.CompletedTask;
        }

        public async Task DownloadModel()
        {
            try
            {
                Directory.CreateDirectory($@"{_path}\model");

                if(Directory.GetFiles($@"{_path}\model").Length == 0)
                    Repository.Clone("https://huggingface.co/netty/monark-gpt2", $@"{_path}\model");

                HttpClient client = new();
                var response =
                    await client.GetAsync(@"https://huggingface.co/netty/monark-gpt2/resolve/main/pytorch_model.bin");

                using (var stream = new FileStream($@"{_path}\model\pytorch_model.bin", FileMode.Create))
                {
                    await response.Content.CopyToAsync(stream);
                }

                using (var stream = new FileStream($@"{_path}\model\training_args.bin", FileMode.Create))
                {
                    response = await client.GetAsync(
                        @"https://huggingface.co/netty/monark-gpt2/resolve/main/training_args.bin");
                    await response.Content.CopyToAsync(stream);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
