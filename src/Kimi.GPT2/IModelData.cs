using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.GPT2
{
    internal interface IModelData
    {
        static string Path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Kimi\GPT2";
        Task DownloadPythonFile();
        Task DownloadModel();
    }
}
