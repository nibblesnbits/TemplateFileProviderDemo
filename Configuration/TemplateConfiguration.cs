using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace TemplateFileProviderDemo.Configuration {

    public class TemplateConfiguration {
        private readonly IOptions<TemplateDefaults> _defaults;

        public TemplateConfiguration(IOptions<TemplateDefaults> defaults) {
            _defaults = defaults;
        }

        public bool ShouldProcess(string subpath) {
            return Path.GetExtension(subpath).Equals(".html", StringComparison.OrdinalIgnoreCase);
        }
        public TemplateDefaults DefaultValues => _defaults.Value;
    }

    public record TemplateDefaults {
        public string AppName { get; set; }
        public string Host { get; set; }
    }
}
