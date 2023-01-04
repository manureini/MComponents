using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public class FileProgressInfo
    {
        public IBrowserFile BrowserFile;
        public int Progress;

        public FileProgressInfo(IBrowserFile pBrowserFile, int pProgress)
        {
            BrowserFile = pBrowserFile;
            Progress = pProgress;
        }
    }
}
