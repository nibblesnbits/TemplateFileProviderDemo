using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using TemplateFileProviderDemo.Configuration;

namespace TemplateFileProviderDemo.Providers {
    /// <summary>
    /// Custom <see cref="IFileProvider"/> for mustache-style templates
    /// </summary>
    public class TemplateFileProvider : IFileProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileProvider _parent;
        private readonly TemplateConfiguration _options;

        /// <summary>
        /// Initialize a new instance of the <see cref="TemplateFileProvider"/> class.
        /// </summary>
        /// <param name="parent">Parent <see cref="IFileProvider"/> for providing default behavior</param>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/> for accessing the current request</param>
        /// <param name="options"></param>
        public TemplateFileProvider(IFileProvider parent, IHttpContextAccessor httpContextAccessor, TemplateConfiguration options) {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the directory.</param>
        /// <returns>Returns the contents of the directory.</returns>
        public IDirectoryContents GetDirectoryContents(string subpath) {
            return _parent.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath) {
            if (!_options.ShouldProcess(subpath)) {
                return _parent.GetFileInfo(subpath);
            }
            var realInfo = _parent.GetFileInfo(subpath);
            if (!realInfo.Exists) {
                return new NotFoundFileInfo(realInfo.Name);
            }

            // grab some request-specific data for the template
            var context = _httpContextAccessor.HttpContext;
            var host = context.Request.Host.Host;
            if (host.Contains("localhost")) {
                host = $"{host}:{context.Request.Host.Port}";
            }

            return new ReplacementFileInfo(realInfo, _options.DefaultValues with {
                Host = $"{context.Request.Scheme}{Uri.SchemeDelimiter}{host}"
            });
        }

        public IChangeToken Watch(string filter) {
            return _parent.Watch(filter);
        }


        private class ReplacementFileInfo : IFileInfo {
            private readonly IFileInfo _realInfo;
            private readonly Stream _stream;

            public ReplacementFileInfo(IFileInfo realInfo, TemplateDefaults replacements) {
                if (replacements is null) {
                    throw new ArgumentNullException(nameof(replacements));
                }

                _realInfo = realInfo ?? throw new ArgumentNullException(nameof(realInfo));
                using var inputStream = _realInfo.CreateReadStream();
                _stream = StreamTokenReplacer.GetReplacementStream(replacements, inputStream);
            }

            public bool Exists => _realInfo.Exists;

            public long Length => _stream.Length;

            /// <remarks>
            /// You must return <c>null</c> here if modifying the return stream.
            /// See https://github.com/dotnet/aspnetcore/issues/34575#issuecomment-883853484
            /// </remarks>
            public string PhysicalPath => null;

            public string Name => _realInfo.Name;

            public DateTimeOffset LastModified => _realInfo.LastModified;

            public bool IsDirectory => _realInfo.IsDirectory;

            public Stream CreateReadStream() {
                return _stream;
            }
        }
    }
}
