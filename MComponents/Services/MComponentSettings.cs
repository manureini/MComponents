﻿using MComponents.MForm;
using MComponents.MGrid;
using MComponents.MToaster;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MComponents.Services
{
    public class MComponentSettings
    {
        public static readonly CultureInfo[] AllSupportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("de-DE"), new CultureInfo("fr-FR") };

        public bool UseDeleteConfirmationWithAlert { get; set; }

        public bool RegisterResourceLocalizer { get; set; }

        public bool RegisterStringLocalizer { get; set; }

        public bool RegisterNavigation { get; set; }

        public bool RegisterTimezoneService { get; set; }

        public CultureInfo[] SupportedCultures { get; set; } = AllSupportedCultures;

        public MGridSettings MGridSettings => MGridSettings.Instance;

        public ToasterConfiguration ToasterConfiguration { get; set; } = new ToasterConfiguration();

        public Func<IServiceProvider, string, Task> EnsureAssemblyIsLoaded { get; set; }

        public RenderFragment<MComplexPropertyFieldContext<string>> CustomTextAreaComponent { get; set; }
    }
}
