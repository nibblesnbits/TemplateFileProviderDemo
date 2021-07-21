using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TemplateFileProviderDemo.Providers {
    /// <summary>
    /// Provides methods for replacing tokens in streams
    /// </summary>
    public static class StreamTokenReplacer {

        /// <summary>
        /// Replaces the tokens in the provided stream
        /// </summary>
        /// <param name="replacements">Object containing values to replace in template</param>
        /// <param name="inputStream">The input stream</param>
        public static Stream GetReplacementStream(object replacements, Stream inputStream) {
            if (replacements is null) {
                throw new ArgumentNullException(nameof(replacements));
            }

            if (inputStream is null) {
                throw new ArgumentNullException(nameof(inputStream));
            }

            using var reader = new StreamReader(inputStream);
            var writeStream = new MemoryStream();
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                var replacement = Interpolate(line, replacements);
                var bytes = reader.CurrentEncoding.GetBytes(replacement);
                if (bytes.Length > 0) {
                    writeStream.Write(bytes);
                }
            }
            writeStream.Seek(0, SeekOrigin.Begin);
            return writeStream;
        }

        private static readonly Regex _mustacheReplace = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

        private static string Interpolate(string input, object replacements) {
            return _mustacheReplace.Replace(input, match => {
                // replace with the value found in the property
                return replacements.GetType().GetProperty(match.Groups[1].Value)?.GetValue(replacements)?.ToString()
                // or just leave it alone
                ?? match.Groups[0].Value;
            });
        }
    }
}
