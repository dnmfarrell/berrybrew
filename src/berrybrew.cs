using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Berrybrew
{
    public class Berrybrew
    {
        static void Main(string[] args)
        {

            switch(args[0])
            {
                case "install":
                    List<StrawberryPerl> perls = GatherPerls();
                    StrawberryPerl perl = ResolveVersion(perls, args[1]);
                    string archive_path = Fetch(perl);
                    Extract(perl, archive_path);
                    break;
                    
                case "available":
                    Available();
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        internal static string Fetch (StrawberryPerl perl)
        {
            WebClient webClient = new WebClient();
            string tempdir = GetTempDirectory();
            string archive_path = tempdir + "/" + perl.ArchiveName;
            Console.WriteLine("Downloading " + perl.Url + " to " + archive_path);
            webClient.DownloadFile(perl.Url, archive_path);
            return archive_path;
        }

        internal static StrawberryPerl ResolveVersion (List <StrawberryPerl> perls, string version_to_resolve)
        {
            foreach (StrawberryPerl perl in perls)
            {
                if (perl.Name == version_to_resolve)
                    return perl;
            }
            throw new ArgumentException("Unknown version: " + version_to_resolve);
        }

        internal static void Extract (StrawberryPerl perl, string archive_path)
        {
            if (File.Exists(archive_path))
            {
                Console.WriteLine("Extracting " + archive_path);
                ExtractZip(archive_path, perl.InstallPath);
            }

        }

        internal static void Switch ()
        {

        }

        internal static void Available ()
        {
            List<StrawberryPerl> perls = GatherPerls();
            foreach (StrawberryPerl perl in perls)
            {
                Console.WriteLine("\nThe following Strawberry Perls are available:\n");
                
                string name = perl.Name;
                if (Directory.Exists(perl.InstallPath))
                    Console.WriteLine("\t" + name + " [installed]");
                    
                else
                    Console.WriteLine(name);
            }
        }

        internal static void PrintHelp()
        {
            Console.WriteLine(@"
berrybrew.exe <command> [option]

    available   List available Strawberry Perl versions and which are installed
    install     Download, extract and install a Strawberry Perl
    switch      Switch to use a different Strawberry Perl
    ");

        }

        internal static string GetTempDirectory()
        {
            string path;
            do {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }
        
        // From https://github.com/icsharpcode/SharpZipLib
        internal static void ExtractZip(string archive_path, string outFolder)
        {
            ZipFile zf = null;
            try {
                FileStream fs = File.OpenRead(archive_path);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf) {
                    if (!zipEntry.IsFile) {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            } 
            finally 
            {
                if (zf != null) {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
        
        internal static List<StrawberryPerl> GatherPerls ()
        {
            List<StrawberryPerl> perls = new List<StrawberryPerl> ();

            perls.Add(new StrawberryPerl (
                "5.20.1_64",
                "strawberry-perl-5.20.1.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.20.1.1/strawberry-perl-5.20.1.1-64bit-portable.zip",
                "5.20.1")
            );
            
            return perls;
        }
    }

    public struct StrawberryPerl
    {
        public string Name;
        public string ArchiveName;
        public string Url;
        public string Version;
        public string InstallPath;

        public StrawberryPerl (string n, string a, string u, string v)
        {
            this.Name = n;
            this.ArchiveName = a;
            this.Url = u;
            this.Version = v;
            this.InstallPath = @"C:/berrybrew/" + n;
        }
    }
}
