using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                builder.OpenComponent<FileInputComponent>(0);
                builder.AddAttribute(1, nameof(FileInputComponent.FileInputName), FileInputName);
                builder.AddAttribute(2, nameof(FileInputComponent.AdditionalHeaders), AdditionalHeaders);
                builder.AddAttribute(3, nameof(FileInputComponent.Value), context.Value);
                builder.AddAttribute(4, nameof(FileInputComponent.ValueChanged), context.ValueChanged);
                builder.AddAttribute(5, nameof(FileInputComponent.ValueExpression), context.ValueExpression);
                builder.AddAttribute(5, nameof(FileInputComponent.Title), Property);
                builder.AddAttribute(5, nameof(FileInputComponent.Attributes), Attributes);
                builder.AddAttribute(5, nameof(FileInputComponent.Accept), Accept);
                builder.CloseComponent();
            };
        }
    }
}
