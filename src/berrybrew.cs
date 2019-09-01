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

        private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
        
        public readonly string ArchivePath;
        public readonly string InstallPath;
        public readonly string RootPath;
        
        private readonly string _binPath = AssemblyDirectory;
        private readonly string _confPath;
        private readonly string _downloadUrl;
        private readonly bool _windowsHomedir;
        
        // private readonly string _strawberryUrl; /* currently unneeded */
        private readonly bool _customExec;

        private bool _bypassOrphanCheck;

        public readonly Message Message = new Message();
        private readonly OrderedDictionary _perls = new OrderedDictionary();

        public Berrybrew(){

            InstallPath = Regex.Replace(_binPath, @"bin", "");
            _confPath = InstallPath + @"/data/";
            
            // config

            dynamic jsonConf = JsonParse("config");
            RootPath = jsonConf.root_dir + "\\";
            ArchivePath = jsonConf.temp_dir;
            // _strawberryUrl = jsonConf.strawberry_url; /* currently unneeded */
            _downloadUrl = jsonConf.download_url;
            _windowsHomedir = jsonConf.windows_homedir;
            
            if (jsonConf.custom_exec == "true")
                _customExec = true;

            Debug = jsonConf.debug;

            // ensure the Perl install dir exists

            CheckRootDir();

            // create the custom perls config file

            string customPerlsFile = _confPath + @"perls_custom.json";

            if (! File.Exists(customPerlsFile)) {
                File.WriteAllText(customPerlsFile, @"[]");
            }

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

            if (orphans.Count > 0 && ! _bypassOrphanCheck){
                string orphanedPerls = Message.Get("perl_orphans");
                Console.WriteLine("\nWARNING! {0}\n\n", orphanedPerls.Trim());
                foreach (string orphan in orphans)
                    Console.WriteLine("  {0}", orphan);
            }
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
                if (perl.Name == currentPerl.Name)
                    Console.Write(" *");

                Console.Write("\n");
            }
        }

        public void Available(){

            Message.Print("available_header");

            List<int> nameLengths = new List<int>();

            foreach (string perlName in _perls.Keys)
                nameLengths.Add(perlName.Length);

            int maxNameLength = nameLengths.Max();

            foreach (StrawberryPerl perl in _perls.Values){
                string perlNameToPrint = perl.Name + new String(' ', (maxNameLength - perl.Name.Length) + 2);
                Console.Write("\t" + perlNameToPrint);

                if (perl.Custom)
                    Console.Write(" [custom]");
                if (PerlIsInstalled(perl))
                    Console.Write(" [installed]");
                if (perl.Name == PerlInUse().Name)
                    Console.Write(" *");

                Console.Write("\n");
            }
            Message.Print("available_footer");
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
            if (Directory.Exists(RootPath)) return;

            try {
                Directory.CreateDirectory(RootPath);
            }

            catch (Exception err){
                Console.WriteLine("\nCouldn't create install dir {0}. Please create it manually and run config again", RootPath);
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
                        Console.WriteLine("\nremoved all files from {0} temp dir", RootPath);
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
            string moduleDir = RootPath + "modules\\";
            string[] moduleListFiles = Directory.GetFiles(moduleDir);

            if (! Directory.Exists(moduleDir)) {
                return true;
            }

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

            string buildDir = RootPath;
            string testDir = RootPath;
            
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
                Directory.Delete(RootPath + orphan, true);
                Console.WriteLine("removed orphan {0} perl instance", orphan);
            }

            return orphans.Count > 0;
        }

        private bool CleanTemp(){
            DirectoryInfo archiveDir = new DirectoryInfo(ArchivePath);
            FilesystemResetAttributes(archiveDir.FullName);

            List<FileInfo> zipFiles = archiveDir.GetFiles().ToList();

            foreach (FileInfo file in zipFiles)
                file.Delete();

            return zipFiles.Count > 0;
        }

        public bool Clone(string sourcePerlName, string destPerlName){

            if (! CheckName(destPerlName))
                return false;

            StrawberryPerl sourcePerl = PerlResolveVersion(sourcePerlName);
            string sourcePerlDir = sourcePerl.InstallPath;
            string destPerlDir = RootPath + destPerlName;
            DirectoryInfo src = new DirectoryInfo(sourcePerlDir);

            if (! src.Exists){
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourcePerlDir
                );
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
            Console.WriteLine(configIntro + Version() + "\n");

            if (! PathScan(new Regex("berrybrew.bin"), "machine")){
                PathAddBerryBrew(_binPath);

                Message.Print(PathScan(new Regex("berrybrew.bin"), "machine")
                    ? "config_success"
                    : "config_failure");
            }
            else
                Message.Print("config_complete");
        }

        public void ImportModules(string version=""){

            string moduleDir = RootPath + "modules\\";

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
            process.WaitForExit();
            
        }

        public void ExportModules(){
            StrawberryPerl perl = PerlInUse();

            if (string.IsNullOrEmpty(perl.Name)) {
                Console.WriteLine("\nno Perl is in use. Run 'berrybrew switch' to enable one before exporting a module list\n");
                Environment.Exit(0);
            }
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            string moduleDir = RootPath + "modules\\";

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
                if (perl.Custom && ! _customExec)
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

                    string fullZipToPath = Path.Combine(perl.InstallPath, entryFileName);
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
            string archivePath = PerlArchivePath(perl);

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

        public void Install(string version){

            StrawberryPerl perl = PerlResolveVersion(version);
            string archivePath = Fetch(perl);
            Extract(perl, archivePath);
            

            if (_windowsHomedir) {
                string homedirFile = perl.InstallPath + "/perl/vendor/lib/Portable/HomeDir.pm";
                
                if (File.Exists(homedirFile)) {
                    Console.WriteLine("file exists");
                    FileRemove(homedirFile);
                }
            }
            
            Available();
        }

        private dynamic JsonParse(string type, bool raw=false){

            string filename = string.Format("{0}.json", type);
            string jsonFile = _confPath + filename;

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

            if (!fullList){
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
            else{
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

            string writeFile = _confPath + type;
            writeFile = writeFile + @".json";

            File.WriteAllText(writeFile, jsonString);
        }

        public void Off(){

            PathRemovePerl();
            Console.Write("berrybrew perl disabled. Open a new shell to use system perl\n");
        }

        private void PathAddBerryBrew(string binPath){

            string path = PathGet();
            List<string> newPath = new List<string>();

            if (path == null)
                newPath.Add(binPath);
            else {
                if (path[path.Length - 1] == ';')
                    path = path.Substring(0, path.Length - 1);

                newPath.Add(path);
                newPath.Add(binPath);
            }
            PathSet(newPath);
        }

        private void PathAddPerl(StrawberryPerl perl){

            string path = PathGet();
            List<string> newPath = perl.Paths;
            newPath.Add(path);
            PathSet(newPath);
        }

        private static string PathGet(){

            const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\";
            string path = null;
            
            if (Registry.LocalMachine != null){
                path = (string) Registry.LocalMachine.OpenSubKey(keyName).GetValue(
                    "PATH",
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
            Regex binPath = new Regex("berrybrew.bin");
            List<string> paths = path.Split(new char[] {';'}).ToList();
            List<string> updatedPaths = new List<string>();

            foreach (string pathEntry in paths){
                if (! binPath.Match(pathEntry).Success)
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

        private static bool PathScan(Regex binPattern, string target){
            var envTarget = target == "machine" ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;

            string paths = Environment.GetEnvironmentVariable("path", envTarget);

            if (paths != null && paths.Split(new char[] {';'}).Any(path => binPattern.Match(path).Success)) return true;
            return false;
        }

        private void PathSet(List<string> path){

            path.RemoveAll(string.IsNullOrEmpty);

            try {
                const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
                Registry.LocalMachine.CreateSubKey(keyName).SetValue(
                    "Path",
                    string.Join(";", path),
                    RegistryValueKind.ExpandString
                );

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

        private static string PerlArchivePath(StrawberryPerl perl){

            string path;

            try {
                if (! Directory.Exists(perl.ArchivePath))
                    Directory.CreateDirectory(perl.ArchivePath);

                return perl.ArchivePath + @"\" + perl.File;
            }

            catch (UnauthorizedAccessException){
                Console.WriteLine("Error, do not have permissions to create directory: " + perl.ArchivePath);
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
                Directory.GetDirectories(RootPath);
            }
            catch (Exception err) {
                if (Debug)
                {
                    Console.WriteLine("failure getting directories of root");
                    Console.WriteLine(err);
                }

                Environment.Exit(0);
            }
            
            List<string> dirs = new List<string>(Directory.GetDirectories(RootPath));
            List<string> perlInstallations = new List<string>();

            foreach (StrawberryPerl perl in perls)
                perlInstallations.Add(perl.InstallPath);

            List<string> orphans = new List<string>();

            foreach (string dir in dirs){
                if (dir == ArchivePath)
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
                
                string dirBaseName = dir.Remove(0, RootPath.Length);
                orphans.Add(dirBaseName);
            }

            return orphans;
        }

        private void PerlGenerateObjects(bool importIntoObject=false){

            List<StrawberryPerl> perlObjects = new List<StrawberryPerl>();
            var perls = JsonParse("perls");
            var customPerls = JsonParse("perls_custom");

            foreach (var perl in perls){
                perlObjects.Add(
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

            foreach (var perl in customPerls){
                perlObjects.Add(
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
            return Directory.Exists(perl.InstallPath)
                   && File.Exists(perl.PerlPath + @"\perl.exe");
        }

        private List<StrawberryPerl> PerlsInstalled(){
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

                if (Directory.Exists(perl.InstallPath)){
                    try {
                        FilesystemResetAttributes(perl.InstallPath);
                        Directory.Delete(perl.InstallPath, true);
                        Console.WriteLine("Successfully removed Strawberry Perl " + perlVersionToRemove);
                    }

                    catch (IOException err){
                        Console.WriteLine("Unable to completely remove Strawberry Perl " + perlVersionToRemove + " some files may remain");

                        if (Debug)
                            Console.WriteLine(err);
                    }
                }
                else {
                    Console.WriteLine("Strawberry Perl " + perlVersionToRemove + " not found (are you sure it's installed?");
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

            if (! Directory.Exists(RootPath + perlName)){
                Console.WriteLine("installation directory '" + perlName + "' does not exist");
                Environment.Exit(0);
            }

            if (! File.Exists(RootPath + perlName + @"\perl\bin\perl.exe")){
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

            _bypassOrphanCheck = true;
        }
        
        public void PerlUpdateAvailableList(bool allPerls=false){

            Console.WriteLine("Attempting to fetch the updated Perls list...");
            
            using (WebClient client = new WebClient()){

                string jsonData = null;

                try {
                    jsonData = client.DownloadString(_downloadUrl);
                }

                catch (WebException error){
                    Console.Write("\nCan't open file {0}. Can not continue...\n", _downloadUrl);
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

                            if (perls.Contains(bbMajorVersion) && ! allPerls)
                                continue;

                            perls.Add(bbMajorVersion);

                            Dictionary<string, object> perlInstance = new Dictionary<string, object>();

                            if (release.edition.portable != null){
                                perlInstance.Add("name", bbVersion);
                                perlInstance.Add("url", release.edition.portable.url);
                                string file = release.edition.portable.url;
                                file = file.Split(new char[] {'/'}).Last();
                                perlInstance.Add("file", file);
                                perlInstance.Add("csum", release.edition.portable.sha1);
                                perlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

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

                                data.Add(pdlInstance);
                            }
                        }
                    }
                } // end build data

                JsonWrite("perls", data, true);
                
                Console.WriteLine("Successfully updated the available Perls list...");
            }
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

        public void Switch(string switchToVersion){

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

                Console.WriteLine(
                        "\nSwitched to Perl version {0}...\n\n",
                        switchToVersion
                );
            }
            catch (ArgumentException){
                Message.Print("perl_unknown_version");
                Environment.Exit(0);
            }
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

        public string Version(){
            return @"1.25";
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

        public void Upgrade(){

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            string backupDir = InstallPath + @"/backup_" + span.TotalSeconds;
            Directory.CreateDirectory(backupDir);

            if (Directory.Exists(_confPath)){
                string[] files = Directory.GetFiles(_confPath);

                foreach (string s in files){
                    string fileName = Path.GetFileName(s);
                    string destFile = Path.Combine(backupDir, fileName);
                    File.Copy(s, destFile, true);
                }
            }

            string cmd = "cd " + InstallPath + " && git pull";
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

                string destFile = Path.Combine(_confPath, fileName);
                File.Copy(s, destFile, true);
            }

            Console.WriteLine("\nSuccessfully upgraded berrybrew\n");
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
        public readonly bool Custom;
        public readonly string Url;
        public readonly string Version;
        public readonly string ArchivePath;
        public readonly string InstallPath;
        public readonly string CPath;
        public readonly string PerlPath;
        public readonly string PerlSitePath;
        public readonly List<string> Paths;
        public readonly string Sha1Checksum;

        public StrawberryPerl(Berrybrew bb, object name, object file, object url, object version, object csum, bool custom){

            Name = name.ToString();
            Custom = custom;
            File = file.ToString();
            Url = url.ToString();
            Version = version.ToString();
            ArchivePath = bb.ArchivePath;
            InstallPath =  bb.RootPath + name;
            CPath = bb.RootPath + name + @"\c\bin";
            PerlPath = bb.RootPath + name + @"\perl\bin";
            PerlSitePath = bb.RootPath + name + @"\perl\site\bin";
            Paths = new List <string>{
                CPath, PerlPath, PerlSitePath
            };
            Sha1Checksum = csum.ToString();
        }
    }
}
