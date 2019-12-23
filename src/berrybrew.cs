using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BerryBrew {
    public class Berrybrew {

        // sends a setting change message to reconfigure PATH

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            IntPtr
            lpdwResult
        );
        private static readonly IntPtr HwndBroadcast = new IntPtr(0xffff);
        private const int WmSettingchange = 0x001a;
        private const int SmtoAbortifhung = 0x2;

        private const int MaxPerlNameLength = 25;

        public bool Debug { set; get; }
        public bool Testing { set; get; }

        public List<string> validOptions;

        private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);

        private string registrySubKey;
        
        private string binPath = AssemblyDirectory;
        public string archivePath;
        public string installPath;
        public string rootPath;
        
        private string configPath;
        private string downloadURL;
        private bool windowsHomedir;
        
        private bool customExec;
        private bool bypassOrphanCheck;

        public readonly Message Message = new Message();
        private readonly OrderedDictionary _perls = new OrderedDictionary();

        public Berrybrew() {

            // Initialize configuration

            installPath = Regex.Replace(binPath, @"bin", "");
            configPath = installPath + @"/data/";                      
            registrySubKey = @"SOFTWARE\berrybrew";

            validOptions = new List<string>{
                "debug",
                "root_dir", 
                "temp_dir", 
                "download_url",
                "windows_homedir", 
                "custom_exec", 
                "run_mode",
                "file_assoc",
                "file_assoc_old"
            }; 

            if (binPath.Contains("test")) {
                Console.WriteLine("IN TEST MODE");
                registrySubKey += "-test";
            }
            else if (binPath.Contains("build")) {
                Console.WriteLine("IN DEV MODE");
                registrySubKey += "-build";
            }
 
            BaseConfig();

            // ensure the Perl install dir exists

            CheckRootDir();

            // create the custom and virtual perls config file

            string customPerlsFile = configPath + @"perls_custom.json";
            string virtualPerlsFile = configPath + @"perls_virtual.json";

            if (! File.Exists(customPerlsFile))
                File.WriteAllText(customPerlsFile, @"[]");

            if (! File.Exists(virtualPerlsFile))
                File.WriteAllText(virtualPerlsFile, @"[]");
            
            // messages

            dynamic jsonMessages = JsonParse("messages");

            foreach (dynamic entry in jsonMessages)
                Message.Add(entry);

            // perls

            const bool installPerlsIntoSelf = true;
            PerlGenerateObjects(installPerlsIntoSelf);
        }

        ~Berrybrew(){
            List<string> orphans = PerlFindOrphans();

            if (orphans.Count > 0 && ! bypassOrphanCheck){
                string orphanedPerls = Message.Get("perl_orphans");
                Console.WriteLine("\nWARNING! {0}\n\n", orphanedPerls.Trim());
                foreach (string orphan in orphans)
                    Console.WriteLine("  {0}", orphan);
            }
        }
      
        public void Available(bool allPerls=false){

            Message.Print("available_header");

            List<int> nameLengths = new List<int>();

            foreach (string perlName in _perls.Keys)
                nameLengths.Add(perlName.Length);

            int maxNameLength = nameLengths.Max();

            foreach (StrawberryPerl perl in _perls.Values){
                if (! allPerls && ! perl.Newest){
                    if (! PerlIsInstalled(perl) && ! perl.Custom && ! perl.Virtual)
                        continue;
                }	

                string perlNameToPrint = perl.Name + new String(' ', (maxNameLength - perl.Name.Length) + 2);
                Console.Write("\t" + perlNameToPrint);

                if (PerlIsInstalled(perl))
                    Console.Write(" [installed] ");
                if (perl.Custom)
                    Console.Write("[custom]");
                if (perl.Virtual)
                    Console.Write("[virtual]");               

                if (perl.Name == PerlInUse().Name)
                    Console.Write(" *");

                Console.Write("\n");
            }
            Message.Print("available_footer");
        }

        public List<string> AvailableList(bool allPerls=false) {
            List<string> availablePerls = new List<string>();

            foreach (StrawberryPerl perl in _perls.Values) {

                if (! allPerls && ! perl.Newest){
                    continue;
                }
    
                if (PerlIsInstalled(perl))
                    continue;
                if (perl.Custom)
                    continue;
                if (perl.Virtual)
                    continue;
                if (perl.Name == PerlInUse().Name)
                    continue;

                availablePerls.Add(perl.Name);
            }

            return availablePerls;
        }

        private void BaseConfig() {

            dynamic jsonConf = JsonParse("config");
            
            try {
                if (Registry.LocalMachine.OpenSubKey(registrySubKey) == null) {

                    RegistryKey regKey =
                        Registry.LocalMachine.CreateSubKey(registrySubKey);

                    foreach (string confKey in validOptions) {
                        if (Debug)
                            Console.WriteLine("{0}: {1}", confKey, jsonConf[confKey]);
                        regKey.SetValue(confKey, jsonConf[confKey]);
                    }
                }
            }
            catch (UnauthorizedAccessException err) {
                Console.WriteLine(
                    "\nInitializing berrybrew requires Administrator privileges");
                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }
            
            RegistryKey registry = Registry.LocalMachine.OpenSubKey(registrySubKey);

            rootPath = (string) registry.GetValue("root_dir", "");
            rootPath += @"\";

            archivePath = (string) registry.GetValue("temp_dir", "");
            
            downloadURL = (string) registry.GetValue("download_url", "");
            
            if ((string) registry.GetValue("windows_homedir", "false") == "true")
                windowsHomedir = true;

            if ((string) registry.GetValue("custom_exec", "false") == "true")
                customExec = true;

            if ((string) registry.GetValue("debug", "false") == "true")
                Debug = true;
        }

        private static bool CheckName (string perlName){

            if (perlName.Length > MaxPerlNameLength){
                Console.WriteLine(
                    "name for a Perl must be {0} chars or less. You supplied {1}, length {2}",
                    MaxPerlNameLength,
                    perlName,
                    perlName.Length
                );
                return false;
            }
            return true;
        }

        private void CheckRootDir(){
            if (Directory.Exists(rootPath)) return;

            try {
                Directory.CreateDirectory(rootPath);
            }

            catch (Exception err){
                Console.WriteLine("\nCouldn't create install dir {0}. Please create it manually and run config again", rootPath);
                if (Debug)
                    Console.WriteLine(err);
            }
        }

        public void Clean(string subcmd="temp"){
            bool cleansed;

            switch (subcmd){
                case "all":
                    CleanTemp();
                    CleanOrphan();
                    CleanModules();
                    CleanDev();
                    break;
                                        
                case "dev":
                    cleansed = CleanDev();
                    Console.WriteLine(cleansed
                        ? "\nremoved the build and test directories"
                        : "\nan error has occured removing dev directories");
                    break;
                    
                case "modules":
                    cleansed = CleanModules();
                    Console.WriteLine(cleansed
                        ? "\ncleaned the module list storage directory"
                        : "\nno module lists saved to remove");
                    break;
                
                case "temp":
                    cleansed = CleanTemp();
                    if (cleansed)
                        Console.WriteLine("\nremoved all files from {0} temp dir", rootPath);
                    else
                        Console.WriteLine("\nno archived perl installation files to remove");
                    break;

                case "orphan":
                    cleansed = CleanOrphan();
                    if (! cleansed)
                        Console.WriteLine("\nno orphaned perls to remove");
                    break;
            }
        }
        
        private bool CleanModules(){
            string moduleDir = rootPath + "modules\\";

            if (! Directory.Exists(moduleDir)) {
                return true;
            }
            
            string[] moduleListFiles = Directory.GetFiles(moduleDir);

            try {
                    FilesystemResetAttributes(moduleDir);

                    foreach (string file in moduleListFiles) {
                        if (! Regex.Match(file, @"\d\.\d+\.\d+_\d+").Success) {
                            continue;
                        }
                        FileRemove(file);
                    }
            }
            
            catch (Exception err) {
                Console.WriteLine(
                    "\nUnable to clean up the module list directory");
                if (Debug) {
                    Console.WriteLine(err);
                }
            }

            moduleListFiles = Directory.GetFiles(moduleDir);
            bool cleaned = true;
            
            foreach (string file in moduleListFiles) {
                if ( Regex.Match(file, @"\d\.\d+\.\d+_\d+").Success) {
                    cleaned = false;
                }
            }           
           
            return cleaned;
        }
 
        private bool CleanDev() {

            string buildDir = rootPath;
            string testDir = rootPath;
            
            if (Testing) {
                buildDir = buildDir.Replace("\\build", "");
                testDir = testDir.Replace("\\build", "");
                buildDir = buildDir.Replace("\\test", "");
                testDir = testDir.Replace("\\test", "");               
            } 

            buildDir += @"build";
            testDir = string.Format(@"{0}test", testDir);

            if (Debug){
                Console.WriteLine("build dir: {0}", buildDir);
                Console.WriteLine("test dir: {0}", testDir);
            }
            try {
                if (Directory.Exists(buildDir)){
                    FilesystemResetAttributes(buildDir);
                    Directory.Delete(buildDir, true);
                }
            }
            catch (Exception err) {
                Console.WriteLine("\nUnable to remove the build directory");
                if (Debug) {
                    Console.WriteLine(err);
                }
            }

            try {
                if (Directory.Exists(testDir)) {
                    FilesystemResetAttributes(testDir);
                    Directory.Delete(testDir, true);
                }
            }
            catch (Exception err){
                Console.WriteLine("\nUnable to remove the test directory");
                if (Debug) {
                    Console.WriteLine(err);
                }               
            }

            if (Directory.Exists(buildDir))
                return false;

            if (Directory.Exists(testDir))
                return false;
            
            return true;
        }
                    
        private bool CleanOrphan(){
            List<string> orphans = PerlFindOrphans();

            foreach (string orphan in orphans){
                FilesystemResetAttributes(orphan);
                Directory.Delete(rootPath + orphan, true);
                Console.WriteLine("removed orphan {0} perl instance", orphan);
            }

            return orphans.Count > 0;
        }

        private bool CleanTemp(){
            
            if (! Directory.Exists(archivePath)) {
                return true;
            }           
           
            DirectoryInfo archiveDir = new DirectoryInfo(archivePath);
            
            FilesystemResetAttributes(archiveDir.FullName);

            List<FileInfo> zipFiles = archiveDir.GetFiles().ToList();

            foreach (FileInfo file in zipFiles)
                file.Delete();

            return zipFiles.Count > 0;
        }

        public bool Clone(string sourcePerlName, string destPerlName){

            if (! CheckName(destPerlName))
                return false;

            StrawberryPerl sourcePerl = new StrawberryPerl();
            
            try {
                sourcePerl = PerlResolveVersion(sourcePerlName);
            }
            catch (System.ArgumentException e) {
                Console.WriteLine("\n'{0}' is an unknown version of Perl. Can't clone.", sourcePerlName);
                if (Debug)
                    Console.WriteLine("\n{0}", e);
                Environment.Exit(0);
            }

            string sourcePerlDir = sourcePerl.installPath;
            string destPerlDir = rootPath + destPerlName;
            DirectoryInfo src = new DirectoryInfo(sourcePerlDir);

            if (! src.Exists) {
                Console.WriteLine("\nPerl instance '{0}' isn't installed. Can't clone.", sourcePerlName);
                Environment.Exit(0);
            }
            
            try {
                if (! Directory.Exists(destPerlDir))
                    Directory.CreateDirectory(destPerlDir);

                foreach (string dirPath in Directory.GetDirectories(sourcePerlDir, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePerlDir, destPerlDir));

                foreach (string newPath in Directory.GetFiles(sourcePerlDir, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePerlDir, destPerlDir), true);

                if (! Directory.Exists(destPerlDir)){
                    Console.WriteLine("\nfailed to clone {0} to {1}", sourcePerlDir, destPerlDir);
                    Environment.Exit(0);
                }

                PerlRegisterCustomInstall(destPerlName, sourcePerl);

                Console.WriteLine("\nSuccessfully installed custom perl '{0}'", destPerlName);

                return true;
            }

            catch (IOException err){
                Console.WriteLine("\nClone failed due to disk I/O error... ensure the disk isn't full\n");

                if (Debug)
                    Console.WriteLine(err);

                return false;
            }
        }

        public void Config(){

            string configIntro = Message.Get("config_intro");
            
            configIntro = configIntro.Replace("\n", String.Empty);
            configIntro = configIntro.Replace("\r", String.Empty);            

            Console.WriteLine("\n{0}{1}", configIntro, Version());

            if (! PathScan(binPath, "machine")){
                PathAddBerryBrew(binPath);

                Message.Print(PathScan(binPath, "machine")
                    ? "config_success"
                    : "config_failure");
            }
            else
                Message.Print("config_complete");
        }
        
        public void ExportModules(){
            StrawberryPerl perl = PerlInUse();

            if (string.IsNullOrEmpty(perl.Name)) {
                Console.WriteLine("\nno Perl is in use. Run 'berrybrew switch' to enable one before exporting a module list\n");
                Environment.Exit(0);
            }
            if (perl.Name == "5.10.1_32"){
                Console.WriteLine("\nmodules command requires a Perl version greater than 5.10\n");
                Environment.Exit(0);
            }           
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            string moduleDir = rootPath + "modules\\";

            if ( !Directory.Exists(moduleDir)) {
                Directory.CreateDirectory(moduleDir);
            }

            string moduleFile = moduleDir + perl.Name;
            
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments =
                "/c " +
                "perl -MExtUtils::Installed -E \"say $_ for ExtUtils::Installed->new->modules\"" +
                " > " +
                moduleFile;
        
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.OutputDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Error.WriteLine(line.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            Console.WriteLine("\nsuccessfully wrote out {0} module list file", moduleFile);
            
        }
       
        private static void Exec(StrawberryPerl perl, IEnumerable<string> parameters, string sysPath, bool singleMode){

            if(!singleMode){
                Console.WriteLine("perl-" + perl.Name + "\n==============");
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            var newPath = perl.Paths;
            newPath.Add(sysPath);

            Environment.SetEnvironmentVariable("PATH", string.Join(";", newPath));

            startInfo.FileName = "cmd.exe";
            List<String> patchedParams = new List<String>();
            foreach(String param in parameters){
                if( param.Contains(" ")){
                     patchedParams.Add("\"" + param + "\"");
                }
                else{
                     patchedParams.Add(param);
                }
            }

            startInfo.Arguments = "/c " + String.Join(" ", patchedParams);
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.OutputDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Error.WriteLine(line.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            if (singleMode){
                Environment.ExitCode = process.ExitCode;
            }
        }

        public void ExecCompile(List<String> parameters){

            List<StrawberryPerl> perlsInstalled = PerlsInstalled();
            List<StrawberryPerl> execWith = new List<StrawberryPerl>();

            if (parameters.ElementAt(0).Equals("--with") && parameters.Count > 1){
                parameters.RemoveAt(0);
                string perlsToUse = parameters.ElementAt(0);
                parameters.RemoveAt(0);

                List<string> perls = new List<string>();

                if (!perlsToUse.Contains(",")) {
                    perls.Add(perlsToUse);
                }
                else
                     perls = new List<string>(perlsToUse.Split(new char[] {','}));

                foreach (StrawberryPerl perl in perlsInstalled){
                    foreach (string perlName in perls){
                        if (perlName.Equals(perl.Name))
                            execWith.Add(perl);
                    }
                }
            }
            else {
                execWith = perlsInstalled;
            }

            string sysPath = PathGet();

            List<StrawberryPerl> filteredExecWith = new List<StrawberryPerl>();

            foreach(StrawberryPerl perl in execWith){
                if (perl.Custom && ! customExec)
                    continue;
                if (perl.Name.Contains("tmpl") || perl.Name.Contains("template"))
                    continue;
                filteredExecWith.Add(perl);
            }

            foreach (StrawberryPerl perl in filteredExecWith){
                Exec(perl, parameters, sysPath, filteredExecWith.Count == 1);
            }
        }

        private static void Extract(StrawberryPerl perl, string archivePath){

            ZipFile zf = null;
            try {
                Console.WriteLine("Extracting {0}", archivePath);
                FileStream fs = File.OpenRead(archivePath);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf){
                    if (! zipEntry.IsFile)
                        continue;

                    string entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    string fullZipToPath = Path.Combine(perl.installPath, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    
                    if (!string.IsNullOrEmpty(directoryName)){
                        Directory.CreateDirectory(directoryName);
                    }
                    else {
                        Console.WriteLine("\nCould not get the zip archive's directory name.\n");
                        Environment.Exit(0);
                    }

                    using (FileStream streamWriter = File.Create(fullZipToPath)){
                        ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally {
                if (zf != null){
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
        }

        private string Fetch(StrawberryPerl perl){

            WebClient webClient = new WebClient();
            string archivePath = PerlarchivePath(perl);

            if (! File.Exists(archivePath)){
                try {
                    Console.WriteLine("Downloading " + perl.Url + " to " + archivePath);
                    webClient.DownloadFile(perl.Url, archivePath);
                }
                catch (WebException){
                    Console.WriteLine("\nUnable to download file. Check your Internet connection and/or the download site\n");
                    Environment.Exit(0);
                }
            }

            Console.WriteLine("Confirming checksum ... ");

            using (var cryptoProvider = new SHA1CryptoServiceProvider()){

                using (var stream = File.OpenRead(archivePath)){

                    string hash = BitConverter.ToString(cryptoProvider.ComputeHash(stream)).Replace("-", "").ToLower();

                    if (perl.Sha1Checksum != hash){
                        Console.WriteLine("Error checksum of downloaded archive \n"
                            + archivePath
                            + "\ndoes not match expected output\nexpected: "
                            + perl.Sha1Checksum
                            + "\n     got: " + hash);
                        stream.Dispose();
                        Console.Write("Would you like berrybrew to delete the corrupted download file? y/n [n]");

                        if (Console.ReadLine() == "y"){
                            string retval = FileRemove(archivePath);
                            if (retval == "True")
                                Console.WriteLine("Deleted! Try to install it again!");
                            else
                                Console.WriteLine("Unable to delete " + archivePath);
                        }

                        Environment.Exit(0);
                    }
                    else {
                        Console.WriteLine("Checksum OK");
                    }
                }
            }
            return archivePath;
        }

        public void FileAssoc(string action="") {
            string plExtSubKey = @".pl";
            string plHandlerName = "";

            try {
                RegistryKey plExtKey = Registry.ClassesRoot.CreateSubKey(plExtSubKey);
                plHandlerName = (string) plExtKey.GetValue("");

                if (action == "set") {
       
                    if (plHandlerName == @"berrybrewPerl") {
                        Console.WriteLine("\nberrybrew is already managing the .pl file type\n");
                        Environment.Exit(0);
                    }

                    Options("file_assoc_old", plHandlerName, true);
                    plHandlerName = @"berrybrewPerl";
                    
                    plExtKey.SetValue("", plHandlerName);
                    Options("file_assoc", plHandlerName, true);

                    RegistryKey plHandlerKey = Registry.ClassesRoot.CreateSubKey(plHandlerName + @"\shell\run\command");
                    plHandlerKey.SetValue("", binPath + @"\env.exe perl ""%1"" %*");
                    
                    Console.WriteLine("\nberrybrew is now managing the Perl file association");
                }
                else if (action == "unset") {
                    string old_file_assoc = Options("file_assoc_old", "", true);

                    if (old_file_assoc == "") {
                        Console.WriteLine("\nDefault file association already in place");
                        Environment.Exit(0);
                    }

                    plExtKey.SetValue("", old_file_assoc);
                    Options("file_assoc_old", "", true);
                    Options("file_assoc", old_file_assoc, true);

                    Console.WriteLine("\nSet Perl file association back to default");
                }
                else {
                    Options("file_assoc", plHandlerName, true);
                    Console.WriteLine("\nPerl file association handling:");
                    Console.WriteLine("\n\tHandler:\t{0}", Options("file_assoc", "", true));
                }
            }
            catch (UnauthorizedAccessException e) {
                Console.WriteLine("\nChanging file associations requires Administrator privileges");

                if (Debug)
                    Console.WriteLine(e);
            }
        }

        private static string FileRemove(string filename){

            try {
                File.Delete(filename);
            }

            catch (Exception ex){
                return ex.ToString();
            }

            return true.ToString();
        }

        private static void FilesystemResetAttributes(string currentDir){
            if (!Directory.Exists(currentDir)) return;

            string[] subDirs = Directory.GetDirectories(currentDir);
            foreach(string dir in subDirs)
                FilesystemResetAttributes(dir);
            string[] files = Directory.GetFiles(currentDir);
            foreach (string file in files)
                File.SetAttributes(file, FileAttributes.Normal);
        }

        public void ImportModules(string version=""){

            string moduleDir = rootPath + "modules\\";

            if (! Directory.Exists(moduleDir)) {
                Directory.CreateDirectory((moduleDir));
            }
            
            if (version == "") {
                string[] moduleListFiles = Directory.GetFiles(moduleDir);

                if (moduleListFiles.Length == 0) {
                    Console.WriteLine("\nno module lists to import from. Run 'berrybrew modules export', then re-run the import command...\n"); 
                    Environment.Exit(0);
                }
                
                Console.WriteLine("\nre-run the command with one of the following options:\n");
                
                foreach (string fileName in moduleListFiles){
                    if (fileName.Contains("~"))
                        continue;
                    
                    Console.WriteLine(Path.GetFileName(fileName)); 
                }

                Console.WriteLine();
            }
            else {
                ImportModulesExec(version, moduleDir + version);
            }
        }

        private void ImportModulesExec(string file, string path){
            if (file == PerlInUse().Name) {
                Console.WriteLine("\ncan't import modules exported from the same perl version\n");
                Console.WriteLine("you're trying to use an export from version {0} and you're on {1}\n", file, arg1: PerlInUse().Name);
                Environment.Exit(0);
            }
                
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            path = path.Replace("\\", "/");
            
            Console.WriteLine(path);
            
            string perlCmd = "while(<>){print $_}\" " + path;
            string cmd = "perl -wMstrict -E \"" + perlCmd + " | cpanm";
           
            Console.WriteLine(cmd);
            
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + cmd;
        
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.OutputDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if( line.Data != null){
                    Console.Error.WriteLine(line.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        
        public void Info(string want){
            List <string> options = new List<string>(){"install_path", "bin_path", "root_path", "archive_path"};

            if (! options.Contains(want)) {
                Console.WriteLine("\n'{0}' is not a valid option. Valid options are:\n", want);
                foreach (string opt in options){
                    Console.WriteLine("\t{0}", opt);
                }
                Environment.Exit(0);
            }

            switch (want) {
                case "install_path":
                    Console.WriteLine("\n\t{0}", installPath);
                    break;
                case "bin_path":
                    Console.WriteLine("\n\t{0}", binPath);
                    break;
                case "root_path":
                    Console.WriteLine("\n\t{0}", rootPath);
                    break;
                case "archive_path":
                    Console.WriteLine("\n\t{0}", archivePath);
                    break;
                default:
                    Console.WriteLine("\nCould not fetch details for '{0}'", want);
                    Environment.Exit(0);
                    break;
            }
        }

        public void Install(string version){

            StrawberryPerl perl = PerlResolveVersion(version);
            string archivePath = Fetch(perl);
            Extract(perl, archivePath);
            

            if (windowsHomedir) {
                string homedirFile = perl.installPath + "/perl/vendor/lib/Portable/HomeDir.pm";
                
                if (File.Exists(homedirFile)) {
                    Console.WriteLine("file exists");
                    FileRemove(homedirFile);
                }
            }
            
            Available();
        }

        private dynamic JsonParse(string type, bool raw=false){

            string filename = string.Format("{0}.json", type);
            string jsonFile = configPath + filename;

            try {
                using (StreamReader r = new StreamReader(jsonFile)){

                    string jsonData = r.ReadToEnd();

                    if (raw)
                        return jsonData;

                    try {
                        dynamic json= JsonConvert.DeserializeObject(jsonData);
                        return json;
                    }

                    catch (JsonReaderException error){
                        Console.WriteLine("\n{0} file is malformed. See berrybrew_error.txt in this directory for details.", jsonFile);
                        using (StreamWriter file = new StreamWriter(@"berrybrew_error.txt", true))
                            file.WriteLine(error);

                        Environment.Exit(0);
                    }
                }
            }

            catch (FileNotFoundException err){
                Console.WriteLine("\n{0} file can not be found in {1}", filename, jsonFile);

                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }
            return "";
        }

        private void JsonWrite(string type, List<Dictionary<string, object>> data, bool fullList=false){

            string jsonString;

            if (!fullList && type == "perls_custom"){
                dynamic customPerlList = JsonParse("perls_custom", true);
                var perlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(customPerlList);

                foreach (Dictionary<string, object> perl in data){
                    foreach (Dictionary<string, object> existingPerl in perlList){
                        if (perl["name"].Equals(existingPerl["name"])){
                            Console.Write("\n{0} instance is already registered...", perl["name"]);
                            Environment.Exit(0);
                        }
                    }
                    perlList.Add(perl);
                }
                jsonString = JsonConvert.SerializeObject(perlList, Formatting.Indented);
            }
            else if (!fullList && type == "perls_virtual"){
                dynamic virtualPerlList = JsonParse("perls_virtual", true);
                var perlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(virtualPerlList);
 
                foreach (Dictionary<string, object> perl in data){
                    foreach (Dictionary<string, object> existingPerl in perlList){
                        if (perl["name"].Equals(existingPerl["name"])){
                            Console.Write("\n{0} instance is already registered...", perl["name"]);
                            Environment.Exit(0);
                        }
                    }
                    perlList.Add(perl);
                }
                jsonString = JsonConvert.SerializeObject(perlList, Formatting.Indented);
            }           
            else {
                List<string> perlVersions = new List<string>();

                foreach (var perl in data){
                    perlVersions.Add(perl["ver"].ToString());
                }

                var sortedPerlVersions = perlVersions
                    .Select(x => x.Split(new char[] {'.'}))
                    .Select(x => new {
                        a = Convert.ToInt32(x[0]),
                        b = Convert.ToInt32(x[1]),
                        c = Convert.ToInt32(x[2])
                    })
                    .OrderBy(x => x.a).ThenBy(x => x.b).ThenBy(x => x.c)
                    .Select(x => string.Format("{0}.{1}.{2}", x.a, x.b, x.c))
                    .ToList();

                sortedPerlVersions.Reverse();
                var sortedData = new List<Dictionary<string, object>>();

                List<string> perlCache = new List<string>();

                foreach (var ver in sortedPerlVersions){
                    foreach (var perl in data) {
                        if (!perl["ver"].Equals(ver)) continue;
                        if (perlCache.Contains(perl["name"].ToString())) continue;
                        perlCache.Add(perl["name"].ToString());
                        sortedData.Add(perl);
                    }
                }

                jsonString = JsonConvert.SerializeObject(sortedData, Formatting.Indented);
            }

            string writeFile = configPath + type;
            writeFile = writeFile + @".json";

            File.WriteAllText(writeFile, jsonString);
        }
        
        public void List(){
            StrawberryPerl currentPerl = PerlInUse();

            List<int> nameLengths = new List<int>();
            List<StrawberryPerl> installedPerls = PerlsInstalled();

            if (! installedPerls.Any()){
                Console.Write("\nNo versions of Perl are installed.\n");
                Environment.Exit(0);
            }

            foreach (StrawberryPerl perl in installedPerls)
                nameLengths.Add(perl.Name.Length);

            int maxNameLength = nameLengths.Max();

            foreach(StrawberryPerl perl in installedPerls){
                string perlNameToPrint = perl.Name + new String(' ', (maxNameLength - perl.Name.Length) + 2);
                Console.Write("\t" + perlNameToPrint);

                if (perl.Custom)
                    Console.Write(" [custom]");
                if (perl.Virtual)
                    Console.Write(" [virtual]");
                if (perl.Name == currentPerl.Name)
                    Console.Write(" *");

                Console.Write("\n");
            }
        }
 
        public void Off(){

            PathRemovePerl();
            Console.Write("berrybrew perl disabled. Run 'berrybrew-refresh' to use the system perl\n");
        }

        public string Options(string option="", string value="", bool quiet=false) {

            if (Debug)
                Console.WriteLine("\noption: {0}, value: {1}\n", option, value);

            RegistryKey registry = Registry.LocalMachine.CreateSubKey(registrySubKey);

            if (option == "") {
                Console.WriteLine("\nOption configuration:\n");

                foreach (string opt in validOptions){
                    string optStr = String.Format("\t{0}:", opt);
                    Console.Write(optStr.PadRight(20, ' '));
                    string optVal = (string) registry.GetValue(opt, "");
                    Console.WriteLine(optVal);
                }
                return "";
            }
            
            if (! validOptions.Contains(option)) {
                Console.WriteLine("\n'{0}' is an invalid option...\n", option);
                Environment.Exit(0);
            }
            else {
                if (value == ""){
                    string optStr = String.Format("\n\t{0}:", option);
                    string optVal = (string) registry.GetValue(option, "");

                    if (! quiet) {
                        Console.Write(optStr.PadRight(20, ' '));
                        Console.WriteLine(optVal);
                    }
                    return optVal;
                }
                else {
                    try {
                        registry.SetValue(option, value);
                    }
                    catch (UnauthorizedAccessException e) {
                        Console.WriteLine("Writing to the registry requires Administrator privileges.\n");
                        if (Debug)
                            Console.WriteLine(e);

                        Environment.Exit(0);
                    }

                    string optStr = String.Format("\n\t{0}:", option);
                    string optVal = (string) registry.GetValue(option, "");

                    if (! quiet) {
                        Console.Write(optStr.PadRight(20, ' '));
                        Console.WriteLine(optVal);
                    }
                    return optVal;
                }
            }	
            return "";
        }

        public void OptionsUpdate() {

            dynamic jsonConf = JsonParse("config");
            
            try {
                if (Registry.LocalMachine.OpenSubKey(registrySubKey) != null) {

                    RegistryKey regKey =
                        Registry.LocalMachine.CreateSubKey(registrySubKey);

                    foreach (string confKey in validOptions) {
                        if (Debug)
                            Console.WriteLine("{0}: {1}", confKey, jsonConf[confKey]);
                        if (! regKey.GetValueNames().Contains(confKey))
                            Console.WriteLine("Adding {0} to the registry configuration", confKey);
                            regKey.SetValue(confKey, jsonConf[confKey]);
                    }
                }
            }
            catch (UnauthorizedAccessException err) {
                Console.WriteLine(
                    "\nInitializing berrybrew requires Administrator privileges");
                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }
        }

        private void PathAddBerryBrew(string binPath){

            string path = PathGet();
            List<string> newPath = new List<string>();

            if (path == null) {
                newPath.Add(binPath);
            }
            else {
                if (path[path.Length - 1] == ';') {
                    path = path.Substring(0, path.Length - 1);
                }

                newPath.Add(binPath);
                newPath.Add(path);
            }
            PathSet(newPath);
        }

        private void PathAddPerl(StrawberryPerl perl){

            string path = PathGet();
            List<string> newPath = perl.Paths;
            
            string[] entries = path.Split(new char [] {';'});

            foreach (string p in entries)
                newPath.Add(p);
        
            PathSet(newPath);
        }

        private static string PathGet(){

            const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\";
            string path = null;
            
            if (Registry.LocalMachine != null){
                path = (string) Registry.LocalMachine.OpenSubKey(keyName).GetValue(
                    "Path",
                    "",
                    RegistryValueOptions.DoNotExpandEnvironmentNames
                );
            }
            return path;
        }

        private static string PathGetUsr(){

            const string keyName = @"Environment\";
            string path = (string)Registry.CurrentUser.OpenSubKey(keyName).GetValue(
                "PATH",
                "",
                RegistryValueOptions.DoNotExpandEnvironmentNames
            );
            return path;
        }

        private void PathRemoveBerrybrew(){

            string path = PathGet();
            List<string> paths = path.Split(new char[] {';'}).ToList();
            List<string> updatedPaths = new List<string>();

            foreach (string pathEntry in paths){
                if (pathEntry != binPath)
                    updatedPaths.Add(pathEntry);
            }

            PathSet(updatedPaths);
        }

        private void PathRemovePerl(bool process=true){

            string path = PathGet();

            if (path == null) return;

            var paths = path.Split(new char[] {';'}).ToList();

            foreach (StrawberryPerl perl in _perls.Values){
                for (var i = 0; i < paths.Count; i++){
                    if (paths[i] == perl.PerlPath
                        || paths[i] == perl.CPath
                        || paths[i] == perl.PerlSitePath){
                        paths[i] = "";
                    }
                }
            }

            paths.RemoveAll(string.IsNullOrEmpty);

            if (process)
                PathSet(paths);
        }

        private static bool PathScan(string binPath, string target){
            
            var envTarget = target == "machine" ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;

            string paths = Environment.GetEnvironmentVariable("path", envTarget);

            foreach (string path in paths.Split(new char[]{';'})) {
                if (path == binPath)
                    return true;
            }

            return false;
        }

        private void PathSet(List<string> path){

            path.RemoveAll(string.IsNullOrEmpty);

            string paths = string.Join(";", path);

            if (! paths.EndsWith(@";"))
                paths += @";";

            try {
                const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
                using (RegistryKey pathKey = Registry.LocalMachine.OpenSubKey(keyName, true)){

                    pathKey.DeleteValue("Path");

                    pathKey.SetValue(
                        "Path",
                        paths,
                        RegistryValueKind.ExpandString
                    );
                }

                SendMessageTimeout(
                    HwndBroadcast,
                    WmSettingchange,
                    IntPtr.Zero,
                    "Environment",
                    SmtoAbortifhung,
                    100,
                    IntPtr.Zero
                );
            }

            catch(UnauthorizedAccessException err){
                Console.WriteLine("\nAdding berrybrew to the PATH requires Administrator privilege");
                if (Debug)
                    Console.WriteLine(err);
            }
        }

        private static string PerlarchivePath(StrawberryPerl perl){

            string path;

            try {
                if (! Directory.Exists(perl.archivePath))
                    Directory.CreateDirectory(perl.archivePath);

                return perl.archivePath + @"\" + perl.File;
            }

            catch (UnauthorizedAccessException){
                Console.WriteLine("Error, do not have permissions to create directory: " + perl.archivePath);
            }

            Console.WriteLine("Creating temporary directory instead");

            do {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));

            Directory.CreateDirectory(path);

            return path + @"\" + perl.File;
        }

        private List<string> PerlFindOrphans(){

            List<StrawberryPerl> perls = PerlsInstalled();
            
            try {
                Directory.GetDirectories(rootPath);
            }
            catch (Exception err) {
                if (Debug)
                {
                    Console.WriteLine("failure getting directories of root");
                    Console.WriteLine(err);
                }

                Environment.Exit(0);
            }
            
            List<string> dirs = new List<string>(Directory.GetDirectories(rootPath));
            List<string> perlInstallations = new List<string>();

            foreach (StrawberryPerl perl in perls)
                perlInstallations.Add(perl.installPath);

            List<string> orphans = new List<string>();

            foreach (string dir in dirs){
                if (dir == archivePath)
                    continue;
               
                // testing directory
                if (Regex.Match(dir, @"\\test$").Success)
                    continue;

                // dev build directory
                if (Regex.Match(dir, @"\\build$").Success)
                    continue;

                // module list directory
                if (Regex.Match(dir, @"\\modules$").Success)
                    continue;

                // cpanm storage directory
                if (Regex.Match(dir, @".cpanm").Success)
                    continue;

                // valid perl instance directory
                if (perlInstallations.Contains(dir))
                    continue;
                
                string dirBaseName = dir.Remove(0, rootPath.Length);
                orphans.Add(dirBaseName);
            }

            return orphans;
        }

        private void PerlGenerateObjects(bool importIntoObject=false){

            List<StrawberryPerl> perlObjects = new List<StrawberryPerl>();

            var perls = JsonParse("perls");
            var customPerls = JsonParse("perls_custom");
            var virtualPerls = JsonParse("perls_virtual");

            foreach (var perl in perls) {

                perlObjects.Add(
                    new StrawberryPerl(
                        this,
                        perl.name,
                        perl.file,
                        perl.url,
                        perl.ver,
                        perl.csum,
                        perl.newest == "true" ? true : false,
                        false // custom
                    )
                );
            }

            foreach (var perl in customPerls){
                perlObjects.Add(
                    new StrawberryPerl(
                        this,
                        perl.name,
                        perl.file,
                        perl.url,
                        perl.ver,
                        perl.csum,
                        perl.newest == "true" ? true : false,
                        true // custom
                    )
                );
            }

            foreach (var perl in virtualPerls) {

                perlObjects.Add(
                    new StrawberryPerl(
                        this,
                        perl.name,
                        perl.file,
                        perl.url,
                        perl.ver,
                        perl.csum,
                        perl.newest == "true" ? true : false,
                        false, // custom
                        true,  // virtual
                        perl.perl_path.ToString(),
                        perl.lib_path.ToString(),
                        perl.aux_path.ToString()
                    )
                );
            }
            
            if (!importIntoObject) return;

            foreach (StrawberryPerl perl in perlObjects)
                _perls.Add(perl.Name, perl);
        }

        public StrawberryPerl PerlInUse(){

            string path = PathGet();
            StrawberryPerl currentPerl = new StrawberryPerl();

            if (path != null){
                string[] paths = path.Split(new char[] {';'});
                foreach (StrawberryPerl perl in _perls.Values) {
                    if (paths.Any(t => t == perl.PerlPath
                                       || t == perl.CPath
                                       || t == perl.PerlSitePath))
                    {
                        currentPerl = perl;
                    }
                }
            }
            return currentPerl;
        }

        private static bool PerlIsInstalled(StrawberryPerl perl){
            return Directory.Exists(perl.installPath)
                   && File.Exists(perl.PerlPath + @"\perl.exe");
        }

        public List<StrawberryPerl> PerlsInstalled(){
            return _perls.Values.Cast<StrawberryPerl>().Where(PerlIsInstalled).ToList();
        }

        public void PerlRemove(string perlVersionToRemove){

            try {
                StrawberryPerl perl = PerlResolveVersion(perlVersionToRemove);
                StrawberryPerl currentPerl = PerlInUse();

                if (perl.Name == currentPerl.Name){
                    Console.WriteLine("Removing Perl " + perlVersionToRemove + " from PATH");
                    PathRemovePerl();
                }

                if (Directory.Exists(perl.installPath)){
                    try {
                        Console.WriteLine("Removing Strawberry Perl " + perlVersionToRemove);
                        FilesystemResetAttributes(perl.installPath);
                        Directory.Delete(perl.installPath, true);
                        Console.WriteLine("Successfully removed Strawberry Perl " + perlVersionToRemove);
                    }

                    catch (IOException err){
                        Console.WriteLine("Unable to completely remove Strawberry Perl " + perlVersionToRemove + " some files may remain");

                        if (Debug)
                            Console.WriteLine(err);
                    }
                }
                else {
                    Console.WriteLine("Strawberry Perl " + perlVersionToRemove + " not found (are you sure it's installed?)");
                    Environment.Exit(0);
                }

                if (perl.Custom){
                    dynamic customPerlList = JsonParse("perls_custom", true);
                    customPerlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(customPerlList);

                    List<Dictionary<string, object>> updatedPerls = new List<Dictionary<string, object>>();

                    foreach (Dictionary<string, object> perlStruct in customPerlList){
                        if (! perlVersionToRemove.Equals(perlStruct["name"].ToString()))
                            updatedPerls.Add(perlStruct);
                    }
                    JsonWrite("perls_custom", updatedPerls, true);
                }
                if (perl.Virtual){
                    dynamic virtualPerlList = JsonParse("perls_virtual", true);
                    virtualPerlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(virtualPerlList);
 
                    List<Dictionary<string, object>> updatedPerls = new List<Dictionary<string, object>>();
 
                    foreach (Dictionary<string, object> perlStruct in virtualPerlList){
                        if (! perlVersionToRemove.Equals(perlStruct["name"].ToString()))
                            updatedPerls.Add(perlStruct);
                    }
                    JsonWrite("perls_virtual", updatedPerls, true);
                }               
            }

            catch (ArgumentException err){
                Message.Print("perl_unknown_version");

                if (Debug)
                    Console.WriteLine(err);

                Environment.Exit(0);
            }

            catch (UnauthorizedAccessException err){
                Console.WriteLine("Unable to remove Strawberry Perl " + perlVersionToRemove + " permission was denied by System");

                if (Debug)
                    Console.WriteLine(err);
            }
        }

        public void PerlRegisterCustomInstall(string perlName, StrawberryPerl perlBase=new StrawberryPerl()){

            if (! Directory.Exists(rootPath + perlName)){
                Console.WriteLine("installation directory '" + perlName + "' does not exist");
                Environment.Exit(0);
            }

            if (! File.Exists(rootPath + perlName + @"\perl\bin\perl.exe")){
                Console.WriteLine("{0} is not a valid Perl installation", perlName);
                Environment.Exit(0);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            data["name"] = perlName;
            data["custom"] = perlBase.Custom;
            data["file"] = perlBase.File;
            data["url"] = perlBase.Url;
            data["ver"] = perlBase.Version;
            data["csum"] = perlBase.Sha1Checksum;

            List<Dictionary<string, object>> perlList = new List<Dictionary<string, object>> {data};

            JsonWrite("perls_custom", perlList);

            Console.WriteLine("Successfully registered {0}", perlName);

            bypassOrphanCheck = true;
        }

        public void PerlRegisterVirtualInstall(string perlName){

            if (!CheckName(perlName))
                Environment.Exit(0);

            Console.Write("\nSpecify the path to the perl binary: ");
            string perlPath = Console.ReadLine();
           
            Console.Write("\nSpecify the library path: ");
            string libPath = Console.ReadLine();           
            
            Console.Write("\nSpecify an additional path: ");
            string auxPath = Console.ReadLine();

            Console.Write("\n");
            
            bool perlPathValid = false;

            if (File.Exists(String.Format("{0}/perl.exe", perlPath))){
                perlPathValid = true;
            }
        
            if (! perlPathValid){
                Console.WriteLine(
                    "ERROR: {0} does not have a perl.exe binary. Can't register '{1}' perl instance'\n", 
                    perlPath, 
                    perlName
                );
                Environment.Exit(0);
            }
            if (! string.IsNullOrEmpty(libPath) && ! Directory.Exists(libPath)){
                Console.WriteLine("\n'{0}' library directory doesn't exist. Can't continue...\n", libPath);
                Environment.Exit(0);
            }
            if (! string.IsNullOrEmpty(auxPath) && ! Directory.Exists(auxPath)){
                Console.WriteLine("\n'{0}' auxillary directory doesn't exist. Can't continue...\n", auxPath);
                Environment.Exit(0);
            }           
            
            string instanceName = rootPath + perlName;
            
            if (!Directory.Exists(instanceName)) {
                // exit if the dir already exists!
                Directory.CreateDirectory(rootPath + perlName);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            data["name"] = perlName;
            data["custom"] = false;
            data["virtual"] = true;
            data["file"] = "";
            data["url"] = "";
            data["ver"] = "";
            data["csum"] = "";
            data["perl_path"] = perlPath;
            data["lib_path"] = libPath;
            data["aux_path"] = auxPath;

            List<Dictionary<string, object>> virtualPerlList = new List<Dictionary<string, object>> {data};

            JsonWrite("perls_virtual", virtualPerlList);

            Console.WriteLine("\nSuccessfully registered virtual perl {0}", perlName);

            bypassOrphanCheck = true;
        }
        
        public void PerlUpdateAvailableList(){

            Console.WriteLine("Attempting to fetch the updated Perls list...");
            
            using (WebClient client = new WebClient()){

                string jsonData = null;

                try {
                    jsonData = client.DownloadString(downloadURL);
                }

                catch (WebException error){
                    Console.Write("\nCan't open file {0}. Can not continue...\n", downloadURL);
                    if (Debug)
                        Console.Write(error);
                    Environment.Exit(0);
                }

                dynamic json = null;

                try {
                    json = JsonConvert.DeserializeObject(jsonData);
                }

                catch (JsonReaderException error){
                    Console.Write("\nCan't read the JSON data. It may be invalid\n");
                    if (Debug)
                        Console.WriteLine(error);
                    Environment.Exit(0);
                }

                List<String> perls = new List<String>();

                // output data
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                foreach (var release in json){
                    string nameString = release.name;

                    if (Regex.IsMatch(nameString, @"(with USE_64_BIT_INT|with USE_LONG_DOUBLE)"))
                        continue;

                    Match versionString = Regex.Match(nameString, @"(\d{1}\.\d{1,2}\.\d{1,2})");

                    if (versionString.Success){
                        Match bitString = Regex.Match(nameString, @"(\d{2})bit");

                        if (bitString.Success){
                            string version = versionString.Groups[1].Value;
                            string bits = bitString.Groups[1].Value;
                            string bbVersion = version + "_" + bits;

                            string[] majorVersionParts = version.Split(new char[] {'.'});
                            string majorVersion = majorVersionParts[0] + "." + majorVersionParts[1];
                            string bbMajorVersion = majorVersion + "_" + bits;

                            Dictionary<string, object> perlInstance = new Dictionary<string, object>();

                            if (release.edition.portable != null){
                                perlInstance.Add("name", bbVersion);
                                perlInstance.Add("url", release.edition.portable.url);
                                string file = release.edition.portable.url;
                                file = file.Split(new char[] {'/'}).Last();
                                perlInstance.Add("file", file);
                                perlInstance.Add("csum", release.edition.portable.sha1);
                                perlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (! perls.Contains(bbMajorVersion))
                                     perlInstance.Add("newest", true);
                                else
                                    perlInstance.Add("newest", false);                               

                                if (Debug){
                                    Console.WriteLine(
                                        "{0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }
                            }
                            else if (release.edition.zip != null){
                                perlInstance.Add("name", bbVersion);
                                perlInstance.Add("url", release.edition.zip.url);
                                string file = release.edition.zip.url;
                                file = file.Split(new char[] {'/'}).Last();
                                perlInstance.Add("file", file);
                                perlInstance.Add("csum", release.edition.zip.sha1);
                                perlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (! perls.Contains(bbMajorVersion))
                                     perlInstance.Add("newest", true);
                                else
                                    perlInstance.Add("newest", false);                               

                                if (Debug){
                                    Console.WriteLine(
                                        "{0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }
                            }

                            data.Add(perlInstance);

                            Dictionary<string, object> pdlInstance = new Dictionary<string, object>();

                            if (release.edition.pdl != null){
                                string pdlVersion = bbVersion + "_" + "PDL";
                                pdlInstance.Add("name", pdlVersion);
                                pdlInstance.Add("url", release.edition.pdl.url);
                                string file = release.edition.pdl.url;
                                file = file.Split(new char[] {'/'}).Last();
                                pdlInstance.Add("file", file);
                                pdlInstance.Add("csum", release.edition.pdl.sha1);
                                pdlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (Debug){
                                    Console.WriteLine(
                                        "{0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }
                                
                                if (! perls.Contains(bbMajorVersion))
                                     pdlInstance.Add("newest", true);
                                else
                                    pdlInstance.Add("newest", false);                               

                                data.Add(pdlInstance);
                            }

                            perls.Add(bbMajorVersion);
                        }
                    }
                } // end build data

                try {
                    JsonWrite("perls", data, true);
                }
                catch (System.UnauthorizedAccessException e){
                    Console.WriteLine("\nYou need to be running with elevated prvileges to run this command\n");
                    
                    if (Debug) {
                        Console.WriteLine(e);
                    }
                   
                    Environment.Exit(0);
                }
                
                Console.WriteLine("Successfully updated the available Perls list...");
            }
        
            PerlUpdateAvailableListOrphans();
        }

        public void PerlUpdateAvailableListOrphans(){

            List<string> orphans = PerlFindOrphans();

            foreach(var orphan in orphans){
                Console.WriteLine("Registering legacy Perl '{0}' as custom...", orphan);
                PerlRegisterCustomInstall(orphan);
            }
        }
       
        private StrawberryPerl PerlResolveVersion(string version){

            foreach (StrawberryPerl perl in _perls.Values){
                if (perl.Name == version)
                    return perl;
            }

            throw new ArgumentException("Unknown version: " + version);
        }
        
        private static Process ProcessCreate(string cmd, bool hidden=true){

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            if (hidden)
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + cmd;
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            return process;
        }
        
        public void Switch(string switchToVersion, bool switchQuick=false){

            try {
                StrawberryPerl perl = PerlResolveVersion(switchToVersion);

                if (! PerlIsInstalled(perl)){
                    Console.WriteLine(
                            "Perl version {0} is not installed. Run the command:\n\n\tberrybrew install",
                            perl.Name 
                    );
                    Environment.Exit(0);
                }

                PathRemovePerl();
                PathAddPerl(perl);

                if (switchQuick){
                    SwitchQuick();
                }
                
                Console.WriteLine(
                        "\nSwitched to Perl version {0}...\n\n",
                        switchToVersion
                );

                if (!switchQuick)
                    Console.WriteLine("Run 'berrybrew-refresh' to use it.\n");
            }
            catch (ArgumentException){
                Message.Print("perl_unknown_version");
                Environment.Exit(0);
            }
        }

        public void SwitchQuick (){
            string procName = Process.GetCurrentProcess().ProcessName;

            Process[] procList = Process.GetProcessesByName(procName);
            PerformanceCounter myParentID = new PerformanceCounter("Process", "Creating Process ID", procName);
            float parentPID = myParentID.NextValue();

            // Console.WriteLine("Parent for {0}: PID: {1}  Name: {2}", procName, parentPID , Process.GetProcessById((int)parentPID).ProcessName);

            for (int i = 1; i < procList.Length; i++)
            {
                PerformanceCounter myParentMultiProcID =
                    new PerformanceCounter("Process", "ID Process",
                        procName + "#" + i);

                parentPID = myParentMultiProcID.NextValue();
            }

            string cwd = Directory.GetCurrentDirectory();
            
            Process replacement = new Process();
            replacement.StartInfo.FileName = "cmd.exe";
            replacement.StartInfo.WorkingDirectory = cwd;
            replacement.StartInfo.EnvironmentVariables.Remove("PATH");
            replacement.StartInfo.EnvironmentVariables.Add("PATH", PathGet());
            replacement.StartInfo.UseShellExecute = false;
            replacement.StartInfo.RedirectStandardOutput = false;
            replacement.Start();

            // kill the original parent proc's cmd window
            
            Process.GetProcessById((int) parentPID).Kill();
        }
 
        public void Unconfig(){
            PathRemoveBerrybrew();
            Message.Print("unconfig");
        }

        public void UseCompile(string usePerlStr, bool newWindow = false){

            List<StrawberryPerl> perlsInstalled = PerlsInstalled();
            List<StrawberryPerl> useWith = new List<StrawberryPerl>();

            string[] perls = usePerlStr.Split(new char[] {','});

            foreach (string perlName in perls){

                bool perlAdded = false;
                
                foreach (StrawberryPerl perl in perlsInstalled){
                    if (perlName.Equals(perl.Name)){
                        useWith.Add(perl);
                        perlAdded = true;
                    }
                }
                
                if (! perlAdded){
                    Console.WriteLine(
                        "Can't launch Perl version {0}. It isn't installed.",
                        perlName
                    );
                }
            }

            if (! useWith.Any()){
                Console.WriteLine("\nThe selected Perl versions you specified are not installed.\n");
                Environment.Exit(0);
            }

            string sysPath = PathGet();
            string usrPath = PathGetUsr();

            foreach (StrawberryPerl perl in useWith){
                if (newWindow)
                    UseInNewWindow(perl, sysPath, usrPath);
                else
                    UseInSameWindow(perl, sysPath, usrPath);
            }
        }

        private static void UseInNewWindow(StrawberryPerl perl, string sysPath, string usrPath){
            try {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Normal};

                var newPath = perl.Paths;
                newPath.AddRange(Environment.ExpandEnvironmentVariables(sysPath).Split(new char[] {';'}).ToList());
                newPath.AddRange(Environment.ExpandEnvironmentVariables(usrPath).Split(new char[] {';'}).ToList());
                Environment.SetEnvironmentVariable("PATH", string.Join(";", newPath));

                string prompt = Environment.GetEnvironmentVariable("PROMPT");
                Environment.SetEnvironmentVariable("PROMPT", "$Lberrybrew use perl-" + perl.Name + "$G" + "$_" + prompt);

                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/k TITLE berrybrew use perl-" + perl.Name;
                process.StartInfo = startInfo;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
                Environment.SetEnvironmentVariable("PROMPT", prompt);   // reset before moving on
                string spawned = "berrybrew use " + perl.Name + ": spawned in new command window";
                if( null != Environment.GetEnvironmentVariable("BBTEST_SHOW_PID") ) {
                    spawned += ", with PID=" + process.Id;
                }
                Console.WriteLine(spawned);
                // possible test syntax: (build\berrybrew use 5.12,5.12.3_32) | perl -e "@pid = grep s/^^berrybrew use.*: spawned in new command-window, with PID=(\d+)\s*$/$1/, <STDIN>; sleep(2); print $_,$/ for @pid; sleep(2); kill 9, $_ for @pid"
            }
            catch(Exception objException) {
                Console.WriteLine(objException);
            }
        }

        private static void UseInSameWindow(StrawberryPerl perl, string sysPath, string usrPath){
            Console.WriteLine("perl-" + perl.Name + "\n==============");
            try {
                Process process = new Process {StartInfo = {WindowStyle = ProcessWindowStyle.Hidden}};

                var newPath = perl.Paths;
                newPath.AddRange(Environment.ExpandEnvironmentVariables(sysPath).Split(new char[] {';'}).ToList());
                newPath.AddRange(Environment.ExpandEnvironmentVariables(usrPath).Split(new char[] {';'}).ToList());

                Environment.SetEnvironmentVariable("PATH", string.Join(";", newPath));

                string prompt = Environment.GetEnvironmentVariable("PROMPT");
                Environment.SetEnvironmentVariable("PROMPT", "$_" + "$Lberrybrew use " + perl.Name + "$G: run \"exit\" leave this environment$_"+prompt);

                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                if( null != Environment.GetEnvironmentVariable("BBTEST_SHOW_PID") ) {
                    Console.WriteLine( "berrybrew use " + perl.Name + ": running in this command window, with PID=" + process.Id );
                }

                process.WaitForExit();

                Environment.SetEnvironmentVariable("PROMPT", prompt);   // reset before moving on
                Console.WriteLine("\nExiting <berrybrew use " + perl.Name + ">\n");
            }
            catch(Exception objException) {
                Console.WriteLine(objException);
            }
        }
        
        public void Upgrade(){

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            string backupDir = installPath + @"/backup_" + span.TotalSeconds;
            Directory.CreateDirectory(backupDir);

            if (Directory.Exists(configPath)){
                string[] files = Directory.GetFiles(configPath);

                foreach (string s in files){
                    string fileName = Path.GetFileName(s);
                    string destFile = Path.Combine(backupDir, fileName);
                    File.Copy(s, destFile, true);
                }
            }

            string cmd = "cd " + installPath + " && git pull";
            Process proc = ProcessCreate(cmd);
            proc.Start();

            while (! proc.StandardOutput.EndOfStream){
                string line = proc.StandardOutput.ReadLine();

                if (line == null || !Regex.Match(line, @"up-to-date").Success) continue;
                Console.WriteLine("\nberrybrew is already up to date\n");
                Environment.Exit(0);
            }

            bool error = false;
            List<string> errorReport = new List<string>();

            while (! proc.StandardError.EndOfStream){
                error = true;
                string line = proc.StandardError.ReadLine();
                errorReport.Add(line);
            }

            if (error){
                Console.WriteLine("\n\nError upgrading berrybrew:\n");

                foreach (string line in errorReport)
                    Console.WriteLine(line);

                Environment.Exit(0);
            }

            string[] bakFiles = Directory.GetFiles(backupDir);

            foreach (string s in bakFiles){
                string fileName = Path.GetFileName(s);

                if (!fileName.Equals(@"perls_custom.json")){
                    if (Debug)
                        Console.WriteLine("Not restoring the '{0}' config file.", fileName);

                    continue;
                }

                if (Debug)
                    Console.WriteLine("Restoring the '{0}' config file.", fileName);

                string destFile = Path.Combine(configPath, fileName);
                File.Copy(s, destFile, true);
            }

            OptionsUpdate();
            PerlUpdateAvailableListOrphans();
            
            Console.WriteLine("\nSuccessfully upgraded berrybrew\n");
        }
        
        public string Version(){
            return @"1.30";
        }
    }

    public class Message {

        private readonly OrderedDictionary _msgMap = new OrderedDictionary();

        public string Get(string label){

            return _msgMap[label].ToString();
        }

        public void Add(dynamic json){

            string content = null;

            foreach (string line in json.content)
                content += String.Format("{0}\n", line);

            _msgMap.Add(json.label.ToString(), content);
        }

        public void Print(string label){

            string msg = Get(label);
            Console.WriteLine(msg);
        }

        public void Say(string label){

            string msg = Get(label);
            Console.WriteLine(msg);
            Environment.Exit(0);
        }
    }

    public struct StrawberryPerl {

        public readonly string Name;
        public readonly string File;
        public readonly string Url;
        public readonly string Version;
        public readonly string Sha1Checksum;
        public readonly bool Newest;
        public readonly bool Custom;
        public readonly bool Virtual;
        public readonly string archivePath;
        public readonly string installPath;
        public readonly string CPath;
        public readonly string PerlPath;
        public readonly string PerlSitePath;
        public readonly List<string> Paths;

        public StrawberryPerl(
            Berrybrew bb, 
            object name, 
            object file, 
            object url, 
            object version, 
            object csum, 
            bool newest = false,
            bool custom = false,
            bool virtual_install = false,
            string perl_path = "",
            string lib_path = "",
            string aux_path = ""
            ){

            if (! virtual_install) {
                if (string.IsNullOrEmpty(perl_path))
                    perl_path = bb.rootPath + name + @"\perl\bin";

                if (string.IsNullOrEmpty(lib_path))
                    lib_path = bb.rootPath + name + @"\perl\site\bin";

                if (string.IsNullOrEmpty(aux_path))
                    aux_path = bb.rootPath + name + @"\c\bin";               
            }
            
            Name = name.ToString();
            Custom = custom;
            Newest = newest;
            Virtual = virtual_install;
            File = file.ToString();
            Url = url.ToString();
            Sha1Checksum = csum.ToString();
            Version = version.ToString();

            archivePath = bb.archivePath;
            installPath =  bb.rootPath + name;

            CPath = aux_path;
            PerlPath = perl_path;
            PerlSitePath = lib_path;
            
            Paths = new List <string>{
                CPath, PerlPath, PerlSitePath
            };

        }
    }
}    
