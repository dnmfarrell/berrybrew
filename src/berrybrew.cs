using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

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

            switch (args[0])
            {
                case "install":
                    if (args.Length == 1)
                    {
                        Console.WriteLine("install command requires a version argument. Use the available command to see what versions of Strawberry Perl are available");
                        Environment.Exit(0);
                    }
                    try
                    {
                        StrawberryPerl perl = ResolveVersion(args[1]);
                        string archive_path = Fetch(perl);
                        Extract(perl, archive_path);
                        Available();
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Unknown version of Perl. Use the available command to see what versions of Strawberry Perl are available");
                        Environment.Exit(0);
                    }
                    break;

                case "switch":
                    if (args.Length == 1)
                    {
                        Console.WriteLine("switch command requires a version argument. Use the available command to see what versions of Strawberry Perl are available");
                        Environment.Exit(0);
                    }
                    Switch(args[1]);
                    break;

                case "available":
                    Available();
                    break;

                case "config":
                    Config();
                    break;

                case "remove":
                    if (args.Length == 1)
                    {
                        Console.WriteLine("remove command requires a version argument. Use the available command to see what versions of Strawberry Perl are available");
                        Environment.Exit(0);
                    }
                    RemovePerl(args[1]);
                    break;

                case "exec":
                    if (args.Length == 1)
                    {
                        Console.WriteLine("exec command requires a command to run.");
                        Environment.Exit(0);
                    }
                    args[0] = "";
                    Exec(String.Join(" ", args).Trim());
                    break;

                case "license":
                    if (args.Length == 1 )
                    {
                        PrintLicense();
                        Environment.Exit(0);
                    }
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        internal static void Exec(String args)
        {
            if (args.StartsWith("--with"))
            {
                var remove = @"--with ";
                var re = new Regex(remove);
                var command = re.Replace(args, "");

                var inputs = command.Split(new[] { ' ' }, 2);

                var perls = inputs[0];
                command = inputs[1];

                Console.WriteLine(perls);

            }
            List<StrawberryPerl> perls_installed = GetInstalledPerls();

            foreach (StrawberryPerl perl in perls_installed)
            {
                Console.WriteLine("Perl-" + perl.Name + "\n==============");

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/c " + perl.PerlPath + @"\" + command;
                process.StartInfo = startInfo;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                Console.WriteLine(process.StandardOutput.ReadToEnd());
                Console.WriteLine(process.StandardError.ReadToEnd());
                process.WaitForExit();
            }
        }

        internal static bool PerlInstalled(StrawberryPerl perl)
        {
            if (Directory.Exists(perl.InstallPath)
                && File.Exists(perl.PerlPath + @"\perl.exe"))
            {
                return true;
            }
            return false;
        }

        internal static List<StrawberryPerl> GetInstalledPerls()
        {
            List<StrawberryPerl> perls = GatherPerls();
            List<StrawberryPerl> perls_installed = new List<StrawberryPerl>();

            foreach (StrawberryPerl perl in perls)
            {
                if (PerlInstalled(perl))
                    perls_installed.Add(perl);
            }
            return perls_installed;
        }

        internal static void Config()
        {
            Console.WriteLine("\nThis is berrybrew, version " + Version() + "\n");

            if (!ScanUserPath(new Regex("berrybrew.bin"))
                   && !ScanSystemPath(new Regex("berrybrew.bin")))
            {
                Console.Write("Would you like to add berrybrew to your user PATH? y/n [n] ");

                if (Console.ReadLine() == "y")
                {
                    //get the full path of the assembly
                    string assembly_path = Assembly.GetExecutingAssembly().Location;

                    //get the parent directory
                    string assembly_directory = Path.GetDirectoryName(assembly_path);

                    AddBinToPath(assembly_directory);

                    if (ScanUserPath(new Regex("berrybrew.bin")))
                    {
                        Console.WriteLine("berrybrew was successfully added to the user PATH, start a new terminal to use it.");
                    }
                    else
                    {
                        Console.WriteLine("Error adding berrybrew to the user PATH");
                    }
                }
            }
            else
            {
                Console.Write("berrybrew is already configured on this system.\n");
            }
        }

        internal static string Version()
        {
            return "0.12.1.20160302";
        }

        internal static string RemoveFile(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return true.ToString();
        }

        internal static string Fetch(StrawberryPerl perl)
        {
            WebClient webClient = new WebClient();
            string archive_path = GetDownloadPath(perl);

            // Download if archive doesn't already exist
            if (!File.Exists(archive_path))
            {
                Console.WriteLine("Downloading " + perl.Url + " to " + archive_path);
                webClient.DownloadFile(perl.Url, archive_path);
            }

            Console.WriteLine("Confirming checksum ...");
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                using (var stream = File.OpenRead(archive_path))
                {
                    string hash = BitConverter.ToString(cryptoProvider.ComputeHash(stream)).Replace("-", "").ToLower();

                    if (perl.Sha1Checksum != hash)
                    {
                        Console.WriteLine("Error checksum of downloaded archive \n"
                            + archive_path
                            + "\ndoes not match expected output\nexpected: "
                            + perl.Sha1Checksum
                            + "\n     got: " + hash);
                        stream.Dispose();
                        Console.Write("Whould you like berrybrew to delete the corrupted download file? y/n [n]");
                        if (Console.ReadLine() == "y")
                        {
                            string retval = RemoveFile(archive_path);
                            if (retval == "True")
                            {
                                Console.WriteLine("Deleted! Try to install it again!");
                            }
                            else
                            {
                                Console.WriteLine("Unable to delete " + archive_path);
                            }
                        }
                        Environment.Exit(0);
                    }
                }
            }
            return archive_path;
        }

        internal static string GetDownloadPath(StrawberryPerl perl)
        {
            string path;

            try
            {
                if (!Directory.Exists(perl.ArchivePath))
                    Directory.CreateDirectory(perl.ArchivePath);

                return perl.ArchivePath + @"\" + perl.ArchiveName;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error, do not have permissions to create directory: " + perl.ArchivePath);
            }

            Console.WriteLine("Creating temporary directory instead");
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);

            return path + @"\" + perl.ArchiveName;

        }

        internal static StrawberryPerl ResolveVersion(string version_to_resolve)
        {
            foreach (StrawberryPerl perl in GatherPerls())
            {
                if (perl.Name == version_to_resolve)
                    return perl;
            }
            throw new ArgumentException("Unknown version: " + version_to_resolve);
        }

        internal static void Extract(StrawberryPerl perl, string archive_path)
        {
            if (File.Exists(archive_path))
            {
                Console.WriteLine("Extracting " + archive_path);
                ExtractZip(archive_path, perl.InstallPath);
            }
        }

        internal static void Switch(string version_to_switch)
        {
            try
        {
                StrawberryPerl perl = ResolveVersion(version_to_switch);

                // if Perl version not installed, can't switch
                if (!PerlInstalled(perl))
                {
                    Console.WriteLine("Perl version " + perl.Name + " is not installed. Run the command:\n\n\tberrybrew install " + perl.Name);
                    Environment.Exit(0);
                }

                RemovePerlFromPath();

                if (ScanUserPath(new Regex("perl.bin")))
                {
                    Console.WriteLine("Warning! Perl binary found in your user PATH: "
                        + "\nYou should remove this as it can prevent berrybrew from working.");
                }

                if (ScanSystemPath(new Regex("perl.bin")))
                {
                    Console.WriteLine("Warning! Perl binary found in your system PATH: "
                        + "\nYou should remove this as it can prevent berrybrew from working.");
                }

                AddPerlToPath(perl);
                Console.WriteLine("Switched to " + version_to_switch + ", start a new terminal to use it.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Unknown version of Perl. Use the available command to see what versions of Strawberry Perl are available");
                Environment.Exit(0);
            }
        }

        internal static bool ScanUserPath(Regex bin_pattern)
        {
            string user_path = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User);
            if (user_path != null)
            {
                foreach (string user_p in user_path.Split(';'))
                {
                    if (bin_pattern.Match(user_p).Success)
                        return true;
                }
            }
            return false;
        }

        internal static bool ScanSystemPath(Regex bin_pattern)
        {
            string system_path = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine);

            if (system_path != null)
            {
                foreach (string sys_p in system_path.Split(';'))
                {
                    if (bin_pattern.Match(sys_p).Success)
                        return true;
                }
            }
            return false;
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
                        if (paths[i] == perl.PerlPath
                            || paths[i] == perl.CPath
                            || paths[i] == perl.PerlSitePath)
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

        internal static StrawberryPerl CheckWhichPerlInPath()
        {
            // get user PATH and remove trailing semicolon if exists
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            StrawberryPerl current_perl = new StrawberryPerl();

            if (path != null)
            {
                string[] paths = path.Split(';');
                foreach (StrawberryPerl perl in GatherPerls())
                {
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (paths[i] == perl.PerlPath
                            || paths[i] == perl.CPath
                            || paths[i] == perl.PerlSitePath)
                        {
                            current_perl = perl;
                            break;
                        }
                    }
                }
            }
            return current_perl;
        }

        internal static void AddPerlToPath(StrawberryPerl perl)
        {
            // get user PATH and remove trailing semicolon if exists
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            string[] new_path;

            if (path == null)
            {
                new_path = new string[] { perl.CPath, perl.PerlPath, perl.PerlSitePath };
            }
            else
            {
                if (path[path.Length - 1] == ';')
                    path = path.Substring(0, path.Length - 1);

                new_path = new string[] { path, perl.CPath, perl.PerlPath, perl.PerlSitePath };
            }
            Environment.SetEnvironmentVariable("PATH", String.Join(";", new_path), EnvironmentVariableTarget.User);
        }

        internal static void AddBinToPath(string bin_path)
        {
            // get user PATH and remove trailing semicolon if exists
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            string[] new_path;

            if (path == null)
            {
                new_path = new string[] { bin_path };
            }

            else
            {
                if (path[path.Length - 1] == ';')
                    path = path.Substring(0, path.Length - 1);

                new_path = new string[] { path, bin_path };
            }
            Environment.SetEnvironmentVariable("PATH", String.Join(";", new_path), EnvironmentVariableTarget.User);
        }

        internal static void Available()
        {
            List<StrawberryPerl> perls = GatherPerls();
            Console.WriteLine("\nThe following Strawberry Perls are available:\n");

            StrawberryPerl current_perl = CheckWhichPerlInPath();
            string column_spaces = "               ";

            foreach (StrawberryPerl perl in perls)
            {
                // cheap printf
                string name_to_print = perl.Name + column_spaces.Substring(0, column_spaces.Length - perl.Name.Length);

                Console.Write("\t" + name_to_print);

                if (PerlInstalled(perl))
                    Console.Write(" [installed]");

                if (perl.Name == current_perl.Name)
                    Console.Write("*");

                Console.Write("\n");
            }
            Console.WriteLine("\n* Currently using");
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("\nThis is berrybrew, version " + Version() + "\n");
            Console.WriteLine(@"
berrybrew <command> [option]

    license     Show berrybrew license

    available   List available Strawberry Perl versions and which are installed
    config      Add berrybrew to your PATH
    install     Download, extract and install a Strawberry Perl
    remove      Uninstall a Strawberry Perl
    switch      Switch to use a different Strawberry Perl
    exec        Run a command for every installed Strawberry Perl
    ");

        }

        internal static void PrintLicense()
        {
            Console.WriteLine(@"
This software is Copyright (c) 2014 by David Farrell.

This is free software, licensed under:

  The (two-clause) FreeBSD License

The FreeBSD License

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

  1. Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in the
     documentation and/or other materials provided with the
     distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
""AS IS"" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES(INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    ");

        }

        // From https://github.com/icsharpcode/SharpZipLib
        internal static void ExtractZip(string archive_path, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archive_path);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
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
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        internal static List<StrawberryPerl> GatherPerls()
        {
            List<StrawberryPerl> perls = new List<StrawberryPerl>();

            perls.Add(new StrawberryPerl(
                "5.22.1_64",
                "strawberry-perl-5.22.1.3-64bit-portable.zip",
                "http://strawberryperl.com/download/5.22.1.3/strawberry-perl-5.22.1.3-64bit-portable.zip",
                "5.22.1",
                "ddac06b9f96577e9ae30c6a05554e440d2461a42")
            );

            perls.Add(new StrawberryPerl(
                "5.22.1_64_PDL",
                "strawberry-perl-5.22.1.3-64bit-PDL.zip",
                "http://strawberryperl.com/download/5.22.1.3/strawberry-perl-5.22.1.3-64bit-PDL.zip",
                "5.22.1",
                "0c0f4a97307156192ecabb786645dbbcb4541c8d")
            );

            perls.Add(new StrawberryPerl(
                "5.22.1_32",
                "strawberry-perl-5.22.1.3-32bit-portable.zip",
                "http://strawberryperl.com/download/5.22.1.3/strawberry-perl-5.22.1.3-32bit-portable.zip",
                "5.22.1",
                "8bd94190db08444f80c436fdf805991e5833f905")
            );

            perls.Add(new StrawberryPerl(
                "5.22.1_32_PDL",
                "strawberry-perl-5.22.1.3-32bit-PDL.zip",
                "http://strawberryperl.com/download/5.22.1.3/strawberry-perl-5.22.1.3-32bit-PDL.zip",
                "5.22.1",
                "5f81f9e3ec41b776e75e055547c76cb0c0304a4a")
            );

            perls.Add(new StrawberryPerl(
                "5.22.1_32_NO64",
                "strawberry-perl-no64-5.22.1.3-32bit-portable.zip",
                "http://strawberryperl.com/download/5.22.1.3/strawberry-perl-no64-5.22.1.3-32bit-portable.zip",
                "5.22.1",
                "22bc152dbd864c81af3fcd845b177c7a0c895171")
            );

            perls.Add(new StrawberryPerl(
                "5.20.3_64",
                "strawberry-perl-5.20.2.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.20.3.3/strawberry-perl-5.20.3.3-64bit-portable.zip",
                "5.20.3",
                "b13a0e3000b3ea4ed6137a6279274c4aa09d1f46")
            );

            perls.Add(new StrawberryPerl(
                "5.20.3_64_PDL",
                "strawberry-perl-5.20.3.1-64bit-PDL.zip",
                "http://strawberryperl.com/download/5.20.3.3/strawberry-perl-5.20.3.3-64bit-PDL.zip",
                "5.20.3",
                "9319e70d1d9bf02d8216737934cf28fb8384c7ed")
            );

            perls.Add(new StrawberryPerl(
                "5.20.3_32",
                "strawberry-perl-5.20.3.3-32bit-portable.zip",
                "http://strawberryperl.com/download/5.20.3.3/strawberry-perl-5.20.3.3-32bit-portable.zip",
                "5.20.3",
                "9a3220a21260339ac6054a8fee4592a00b41e265")
            );

            perls.Add(new StrawberryPerl(
                "5.20.3_32_PDL",
                "strawberry-perl-5.20.3.3-32bit-PDL.zip",
                "http://strawberryperl.com/download/5.20.3.3/strawberry-perl-5.20.3.3-32bit-PDL.zip",
                "5.20.3",
                "de0141d2a36dcee0da7c56961ae4254780e512b9")
            );

            perls.Add(new StrawberryPerl(
                "5.20.3_32_NO64",
                "strawberry-perl-no64-5.20.3.3-32bit-portable.zip",
                "http://strawberryperl.com/download/5.20.3.3/strawberry-perl-no64-5.20.3.3-32bit-portable.zip",
                "5.20.3",
                "525db38f8bee6b9940b35b9d8fd76dd1518e3b49")
            );

            perls.Add(new StrawberryPerl(
                "5.18.4_64",
                "strawberry-perl-5.18.4.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.18.4.1/strawberry-perl-5.18.4.1-64bit-portable.zip",
                "5.18.4",
                "cd0809c5d885043d1f14a47f6192503359914d8a")
            );

            perls.Add(new StrawberryPerl(
                "5.18.4_32",
                "strawberry-perl-5.18.4.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.18.4.1/strawberry-perl-5.18.4.1-32bit-portable.zip",
                "5.18.4",
                "f6118ba24e4430a7ddab1200746725f262117fbf")
            );

            perls.Add(new StrawberryPerl(
                "5.16.3_64",
                "strawberry-perl-5.16.3.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.16.3.1/strawberry-perl-5.16.3.1-64bit-portable.zip",
                "5.16.3",
                "07573b99e40355a4920fc9c07fb594575a53c107")
            );

            perls.Add(new StrawberryPerl(
                "5.16.3_32",
                "strawberry-perl-5.16.3.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.16.3.1/strawberry-perl-5.16.3.1-32bit-portable.zip",
                "5.16.3",
                "3b9c4c32bf29e141329c3be417d9c425a7f6c2ff")
            );

            perls.Add(new StrawberryPerl(
                "5.14.4_64",
                "strawberry-perl-5.14.4.1-64bit-portable.zip",
                "http://strawberryperl.com/download/5.14.4.1/strawberry-perl-5.14.4.1-64bit-portable.zip",
                "5.14.4",
                "73ac65962e68f68cf551c3911ab81dcf6f73e018")
            );

            perls.Add(new StrawberryPerl(
                "5.14.4_32",
                "strawberry-perl-5.14.4.1-32bit-portable.zip",
                "http://strawberryperl.com/download/5.14.4.1/strawberry-perl-5.14.4.1-32bit-portable.zip",
                "5.14.4",
                "42f092619704763d4c3cd4f3e1183d3e58d9d02c")
            );

            perls.Add(new StrawberryPerl(
                "5.12.3_32",
                "strawberry-perl-5.12.3.0-portable.zip",
                "http://strawberryperl.com/download/5.12.3.0/strawberry-perl-5.12.3.0-portable.zip",
                "5.12.3",
                "dc6facf9fb7ce2de2e42ee65e84805a6d0dd5fbc")
            );

            perls.Add(new StrawberryPerl(
                "5.10.1_32",
                "strawberry-perl-5.10.1.2-portable.zip",
                "http://strawberryperl.com/download/5.10.1.2/strawberry-perl-5.10.1.2-portable.zip",
                "5.10.1",
                "f86ae4b14daf0b1162d2c4c90a9d22e4c2452a98")
            );

            return perls;
        }

        internal static void RemovePerl(string version_to_remove)
        {
            try
            {
                StrawberryPerl perl = ResolveVersion(version_to_remove);

                StrawberryPerl current_perl = CheckWhichPerlInPath();

                if (perl.Name == current_perl.Name)
                {
                    Console.WriteLine("Removing Perl " + version_to_remove + " from PATH");
                    RemovePerlFromPath();
                }

                if (Directory.Exists(perl.InstallPath))
                {
                    try
                    {
                        Directory.Delete(perl.InstallPath, true);
                        Console.WriteLine("Successfully removed Strawberry Perl " + version_to_remove);
                    }
                    catch (System.IO.IOException)
                    {
                        Console.WriteLine("Unable to completely remove Strawberry Perl " + version_to_remove + " some files may remain");
                    }
                }
                else
                {
                    Console.WriteLine("Strawberry Perl " + version_to_remove + " not found (are you sure it's installed?");
                    Environment.Exit(0);
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Unknown version of Perl. Use the available command to see what versions of Strawberry Perl are available");
                Environment.Exit(0);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Unable to remove Strawberry Perl " + version_to_remove + " permission was denied by System");
            }
        }
    }

    public struct StrawberryPerl
    {
        public string Name;
        public string ArchiveName;
        public string Url;
        public string Version;
        public string ArchivePath;
        public string InstallPath;
        public string CPath;
        public string PerlPath;
        public string PerlSitePath;
        public string Sha1Checksum;

        public StrawberryPerl(string n, string a, string u, string v, string c)
        {
            this.Name = n;
            this.ArchiveName = a;
            this.Url = u;
            this.Version = v;
            this.ArchivePath = @"C:\berrybrew\temp";
            this.InstallPath = @"C:\berrybrew\" + n;
            this.CPath = @"C:\berrybrew\" + n + @"\c\bin";
            this.PerlPath = @"C:\berrybrew\" + n + @"\perl\bin";
            this.PerlSitePath = @"C:\berrybrew\" + n + @"\perl\site\bin";
            this.Sha1Checksum = c;
        }
    }
}
