using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

namespace Berrybrew
{
    public class Berrybrew
    {
        static void Main(string[] args)
        {
            List<StrawberryPerl> perls = new List<StrawberryPerl> ();
            perls.Add(new StrawberryPerl (
                "5.20.1_64",
                "strawberry-perl-5.20.1.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.20.1.1/strawberry-perl-5.20.1.1-64bit-portable.zip",
                "5.20.1")
            );

            if (args.Length > 1)
            {
                switch(args[0])
                {
                    case "install":
                        StrawberryPerl perl = ResolveVersion(perls, args[1]);
                        string archive_path = Fetch(perl);
                        Extract(perl, archive_path);
                        break;

                    default:
                        PrintHelp();
                        break;
                }

            }
            else
            {
                PrintHelp();
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

                FastZip fastZip = new FastZip();
                string fileFilter = null;

                // Will always overwrite if target filenames already exist
                fastZip.ExtractZip(archive_path, perl.InstallPath, fileFilter);
            }

        }

        internal static void Switch ()
        {

        }

        internal static void Available (List <StrawberryPerl> perls)
        {

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
    }

    public struct StrawberryPerl
    {
        public string Name;
        public string ArchiveName;
        public string Url;
        public string Version;
        public string RootPath;

        public StrawberryPerl (string n, string a, string u, string v)
        {
            this.Name = n;
            this.ArchiveName = a;
            this.Url = u;
            this.Version = v;
            this.InstallPath = @"/home/sillymoose/projects/strawberry/" + a;
        }
    }
}
