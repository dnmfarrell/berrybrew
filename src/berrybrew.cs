using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Principal;

namespace Berrybrew
{
    public class Berrybrew
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                Environment.Exit(0);
            }
            
            switch(args[0])
            {
                case "install":
                    StrawberryPerl perl = ResolveVersion(args[1]);
                    string archive_path = Fetch(perl);
                    Extract(perl, archive_path);
                    Available();
                    break;
                
                case "switch":
                    Switch(args[1]);
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

        internal static StrawberryPerl ResolveVersion (string version_to_resolve)
        {
            foreach (StrawberryPerl perl in GatherPerls())
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

        internal static void Switch (string version_to_switch)
        {
            StrawberryPerl perl = ResolveVersion(version_to_switch);
            RemovePerlFromPath();
            ScanPathsForPerl();
            AddPerlToPath(perl);
        }
        
        internal static void ScanPathsForPerl()
        {
            Regex perl_bin = new Regex("perl[\\/]bin");
            string user_path = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User);
            if (user_path != null)
            {
                foreach (string user_p in user_path.Split(';'))
                {
                    if (perl_bin.Match(user_p).Success)
                        Console.WriteLine("Warning! Perl binary found in your user PATH: "  
                            + user_p 
                            + "\nYou should remove this as it can prevent berrybrew from working.");
                }
            }
            
            string system_path = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine);
            
            if (system_path != null) 
            {
                foreach (string sys_p in system_path.Split(';'))
                {
                    if (perl_bin.Match(sys_p).Success)
                        Console.WriteLine("Warning! Perl binary found in your system PATH: "  
                            + sys_p 
                            + "\nYou should remove this as it can prevent berrybrew from working.");
                }
            }
        }
        
        internal static void RemovePerlFromPath() 
        {
            // get user PATH and remove trailing semicolon if exists
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            
            if (path != null)
            {
                string[] paths = path.Split(';');      
                foreach (StrawberryPerl perl in GatherPerls())
                {
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (paths[i] == perl.PerlPath || paths[i] == perl.CPath)
                        {
                            paths[i] = "";
                        }
                    }
                }
                
                // Update user path and parse out unnecessary semicolons
                string new_path = String.Join(";", paths);
                Regex multi_semicolon = new Regex(";{2,}");
                new_path = multi_semicolon.Replace(new_path, ";");
                Regex lead_semicolon = new Regex("^;");
                new_path = lead_semicolon.Replace(new_path, "");
                Environment.SetEnvironmentVariable("Path", new_path, EnvironmentVariableTarget.User);   
            }            
        }
        
        internal static void AddPerlToPath(StrawberryPerl perl) 
        {
            // get user PATH and remove trailing semicolon if exists
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            string[] new_path;

            if (path == null)
            {
                new_path = new string[] { perl.CPath, perl.PerlPath };
            }
            
            else 
            {
                if (path[path.Length - 1] == ';')
                    path = path.Substring(0, path.Length - 1);
                
                new_path = new string[] { path, perl.CPath, perl.PerlPath };
            }
            Environment.SetEnvironmentVariable("PATH", String.Join(";", new_path), EnvironmentVariableTarget.User);           
        }

        internal static void Available ()
        {
            List<StrawberryPerl> perls = GatherPerls();
            Console.WriteLine("\nThe following Strawberry Perls are available:\n");
                
            foreach (StrawberryPerl perl in perls)
            {
                string name = perl.Name;
                if (Directory.Exists(perl.InstallPath))
                    Console.WriteLine("\t" + name + " [installed]");
                    
                else
                    Console.WriteLine("\t" + name);
            }
        }

        internal static void PrintHelp()
        {
            Console.WriteLine(@"
berrybrew <command> [option]

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
            
            perls.Add(new StrawberryPerl (
                "5.20.1_32",
                "strawberry-perl-5.20.1.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.20.1.1/strawberry-perl-5.20.1.1-32bit-portable.zip",
                "5.20.1")
            );
            
                        
            perls.Add(new StrawberryPerl (
                "5.18.4_64",
                "strawberry-perl-5.18.4.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.18.4.1/strawberry-perl-5.18.4.1-64bit-portable.zip",
                "5.18.4")
            );
            
            perls.Add(new StrawberryPerl (
                "5.18.4_32",
                "strawberry-perl-5.18.4.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.18.4.1/strawberry-perl-5.18.4.1-32bit-portable.zip",
                "5.18.4")
            );
            
            perls.Add(new StrawberryPerl (
                "5.16.3_64",
                "strawberry-perl-5.16.3.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.16.3.1/strawberry-perl-5.16.3.1-64bit-portable.zip",
                "5.16.3")
            );
            
            perls.Add(new StrawberryPerl (
                "5.16.3_32",
                "strawberry-perl-5.16.3.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.16.3.1/strawberry-perl-5.16.3.1-32bit-portable.zip",
                "5.16.3")
            );
            
            perls.Add(new StrawberryPerl (
                "5.14.4_64",
                "strawberry-perl-5.14.4.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.14.4.1/strawberry-perl-5.14.4.1-64bit-portable.zip",
                "5.14.4")
            );
            
            perls.Add(new StrawberryPerl (
                "5.14.4_32",
                "strawberry-perl-5.14.4.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.14.4.1/strawberry-perl-5.14.4.1-32bit-portable.zip",
                "5.14.4")
            );
            
            perls.Add(new StrawberryPerl (
                "5.12.3_32",
                "strawberry-perl-5.12.3.0-portable.zip",
                "http://strawberryperl.com/download/5.12.3.0/strawberry-perl-5.12.3.0-portable.zip",
                "5.12.3")
            );
            
            perls.Add(new StrawberryPerl (
                "5.10.1_32",
                "strawberry-perl-5.10.1.2-portable.zip",
                "http://strawberryperl.com/download/5.10.1.2/strawberry-perl-5.10.1.2-portable.zip",
                "5.10.1")
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
        public string CPath;
        public string PerlPath;

        public StrawberryPerl (string n, string a, string u, string v)
        {
            this.Name = n;
            this.ArchiveName = a;
            this.Url = u;
            this.Version = v;
            this.InstallPath = @"C:/berrybrew/" + n;
            this.CPath = "C:/berrybrew/" + n + "/c/bin";
            this.PerlPath = "C:/berrybrew/" + n + "/perl/bin";
        }
    }
}
