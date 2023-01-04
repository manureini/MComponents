using MComponents.MForm;
using System.Collections.Generic;

namespace MComponents.Files
{
    public class FileComplexPropertyField : MComplexPropertyField<IFile>
    {
        public string FileInputName { get; set; }

        public string Accept { get; set; }

        public IDictionary<string, string> AdditionalHeaders { get; set; }

        public FileComplexPropertyField()
        {
            Template = (context) => (builder) =>
            {
                builder.OpenComponent<MInputFile>(0);
                builder.AddAttribute(1, nameof(MInputFile.FileInputName), FileInputName);
                builder.AddAttribute(2, nameof(MInputFile.AdditionalHeaders), AdditionalHeaders);
                builder.AddAttribute(3, nameof(MInputFile.Value), context.Value);
                builder.AddAttribute(4, nameof(MInputFile.ValueChanged), context.ValueChanged);
                builder.AddAttribute(5, nameof(MInputFile.ValueExpression), context.ValueExpression);
                builder.AddAttribute(5, nameof(MInputFile.Title), Property);
                builder.AddAttribute(5, nameof(MInputFile.Attributes), Attributes);
                builder.AddAttribute(5, nameof(MInputFile.Accept), Accept);
                builder.CloseComponent();
            };
        }
    }
}
