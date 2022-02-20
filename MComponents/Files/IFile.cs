using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public interface IFile
    {
        public string FileName { get; }

        public string Url { get; }
    }
}
