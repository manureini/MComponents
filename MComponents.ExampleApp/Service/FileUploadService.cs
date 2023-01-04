using MComponents.Files;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MComponents.ExampleApp.Service
{
    public class FileUploadService : IFileUploadService
    {
        public async Task<IFile> UploadFile(IBrowserFile pFile, IDictionary<string, string> pAdditionalHeaders, Func<IBrowserFile, long, Task> pOnProgressChanged)
        {
            long size = 0;

            while (size < pFile.Size)
            {
                _ = pOnProgressChanged(pFile, size);
                await Task.Delay(300);

                size += pFile.Size / 10 + 100;
            }

            var file = new UploadedFile()
            {
                FileName = pFile.Name,
            };

            return file;
        }
    }
}
