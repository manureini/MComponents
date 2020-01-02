using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace MComponents
{
    internal static class FileUtil
    {
        public static async Task SaveAs(IJSRuntime pJsRuntime, string filename, byte[] data)
        {
            await pJsRuntime.InvokeAsync<object>("mcomponents.saveAsFile", filename, Convert.ToBase64String(data));
        }
    }
}
