using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public interface IFileUploadService
    {
        public Task<IFile> UploadFile(IBrowserFile pFile, IDictionary<string, string> pAdditionalHeaders, Action<IBrowserFile, long> pOnProgressChanged, CancellationToken pCancellationToken);
    }
}
