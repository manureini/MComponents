using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public interface IFileUploadService
    {
        public Task<IList<IFile>> UploadFiles(IReadOnlyList<IBrowserFile> pFiles, IDictionary<string, string> pAdditionalHeaders, Action<IBrowserFile, float> pOnProgressChanged);
    }
}
