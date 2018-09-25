using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace texted_tool
{
    [Command(Description = "Text file editor tool.")]
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "The name of source text file.")]
        [Required]
        public string SourceFile { get; }

        [Argument(1, Description = "The name of destination text file.")]
        [Required]
        public string DestFile { get; }
        [Argument(2, Description = "A RegEx pattern that needs to be replaced in text file.")]
        [Required]
        public string Pattern { get; }

        [Argument(3, Description = "String Value")]
        [Required]
        public string Value { get; }

        [Option(Description = "The RegEx pattern. Only matched named group 'value' of Value will be used to replace.", LongName = "extract", ShortName = "e")]
        public string Extract { get; }

        [Option(Description = "This option indicate that tool chould update target file only if changed (slow).", LongName = "if-changed", ShortName = "if")]
        public bool OnlyIfChanged { get; }

        private int OnExecute()
        {
            try
            {
                using (var reader = File.OpenText(SourceFile))
                {
                    MemoryStream stream = null;
                    using (var writer = OnlyIfChanged && File.Exists(DestFile) ? new StreamWriter(stream = new MemoryStream(), encoding: Encoding.UTF8, bufferSize: 256, leaveOpen: true) : File.CreateText(DestFile))
                    {
                        Regex rx = new Regex(Pattern);
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var matches = rx.Matches(line);
                            if (matches.Count > 0)
                            {
                                int pos = 0;
                                foreach (Match m in matches)
                                {
                                    writer.Write(line.Substring(pos, m.Index));
                                    if (String.IsNullOrEmpty(Extract))
                                        writer.Write(Value);
                                    else
                                    {
                                        Regex extract = new Regex(Extract);
                                        var match = extract.Match(Value);
                                        if (match.Success && match.Groups["value"].Success)
                                        {
                                            writer.Write(match.Groups["value"].Value);
                                        }
                                        else
                                        {
                                            throw new ApplicationException("Specified pattern can't be extracted from Value.");
                                        }

                                    }
                                    pos += (m.Index - pos + m.Length);
                                }
                                writer.WriteLine(pos < line.Length ? line.Substring(pos) : "");
                            }
                            else
                                writer.WriteLine(line);
                        }
                    }
                    if (stream != null)
                    {
                        stream.Position = 0;
                        bool equals = true;
                        using (var destStream = File.OpenRead(DestFile))
                        {
                            int data;
                            if (stream.Length == destStream.Length)
                            {
                                while ((data = destStream.ReadByte()) >= 0)
                                    if (data != stream.ReadByte())
                                    {
                                        equals = false;
                                        break;
                                    }
                            }
                            else
                                equals = false;
                        }
                        if (!equals)
                        {
                            stream.Position = 0;
                            using (var destStream = File.Create(DestFile))
                                stream.CopyTo(destStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            return 0;
        }
    }
}
