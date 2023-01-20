using MComponents.Files;

namespace MComponents.ExampleApp.Service
{
    public class UploadedFile : IFile
    {
        public string FileName { get; set; }

        public string Url { get; set; }

        public long Size { get; set; }
    }
}
