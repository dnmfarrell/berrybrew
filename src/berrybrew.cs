using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace BerryBrew
{
    public class Berrybrew
    {
        // sends a setting change message to reconfigure PATH

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd, 
            int Msg, 
            IntPtr wParam, 
            string lParam, 
            uint fuFlags, 
            uint uTimeout, 
            IntPtr 
            lpdwResult
        );
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        private const int WM_SETTINGCHANGE = 0x001a;
        private const int SMTO_ABORTIFHUNG = 0x2;

        public bool Debug { set; get; }

        static string assembly_path = Assembly.GetExecutingAssembly().Location;
        static string assembly_directory = Path.GetDirectoryName(assembly_path);       

        public string installPath = assembly_directory;
        public string rootPath = null;
        public string archivePath = null;
        public string downloadURL = null;
        public string strawberryURL = null;

        public Message Message = new Message();
        public OrderedDictionary Perls = new OrderedDictionary();

        public Berrybrew()
        {
            // config

            dynamic jsonConf = JsonParse("config");
            this.rootPath = jsonConf.root_dir + "\\";
            this.archivePath = jsonConf.temp_dir;
            this.strawberryURL = jsonConf.strawberry_url;
            this.downloadURL = jsonConf.download_url;
            Debug = jsonConf.debug;

            // messages

            dynamic jsonMessages = JsonParse("messages");
            foreach (dynamic entry in jsonMessages)
            {
                Message.Add(entry);
            }

            // perls
            
            bool installPerlsIntoSelf = true;
            PerlGenerateObjects(installPerlsIntoSelf);
        }

        ~Berrybrew()
        {
            List<string> orphans = PerlFindOrphans();

            if (orphans.Count > 0)
            {
                string orphanedPerls = Message.Get("perl_orphans");
                Console.WriteLine("\nWARNING! {0}\n\n", orphanedPerls.Trim());
                foreach (string orphan in orphans)
                {
                    Console.WriteLine("  {0}\n", orphan);
                }
            }
        }

        public void Available()
        {
            Message.Print("available_header");

            StrawberryPerl currentPerl = PerlInUse();
            string columnSpaces = "               ";

            foreach (StrawberryPerl perl in Perls.Values)
            {
                // cheap printf
                string perlNameToPrint = perl.Name + columnSpaces.Substring(0, columnSpaces.Length - perl.Name.Length);

                Console.Write("\t" + perlNameToPrint);

                if (PerlIsInstalled(perl))
                    Console.Write(" [installed]");

                if (perl.Name == currentPerl.Name)
                    Console.Write("*");

                Console.Write("\n");
            }
            Message.Print("available_footer");
        }

        public void Clean(string subcmd="temp")
        {
            bool cleansed = false;

            switch (subcmd)
            {
                case "temp":
                    cleansed = CleanTemp();
                    if (cleansed)
                        Console.WriteLine("\nremoved all files from {0} temp dir", this.rootPath);
                    else
                        Console.WriteLine("\nno archived perl installation files to remove");
                    break;
                
                case "orphan":
                    cleansed = CleanOrphan();
                    if (!cleansed)
                        Console.WriteLine("\nno orphaned perls to remove");
                    break;
            }
        }

        internal bool CleanOrphan()
        {
            List<string> orphans = PerlFindOrphans();

            foreach (string orphan in orphans)
            {
                FilesystemResetAttributes(orphan);
                Directory.Delete(this.rootPath + orphan);
                Console.WriteLine("removed orphan {0} perl instance", orphan);
            }

            if (orphans.Count > 0)
                return true;
            return false;
        }

        internal bool CleanTemp()
        {
            System.IO.DirectoryInfo archiveDir = new DirectoryInfo(archivePath); 
            FilesystemResetAttributes(archiveDir.FullName);

            List<FileInfo> zipFiles = archiveDir.GetFiles().ToList();

            foreach (FileInfo file in zipFiles) 
            {
                file.Delete();
            }

            if (zipFiles.Count > 0)
                return true;
            return false;
        }

        public void Clone(string sourcePerlName, string destPerlName)
        {
            StrawberryPerl sourcePerl = PerlResolveVersion(sourcePerlName);

            string sourcePerlDir = sourcePerl.InstallPath;
            string destPerlDir = this.rootPath + destPerlName;

            DirectoryInfo src = new DirectoryInfo(sourcePerlDir);

            if (!src.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: " 
                    + sourcePerlDir
                );
            }
            if (!Directory.Exists(destPerlDir))
                Directory.CreateDirectory(destPerlDir);

            foreach (string dirPath in Directory.GetDirectories(sourcePerlDir, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePerlDir, destPerlDir));

            foreach (string newPath in Directory.GetFiles(sourcePerlDir, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePerlDir, destPerlDir), true);

            if (!Directory.Exists(destPerlDir))
            {
                Console.WriteLine("\nfailed to clone {0} to {1}", sourcePerlDir, destPerlDir);
                Environment.Exit(0);
            }

            PerlRegisterCustomInstall(destPerlName, sourcePerl);
        }

        public void Config()
        {
            string configIntro = Message.Get("config_intro");
            Console.WriteLine(configIntro + Version() + "\n");

            if (!PathScan(new Regex("berrybrew.bin"), "machine"))
            {
                Message.Print("add_bb_to_path");

                if (Console.ReadLine() == "y")
                {
                    PathAddBerryBrew(this.installPath);

                    if (PathScan(new Regex("berrybrew.bin"), "machine"))
                    {
                        Message.Print("config_success");
                    }
                    else
                    {
                        Message.Print("config_failure");
                    }
                }
            }
            else
            {
                Message.Print("config_complete");
            }
        }
       
        internal void Exec(StrawberryPerl perl, string command, string sysPath)
        {
            Console.WriteLine("Perl-" + perl.Name + "\n==============");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            List<String> newPath;
            newPath = perl.Paths;
            newPath.Add(sysPath);
            
            System.Environment.SetEnvironmentVariable("PATH", String.Join(";", newPath));

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

        public void ExecCompile(string parameters)
        {
            List<StrawberryPerl> perlsInstalled = PerlsInstalled();
            List<StrawberryPerl> execWith = new List<StrawberryPerl>();
            string command;

            if (parameters.StartsWith("--with"))
            {
                string paramList = Regex.Replace(parameters, @"--with\s+", "");

                string perlStr = paramList.Split(new[] { ' ' }, 2)[0];
                command = paramList.Split(new[] { ' ' }, 2)[1];

                string[] perls = perlStr.Split(',');

                foreach (StrawberryPerl perl in perlsInstalled)
                {
                    foreach (string perlName in perls)
                    {
                        if (perlName.Equals(perl.Name))
                            execWith.Add(perl);
                    }
                }
            }
            else
            {
                command = parameters;
                execWith = perlsInstalled;
            }

            string sysPath = PathRemovePerl(false);

            foreach (StrawberryPerl perl in execWith)
            {
                Exec(perl, command, sysPath);
            }
        }

        public static void Extract(StrawberryPerl perl, string archivePath)
        {
            if (File.Exists(archivePath))
            {
                Console.WriteLine("Extracting " + archivePath);
                ExtractZip(archivePath, perl.InstallPath);
            }
        }

        internal static void ExtractZip(string archivePath, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archivePath);
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

        public string Fetch(StrawberryPerl perl)
        {
            WebClient webClient = new WebClient();
            string archivePath = PerlArchivePath(perl);

            if (!File.Exists(archivePath))
            {
                Console.WriteLine("Downloading " + perl.Url + " to " + archivePath);
                webClient.DownloadFile(perl.Url, archivePath);
            }

            Console.WriteLine("Confirming checksum ...");
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                using (var stream = File.OpenRead(archivePath))
                {
                    string hash = BitConverter.ToString(cryptoProvider.ComputeHash(stream)).Replace("-", "").ToLower();

                    if (perl.Sha1Checksum != hash)
                    {
                        Console.WriteLine("Error checksum of downloaded archive \n"
                            + archivePath
                            + "\ndoes not match expected output\nexpected: "
                            + perl.Sha1Checksum
                            + "\n     got: " + hash);
                        stream.Dispose();
                        Console.Write("Whould you like berrybrew to delete the corrupted download file? y/n [n]");
                        if (Console.ReadLine() == "y")
                        {
                            string retval = FileRemove(archivePath);
                            if (retval == "True")
                            {
                                Console.WriteLine("Deleted! Try to install it again!");
                            }
                            else
                            {
                                Console.WriteLine("Unable to delete " + archivePath);
                            }
                        }
                        Environment.Exit(0);
                    }
                }
            }
            return archivePath;
        }

        internal static string FileRemove(string filename)
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

        internal void FilesystemResetAttributes(string currentDir)
        {
           if (Directory.Exists(currentDir))
           {
               string[] subDirs = Directory.GetDirectories(currentDir);
               foreach(string dir in subDirs)
               FilesystemResetAttributes(dir);
               string[] files = Directory.GetFiles(currentDir);
               foreach (string file in files)
               File.SetAttributes(file, FileAttributes.Normal);
           }
        }

        public string Install(string version)
        {
            StrawberryPerl perl = PerlResolveVersion(version);
            string archive_path = Fetch(perl);
            Extract(perl, archive_path);
            Available();
            return perl.Name;
        }

        internal dynamic JsonParse(string type, bool raw=false)
        {
            string filename = String.Format("{0}.json", type);
            string jsonPath = String.Format("{0}/data/{1}", this.installPath, filename);
            string jsonFile = Regex.Replace(jsonPath, @"bin", "");

            try
            {
                using (StreamReader r = new StreamReader(jsonFile))
                {
                    string jsonData = r.ReadToEnd();
                    if (raw)
                        return jsonData;

                    try
                    {
                        dynamic json= JsonConvert.DeserializeObject(jsonData);
                        return json;
                    }
                    catch (JsonReaderException error)
                    {
                        Console.WriteLine("\n{0} file is malformed. See berrybrew_error.txt in this directory for details.", jsonFile);
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"berrybrew_error.txt", true))
                        {
                            file.WriteLine(error);
                        }
                        Environment.Exit(0);
                    }
                }
            }
            catch (System.IO.FileNotFoundException err)
            {
                Console.WriteLine("\n{0} file can not be found in {1}", filename, this.installPath);

                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }
            return "";
        }

        internal void JsonWrite(string type, dynamic data)
        {
            string jsonString = JsonParse("perls_custom", true);

            List<OrderedDictionary> dataList
                = JsonConvert.DeserializeObject<List<OrderedDictionary>>(jsonString);

            dataList.Add(data);

            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dataList);

            string writeFile = this.installPath + @"data\" + type;
            writeFile = Regex.Replace(writeFile, @"bin", "");
            writeFile = writeFile + @".json";

            System.IO.File.WriteAllText(writeFile, jsonString);
        }

        public void Off()
        {
            PathRemovePerl();
            Console.Write("berrybrew perl disabled. Open a new shell to use system perl\n");
        }

        internal static void PathAddBerryBrew(string binPath)
        {
            string path = PathGet();
            List<string> newPath = new List<string>();

            if (path == null)
            {
                newPath.Add(binPath);
            }
            else
            {
                if (path[path.Length - 1] == ';')
                    path = path.Substring(0, path.Length - 1);

                newPath.Add(path);
                newPath.Add(binPath);
            }

            PathSet(newPath);
        }

        internal static void PathAddPerl(StrawberryPerl perl)
        {
            string path = PathGet();
            List<string> newPath = perl.Paths;
            newPath.Add(path);
            PathSet(newPath);
        }

        internal static string PathGet()
        {
            string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\";
            string path = (string)Registry.LocalMachine.OpenSubKey(keyName).GetValue(
                "PATH",
                "",
                RegistryValueOptions.DoNotExpandEnvironmentNames
            );
            return path;
        }
 
        internal string PathRemovePerl(bool process=true)
        {
            string path = PathGet();
            List<String> paths = new List<String>();

            if (path != null)
            {
                paths = path.Split(';').ToList();

                foreach (StrawberryPerl perl in Perls.Values)
                {
                    for (var i = 0; i < paths.Count; i++)
                    {
                        if (paths[i] == perl.PerlPath
                            || paths[i] == perl.CPath
                            || paths[i] == perl.PerlSitePath)
                        {
                            paths[i] = "";
                        }
                    }
                }

                paths.RemoveAll(str => String.IsNullOrEmpty(str));
                
                if (process)
                    PathSet(paths);
            }

            return String.Join(";", paths);
        }

        internal static bool PathScan(Regex binPattern, string target)
        {
            EnvironmentVariableTarget envTarget = new EnvironmentVariableTarget();

            if (target == "machine"){
                envTarget = EnvironmentVariableTarget.Machine;
            }
            else {
                envTarget = EnvironmentVariableTarget.User;
            }

            string paths = Environment.GetEnvironmentVariable("path", envTarget);

            if (paths != null)
            {
                foreach (string path in paths.Split(';'))
                {
                    if (binPattern.Match(path).Success)
                        return true;
                }
            }
            return false;
        }

        internal static void PathSet(List<string> path)
        {
            path.RemoveAll(str => String.IsNullOrEmpty(str));

            string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
            Registry.LocalMachine.CreateSubKey(keyName).SetValue(
                "Path", 
                String.Join(";", path), 
                RegistryValueKind.ExpandString
            );
            
            SendMessageTimeout(
                HWND_BROADCAST, 
                WM_SETTINGCHANGE, 
                IntPtr.Zero, 
                "Environment", 
                SMTO_ABORTIFHUNG, 
                100, 
                IntPtr.Zero
            );
        }

        internal static string PerlArchivePath(StrawberryPerl perl)
        {
            string path;

            try
            {
                if (!Directory.Exists(perl.ArchivePath))
                    Directory.CreateDirectory(perl.ArchivePath);

                return perl.ArchivePath + @"\" + perl.File;
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

            return path + @"\" + perl.File;
        }

        public List<string> PerlFindOrphans()
        {
            List<StrawberryPerl> perls = PerlsInstalled();

            string[] dirs = Directory.GetDirectories(this.rootPath);

            List<string> perlInstallations = new List<string>();

            foreach (StrawberryPerl perl in perls)
            {
                perlInstallations.Add(perl.InstallPath);
            }

            List<string> orphans = new List<string>();

            foreach (string dir in dirs)
            {
                if (dir == this.archivePath)
                    continue;

                if (!perlInstallations.Contains(dir))
                {
                    string dirBaseName= dir.Remove(0, this.rootPath.Length);
                    orphans.Add(dirBaseName);
                }
            }

            return orphans;
        }

        internal List<StrawberryPerl> PerlGenerateObjects(bool importIntoObject=false)
        {
            List<StrawberryPerl> perls = new List<StrawberryPerl>();
            var Perls = JsonParse("perls");
            var customPerls = JsonParse("perls_custom");

            foreach (var perl in Perls)
            {
                perls.Add(
                    new StrawberryPerl(
                        this,
                        perl.name,
                        perl.file,
                        perl.url,
                        perl.ver,
                        perl.csum,
                        false // custom
                    )
                );
            }

            foreach (var perl in customPerls)
            {
                perls.Add(
                    new StrawberryPerl(
                        this,
                        perl.name,
                        perl.file,
                        perl.url,
                        perl.ver,
                        perl.csum,
                        true // custom
                    )
                );
            }
            if (importIntoObject)
            {
                foreach (StrawberryPerl perl in perls)
                {
                    this.Perls.Add(perl.Name, perl);
                }
            }
            return perls;
        }

        internal StrawberryPerl PerlInUse()
        {
            string path = PathGet();

            StrawberryPerl currentPerl = new StrawberryPerl();

            if (path != null)
            {
                string[] paths = path.Split(';');
                foreach (StrawberryPerl perl in Perls.Values)
                {
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (paths[i] == perl.PerlPath
                            || paths[i] == perl.CPath
                            || paths[i] == perl.PerlSitePath)
                        {
                            currentPerl = perl;
                            break;
                        }
                    }
                }
            }
            return currentPerl;
        }

        internal static bool PerlIsInstalled(StrawberryPerl perl)
        {
            if (Directory.Exists(perl.InstallPath)
                && File.Exists(perl.PerlPath + @"\perl.exe"))
            {
                return true;
            }
            return false;
        }
        
        internal List<StrawberryPerl> PerlsInstalled()
        {
            List<StrawberryPerl> PerlsInstalled = new List<StrawberryPerl>();

            foreach (StrawberryPerl perl in Perls.Values)
            {
                if (PerlIsInstalled(perl))
                    PerlsInstalled.Add(perl);
            }
            return PerlsInstalled;
        }

        public void PerlRemove(string perlVersionToRemove)
        {
            try
            {
                StrawberryPerl perl = PerlResolveVersion(perlVersionToRemove);
                StrawberryPerl currentPerl = PerlInUse();

                if (perl.Name == currentPerl.Name)
                {
                    Console.WriteLine("Removing Perl " + perlVersionToRemove + " from PATH");
                    PathRemovePerl();
                }

                if (Directory.Exists(perl.InstallPath))
                {
                    try
                    {
                        FilesystemResetAttributes(perl.InstallPath);
                        Directory.Delete(perl.InstallPath, true);
                        Console.WriteLine("Successfully removed Strawberry Perl " + perlVersionToRemove);
                    }
                    catch (System.IO.IOException err)
                    {
                        Console.WriteLine("Unable to completely remove Strawberry Perl " + perlVersionToRemove + " some files may remain");

                        if (Debug)
                            Console.WriteLine(err);
                    }
                }
                else
                {
                    Console.WriteLine("Strawberry Perl " + perlVersionToRemove + " not found (are you sure it's installed?");
                    Environment.Exit(0);
                }
            }
            catch (ArgumentException err)
            {
                Message.Print("perl_unknown_version");
                
                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }
            catch (UnauthorizedAccessException err)
            {
                Console.WriteLine("Unable to remove Strawberry Perl " + perlVersionToRemove + " permission was denied by System");

                if (Debug)
                    Console.WriteLine(err);
            }
        }

        public void PerlRegisterCustomInstall(string perlName, StrawberryPerl perlBase=new StrawberryPerl())
        {
            OrderedDictionary data = new OrderedDictionary();

            data["name"] = perlName;
            data["custom"] = perlBase.Custom;
            data["file"] = perlBase.File;
            data["url"] = perlBase.Url;
            data["ver"] = perlBase.Version;
            data["csum"] = perlBase.Sha1Checksum;

            JsonWrite("perls_custom", data);
        }

        internal StrawberryPerl PerlResolveVersion(string version)
        {
            foreach (StrawberryPerl perl in Perls.Values)
            {
                if (perl.Name == version)
                    return perl;
            }
            throw new ArgumentException("Unknown version: " + version);
        }

        public void PerlUpdateAvailableList()
        {
            //FIXME: incomplete for now. do not use

            using (WebClient client = new WebClient())
            {
                string page = client.DownloadString(this.downloadURL);
                string[] content = page.Split('\n');

                OrderedDictionary strawberryPerls = new OrderedDictionary();

                int i = 0;

                foreach (string line in content)
                {
                    if (line.Contains("no64") || line.Contains("-ld-") || line.Contains("PDL"))
                    {
                        i++;
                        continue;
                    }

                    Match lMatch = Regex.Match(line, @"a href=""(.*?(portable|PDL).zip)""");
                    if (lMatch.Success)
                    {
                        string link = this.strawberryURL + lMatch.Groups[1].Value;

                        Match cMatch = Regex.Match(content[i + 1], @">(\w{40})<");
                        if (cMatch.Success)
                        {
                            strawberryPerls.Add(link, cMatch.Groups[1].Value);
                        }
                    }
                    i++;
                }

                //OrderedDictionary perlDetails = new OrderedDictionary();

                foreach (string link in strawberryPerls.Keys)
                {
                    // http://strawberryperl.com/download/5.10.0/strawberry-perl-5.10.0.4-1-portable.zip
                    Match match = Regex.Match(link, @".*/download/.*?/.*(5.*)-portable.zip");
                    if (match.Success)
                    {
                        string verLabel = match.Groups[2].Value;
                        Match extract = Regex.Match(verLabel, @"(5.\d+.\d+).*-(\d{2}bit)");
                        string ver = extract.Groups[1].Value;

                        Console.WriteLine(verLabel);
                    }
                }
            }
        }

        public void Switch(string switchToVersion)
        {
            try
            {
                StrawberryPerl perl = PerlResolveVersion(switchToVersion);

                if (!PerlIsInstalled(perl))
                {
                    Console.WriteLine("Perl version " + perl.Name + " is not installed. Run the command:\n\n\tberrybrew install " + perl.Name);
                    Environment.Exit(0);
                }

                PathRemovePerl();
                PathAddPerl(perl);

                Console.WriteLine("Switched to " + switchToVersion + ", start a new terminal to use it.");
            }
            catch (ArgumentException)
            {
                Message.Print("perl_unknown_version");
                Environment.Exit(0);
            }
        }

        public string Version()
        {
            return Message.Get("version");
        }
    }

    public class Message
    {
        public OrderedDictionary msgMap = new OrderedDictionary();

        public string Get(string label)
        {
            return this.msgMap[label].ToString();
        }

        public void Add(dynamic json)
        {
            string content = null;

            foreach (string line in json.content)
            {
                content += String.Format("{0}\n", line);
            }
            this.msgMap.Add(json.label.ToString(), content);
        }
        
        public void Print(string label)
        {
            string msg = this.Get(label);
            Console.WriteLine(msg);
        }

        public void Say(string label)
        {
            string msg = this.Get(label);
            Console.WriteLine(msg);
            Environment.Exit(0);
        }
    }

    public struct StrawberryPerl
    {
        public string Name;
        public string File;
        public bool Custom;
        public string Url;
        public string Version;
        public string ArchivePath;
        public string InstallPath;
        public string CPath;
        public string PerlPath;
        public string PerlSitePath;
        public List<String> Paths;
        public string Sha1Checksum;

        public StrawberryPerl(Berrybrew BB, object name, object file, object url, object version, object csum, bool custom)
        {
            this.Name = name.ToString();
            this.Custom = custom;
            this.File = file.ToString();
            this.Url = url.ToString();
            this.Version = version.ToString();
            this.ArchivePath = BB.archivePath;
            this.InstallPath =  BB.rootPath + name;
            this.CPath = BB.rootPath + name + @"\c\bin";
            this.PerlPath = BB.rootPath + name + @"\perl\bin";
            this.PerlSitePath = BB.rootPath + name + @"\perl\site\bin";
            this.Paths = new List <String>{
                this.CPath, this.PerlPath, this.PerlSitePath
            };
            this.Sha1Checksum = csum.ToString();
        }
    }
}
