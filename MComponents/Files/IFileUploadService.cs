using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public interface IFileUploadService
    {
        public Task<IFile> UploadFile(IBrowserFile pFile, IDictionary<string, string> pAdditionalHeaders, Func<IBrowserFile, long, Task> pOnProgressChanged);
    }
}
