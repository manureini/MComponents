using Microsoft.JSInterop;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace MComponents
{
    public static class FileUtil
    {
        public static async Task SaveAs(IJSRuntime pJsRuntime, string pFilename, Stream pStream)
        {
            using var streamRef = new DotNetStreamReference(stream: pStream);
            await pJsRuntime.InvokeVoidAsync("mcomponents.saveAsFile", pFilename, streamRef);
        }
    }
}
