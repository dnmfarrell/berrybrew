using BerryBrew.Messaging;
using BerryBrew.PathOperations;
using BerryBrew.PerlInstance;
using BerryBrew.PerlOperations;
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
using System.Security;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BerryBrew {
    public class Berrybrew {

        // prepares a setting change message to reconfigure PATH

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            IntPtr
            lpdwResult
        );
        internal static readonly IntPtr HwndBroadcast = new IntPtr(0xffff);
        internal const int WmSettingchange = 0x001a;
        internal const int SmtoAbortifhung = 0x2;

        private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);

        public enum ErrorCodes : int {
            GENERIC_ERROR                   = -1,
            SUCCESS                         = 0,
            ADMIN_BERRYBREW_INIT            = 5,
            ADMIN_FILE_ASSOC                = 10,
            ADMIN_PATH_ERROR                = 15,
            ADMIN_REGISTRY_WRITE            = 20,
            ARCHIVE_PATH_NAME_NOT_FOUND     = 25,
            BERRYBREW_UPGRADE_FAILED        = 30,
            DIRECTORY_CREATE_FAILED         = 40,
            DIRECTORY_LIST_FAILED           = 45,
            DIRECTORY_ALREADY_EXIST         = 47,
            DIRECTORY_NOT_EXIST             = 50,
            FILE_DELETE_FAILED              = 55,
            FILE_DOWNLOAD_FAILED            = 60,
            FILE_NOT_FOUND_ERROR            = 65,
            FILE_OPEN_FAILED                = 70,
            INFO_OPTION_INVALID_ERROR       = 75,
            INFO_OPTION_NOT_FOUND_ERROR     = 80,
            JSON_FILE_MALFORMED_ERROR       = 85,
            JSON_INVALID_ERROR              = 90,
            JSON_WRITE_FAILED               = 95,
            PERL_ALREADY_INSTALLED          = 98,
            PERL_ARCHIVE_CHECKSUM_FAILED    = 100,
            PERL_CLONE_FAILED               = 105,
            PERL_CLONE_FAILED_IO_ERROR      = 110,
            PERL_DIRECTORY_SPECIAL          = 112, 
            PERL_FILE_ASSOC_FAILED          = 115,
            PERL_INVALID_ERROR              = 120,
            PERL_MIN_VER_GREATER_510        = 125,
            PERL_NAME_COLLISION             = 127,
            PERL_NAME_INVALID               = 130,
            PERL_NONE_IN_USE                = 135,
            PERL_NONE_INSTALLED             = 140,
            PERL_NOT_INSTALLED              = 145,
            PERL_REMOVE_FAILED              = 150,
            PERL_TEMP_INSTANCE_NOT_ALLOWED  = 153,
            PERL_UNKNOWN_VERSION            = 155,
            PERL_VERSION_ALREADY_REGISTERED = 160,
            MODULE_IMPORT_FILE_UNAVAIL      = 165,
            MODULE_IMPORT_SAME_VERSION_ERROR= 170,
            MODULE_IMPORT_VERSION_REQUIRED  = 175,
            OPTION_INVALID_ERROR            = 180
        }

        public List<string> validOptions;

        // berrybrew command modifiers

        public bool Debug   { set; get; }
        public bool Testing { set; get; }
        public bool Trace   { set; get; }
        public bool Status  { set; get; }

        // related class objects

        public readonly Message Message = new Message();
        public PathOp PathOp = null;
        public PerlOp PerlOp = null;

        // Strawberry Perl instance initializers

        public readonly OrderedDictionary _perls = new OrderedDictionary();

        private string registrySubKey;

        private string binPath = AssemblyDirectory;
        public string archivePath;
        public string installPath;
        public string rootPath;
        private string configPath;
        private string snapshotPath;
        public string downloadURL;
        private bool windowsHomedir;

        private bool customExec;
        public bool bypassOrphanCheck = false;

        private const int MaxPerlNameLength = 25;

        public Berrybrew() {

            // related object instantiations

            PathOp = new PathOp(this);
            PerlOp = new PerlOp(this);

            // Initialize configuration

            installPath     = Regex.Replace(binPath, @"bin", "");
            configPath      = installPath + @"/data/";
            registrySubKey  = @"SOFTWARE\berrybrew";

            validOptions = new List<string>{
                "debug",
                "root_dir",
                "temp_dir",
                "strawberry_url",
                "download_url",
                "windows_homedir",
                "custom_exec",
                "run_mode",
                "file_assoc",
                "file_assoc_old",
                "shell",
                "warn_orphans",
            };

            // We need to change the configuration registry key names based on
            // the actual environment we're operating in outside of production

            if (binPath.Contains("testing")) {
                // Console.WriteLine("IN TEST MODE");
                registrySubKey += "-testing";
            }
            else if (binPath.Contains("staging")) {
                Console.WriteLine("IN DEV MODE");
                registrySubKey += "-staging";
            }

            // set the SSL security protocol

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // set initial registry configuration if it's not already done

            BaseConfig();

            // ensure the Perl install dir exists

            CheckRootDir();

            // create the custom and virtual perls config file

            string customPerlsFile = configPath + @"perls_custom.json";
            string virtualPerlsFile = configPath + @"perls_virtual.json";

            if (! File.Exists(customPerlsFile)) {
                File.WriteAllText(customPerlsFile, @"[]");
            }

            if (! File.Exists(virtualPerlsFile)) {
                File.WriteAllText(virtualPerlsFile, @"[]");
            }

            // messages

            dynamic jsonMessages = JsonParse("messages");

            foreach (dynamic entry in jsonMessages) {
                Message.Add(entry);
            }

            // perl instances

            List<StrawberryPerl> perlObjects = PerlOp.PerlGenerateObjects();

            foreach (StrawberryPerl perl in perlObjects) {
                if (! _perls.Contains(perl.Name)) {
                    _perls.Add(perl.Name, perl);
                }
            }
        }

        ~Berrybrew(){
            OrphanedPerls();
        }

        public void Available(bool allPerls=false) {
            Message.Print("available_header");

            List<int> nameLengths = new List<int>();

            foreach (string perlName in _perls.Keys) {
                nameLengths.Add(perlName.Length);
            }

            int maxNameLength = nameLengths.Max();

            foreach (StrawberryPerl perl in _perls.Values) {
                if (! allPerls && ! perl.Newest) {
                    if (! PerlOp.PerlIsInstalled(perl) && ! perl.Custom && ! perl.Virtual) {
                        continue;
                    }
                }

                string perlNameToPrint = perl.Name + new String(' ', (maxNameLength - perl.Name.Length) + 2);
                Console.Write("\t" + perlNameToPrint);

                if (PerlOp.PerlIsInstalled(perl)) {
                    Console.Write(" [installed] ");
                }
                if (perl.Custom) {
                    Console.Write("[custom]");
                }
                if (perl.Virtual) {
                    Console.Write("[virtual]");
                }
                if (perl.Name == PerlOp.PerlInUse().Name) {
                    Console.Write(" *");
                }
                Console.Write("\n");
            }
            Message.Print("available_footer");
        }

        public List<string> AvailableList(bool allPerls=false) {
            List<string> availablePerls = new List<string>();

            foreach (StrawberryPerl perl in _perls.Values) {
                if (! allPerls && ! perl.Newest) {
                    continue;
                }
                if (PerlOp.PerlIsInstalled(perl)) {
                    continue;
                }
                if (perl.Custom) {
                    continue;
                }
                if (perl.Virtual) {
                    continue;
                }
                if (perl.Name == PerlOp.PerlInUse().Name) {
                    continue;
                }

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
                        if (Debug) {
                            Console.WriteLine("DEBUG: {0}: {1}", confKey, jsonConf[confKey]);
                        }
                        regKey.SetValue(confKey, jsonConf[confKey]);
                    }
                }
            }
            catch (UnauthorizedAccessException err) {
                Console.Error.WriteLine("\nBase config of berrybrew requires Administrator privileges");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.ADMIN_REGISTRY_WRITE);
            }

            RegistryKey registry = Registry.LocalMachine.OpenSubKey(registrySubKey);

            rootPath = (string) registry.GetValue("root_dir", "");
            rootPath += @"\";
            
            snapshotPath = rootPath + @"snapshots\";

            archivePath = (string) registry.GetValue("temp_dir", "");

            downloadURL = (string) registry.GetValue("download_url", "");

            if ((string) registry.GetValue("windows_homedir", "false") == "true") {
                windowsHomedir = true;
            }

            if ((string) registry.GetValue("custom_exec", "false") == "true") {
                customExec = true;
            }

            if ((string) registry.GetValue("debug", "false") == "true") {
                Debug = true;
            }

            if ((string) registry.GetValue("warn_orphans", "false") == "false") {
                bypassOrphanCheck = true;
            }

            FileAssoc("", true);
        }

        public string BitSuffixCheck(string perlName) {
            if (Regex.Match(perlName, @"^5\.\d+\.\d+$").Success) {
                if (! Regex.Match(perlName, @"_32").Success && ! Regex.Match(perlName, @"_64").Success) {
                    return perlName + "_64";
                }
            }

            return perlName;
        }

        public static bool CheckName (string perlName) {

            if (perlName.Length > MaxPerlNameLength) {
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

        private void CheckRootDir() {
            if (Directory.Exists(rootPath)) {
                return;
            }

            try {
                Directory.CreateDirectory(rootPath);
            }
            catch (Exception err) {
                Console.Error.WriteLine("\nCouldn't create install dir {0}. Please create it manually and run config again", rootPath);
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.DIRECTORY_CREATE_FAILED);
            }
        }

        public void Clean(string subcmd="temp") {
            bool cleansed;

            switch (subcmd) {
                case "all":
                    CleanTemp();
                    CleanOrphan();
                    CleanModules();
                    CleanDev();
                    break;

                case "build":
                    cleansed = CleanBuild();
                    Console.WriteLine(
                        cleansed
                        ? "\nremoved the staging build directory"
                        : "\nan error has occured removing staging build directory"
                    );
                    break;

                case "dev":
                    cleansed = CleanDev();
                    Console.WriteLine(
                        cleansed
                        ? "\nremoved the staging and testing directories"
                        : "\nan error has occured removing dev directories"
                    );
                    break;

                case "modules":
                    cleansed = CleanModules();
                    Console.WriteLine(
                        cleansed
                        ? "\ncleaned the module list storage directory"
                        : "\nno module lists saved to remove"
                    );
                    break;

                case "orphan":
                    cleansed = CleanOrphan();
                    if (! cleansed) {
                        Console.WriteLine("\nno orphaned perls to remove");
                    }
                    break;

                case "temp":
                    cleansed = CleanTemp();
                    if (cleansed) {
                        Console.WriteLine("\nremoved all files from {0} temp dir", rootPath);
                    }
                    else {
                        Console.WriteLine("\nno archived perl installation files to remove");
                    }
                    break;
            }
        }

        private bool CleanBuild() {
            string runMode = Options("run_mode", null, true);

            if (runMode == "staging") {
                Console.Error.WriteLine("\nCan't remove staging build dir while in staging run_mode. Use 'bin\\berrybrew clean dev' instead");
                Exit(-1);
            }

            string stagingBuildDir = installPath;

            stagingBuildDir += @"staging";

            if (Debug) {
                Console.WriteLine("DEBUG: staging dir: {0}", stagingBuildDir);
            }
            try {
                if (Directory.Exists(stagingBuildDir)){
                    FilesystemResetAttributes(stagingBuildDir);
                    Directory.Delete(stagingBuildDir, true);
                }
            }
            catch (Exception err) {
                Console.Error.WriteLine("\nUnable to remove the staging build directory '{0}'", stagingBuildDir);
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
            }

            if (Directory.Exists(stagingBuildDir)) {
                return false;
            }

            return true;
        }

        private bool CleanDev() {
            string stagingDir = rootPath;
            string testingDir = rootPath;

            if (Testing) {
                stagingDir = stagingDir.Replace("\\staging", "");
                testingDir = testingDir.Replace("\\staging", "");
                stagingDir = stagingDir.Replace("\\testing", "");
                testingDir = testingDir.Replace("\\testing", "");
            }

            stagingDir += @"staging";
            testingDir = string.Format(@"{0}testing", testingDir);

            Console.WriteLine("{0}", stagingDir);

            if (Debug) {
                Console.WriteLine("DEBUG: staging dir: {0}", stagingDir);
                Console.WriteLine("DEBUG: testing dir: {0}", testingDir);
            }
            try {
                if (Directory.Exists(stagingDir)){
                    FilesystemResetAttributes(stagingDir);
                    Directory.Delete(stagingDir, true);
                }
            }
            catch (Exception err) {
                Console.Error.WriteLine("\nUnable to remove the staging directory");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
            }

            try {
                if (Directory.Exists(testingDir)) {
                    FilesystemResetAttributes(testingDir);
                    Directory.Delete(testingDir, true);
                }
            }
            catch (Exception err) {
                Console.Error.WriteLine("\nUnable to remove the testing directory");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
            }

            if (Directory.Exists(stagingDir)) {
                return false;
            }

            if (Directory.Exists(testingDir)) {
                return false;
            }

            return true;
        }

        private bool CleanModules() {
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
                Console.Error.WriteLine("\nUnable to clean up the module list directory");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.FILE_DELETE_FAILED);
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

        private bool CleanOrphan() {
            List<string> orphans = PerlOp.PerlOrphansFind();

            foreach (string orphan in orphans) {
                FilesystemResetAttributes(orphan);
                Directory.Delete(rootPath + orphan, true);
                Console.WriteLine("removed orphan {0} perl instance", orphan);
            }

            return orphans.Count > 0;
        }

        private bool CleanTemp() {
            if (! Directory.Exists(archivePath)) {
                return true;
            }

            DirectoryInfo archiveDir = new DirectoryInfo(archivePath);

            FilesystemResetAttributes(archiveDir.FullName);

            List<FileInfo> zipFiles = archiveDir.GetFiles().ToList();

            foreach (FileInfo file in zipFiles) {
                file.Delete();
            }

            return zipFiles.Count > 0;
        }

        public void Clone(string sourcePerlName, string destPerlName) {

            if (! CheckName(destPerlName)) {
                Exit(0);
            }

            StrawberryPerl sourcePerl = new StrawberryPerl();

            try {
                sourcePerl = PerlOp.PerlResolveVersion(sourcePerlName);
            }
            catch (System.ArgumentException e) {
                Console.Error.WriteLine("\n'{0}' is an unknown version of Perl. Can't clone.", sourcePerlName);
                if (Debug) {
                    Console.Error.WriteLine("\nDEBUG{0}", e);
                }
                Exit((int)ErrorCodes.PERL_UNKNOWN_VERSION);
            }

            string sourcePerlDir = sourcePerl.installPath;
            string destPerlDir = rootPath + destPerlName;
            DirectoryInfo src = new DirectoryInfo(sourcePerlDir);

            if (! src.Exists) {
                Console.Error.WriteLine("\nPerl instance '{0}' isn't installed. Can't clone.", sourcePerlName);
                Exit((int)ErrorCodes.PERL_NOT_INSTALLED);
            }

            try {
                Console.WriteLine("Attempting to clone {0} to {1}", sourcePerlName, destPerlName);

                if (! Directory.Exists(destPerlDir)) {
                    Directory.CreateDirectory(destPerlDir);
                }

                foreach (string dirPath in Directory.GetDirectories(sourcePerlDir, "*", SearchOption.AllDirectories)) {
                    Directory.CreateDirectory(dirPath.Replace(sourcePerlDir, destPerlDir));
                }

                foreach (string newPath in Directory.GetFiles(sourcePerlDir, "*.*", SearchOption.AllDirectories)) {
                    File.Copy(newPath, newPath.Replace(sourcePerlDir, destPerlDir), true);
                }

                if (! Directory.Exists(destPerlDir)) {
                    Console.Error.WriteLine("\nfailed to clone {0} to {1}", sourcePerlDir, destPerlDir);
                    Exit((int)ErrorCodes.PERL_CLONE_FAILED);
                }

                PerlOp.PerlRegisterCustomInstall(destPerlName, sourcePerl);

                Console.WriteLine("\nSuccessfully installed custom perl '{0}'", destPerlName);
            }
            catch (IOException err) {
                Console.Error.WriteLine("\nClone failed due to disk I/O error... ensure the disk isn't full\n");

                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.PERL_CLONE_FAILED_IO_ERROR);
            }
        }

        public void Config() {
            string configIntro = Message.Get("config_intro");

            configIntro = configIntro.Replace("\n", String.Empty);
            configIntro = configIntro.Replace("\r", String.Empty);

            Console.WriteLine("\n{0}{1}", configIntro, Version());

            if (! PathOp.PathScan(binPath, "machine")) {
                PathOp.PathAddBerryBrew(binPath);

                Message.Print(PathOp.PathScan(binPath, "machine")
                    ? "config_success"
                    : "config_failure");
            }
            else {
                Message.Print("config_complete");
            }
        }

        public void Download(string versionString) {
            List<string> available = AvailableList(false);

            if (versionString == "all") {
                foreach (string version in available) {
                    StrawberryPerl perl = PerlOp.PerlResolveVersion(version);
                    Fetch(perl);
                }
            }
            else {
                StrawberryPerl perl = PerlOp.PerlResolveVersion(versionString);
                Fetch(perl);
            }
        }

        private void Exec(StrawberryPerl perl, IEnumerable<string> parameters, string sysPath, bool singleMode) {
            if (! singleMode) {
                Console.WriteLine("perl-" + perl.Name + "\n==============");
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            var newPath = perl.Paths;
            newPath.Add(sysPath);

            Environment.SetEnvironmentVariable("PATH", string.Join(";", newPath));

            startInfo.FileName = "cmd.exe";
            List<String> patchedParams = new List<String>();

            foreach (String param in parameters) {
                if ( param.Contains(" ")) {
                     patchedParams.Add("\"" + param + "\"");
                }
                else {
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
                if (line.Data != null) {
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if (line.Data != null) {
                    Console.Error.WriteLine(line.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (Debug) {
                Console.WriteLine("DEBUG: Perl: {0}, Exit status: {1}\n", perl.Name, process.ExitCode);
            }

            if (singleMode) {
                Environment.ExitCode = process.ExitCode;
            }
            else if (process.ExitCode != 0) {
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: Non-zero exit code: Perl {0} returned with exit code {1}\n", perl.Name, process.ExitCode);
                }
                Environment.ExitCode = process.ExitCode;
            }
        }

        public void ExecCompile(List<String> parameters) {
            List<StrawberryPerl> perlsInstalled = PerlOp.PerlsInstalled();
            List<StrawberryPerl> execWith = new List<StrawberryPerl>();

            if (parameters.ElementAt(0).Equals("--with") && parameters.Count > 1) {
                parameters.RemoveAt(0);
                string perlsToUse = parameters.ElementAt(0);
                parameters.RemoveAt(0);

                List<string> perls = new List<string>();

                if (! perlsToUse.Contains(",")) {
                    perls.Add(perlsToUse);
                }
                else {
                     perls = new List<string>(perlsToUse.Split(new char[] {','}));
                }

                foreach (StrawberryPerl perl in perlsInstalled) {
                    foreach (string perlName in perls) {

                        if (BitSuffixCheck(perlName).Equals(perl.Name)) {
                            execWith.Add(perl);
                        }
                    }
                }
            }
            else {
                execWith = perlsInstalled;
            }

            string sysPath = PathOp.PathGet();

            List<StrawberryPerl> filteredExecWith = new List<StrawberryPerl>();

            foreach(StrawberryPerl perl in execWith) {
                if (perl.Custom && ! customExec) {
                    continue;
                }

                if (perl.Name.Contains("tmpl") || perl.Name.Contains("template")) {
                    continue;
                }

                filteredExecWith.Add(perl);
            }

            foreach (StrawberryPerl perl in filteredExecWith) {
                Exec(perl, parameters, sysPath, filteredExecWith.Count == 1);
            }

            if (Environment.ExitCode != 0) {
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: ExecCompile returned non-zero status: {0}\n", Environment.ExitCode);
                }
                Exit(Environment.ExitCode);
            }

            Exit(0);
        }

        public void Exit(int exitCode) {
            if (Debug) {
                Console.WriteLine("\nDEBUG: Exit code: {0}", exitCode);
            }

            if (Trace) {
                Console.Error.WriteLine("\nStack Trace:");

                StackTrace trace = new StackTrace();
                StackFrame[] frames = trace.GetFrames();

                foreach (StackFrame frame in frames) {
                    MethodBase info = frame.GetMethod();
                    Console.Error.WriteLine("\t{0}.{1}", info.ReflectedType.FullName, info.Name);
                }

                string exitCodeName = Enum.GetName(typeof(Berrybrew.ErrorCodes), exitCode);

                if (exitCodeName == null) {
                    exitCodeName = "EXTERNAL_PROCESS_ERROR";
                }

                Console.Error.WriteLine("\nExit code:\n\t{0} - {1}", exitCode, exitCodeName);
            }

            if (Status) {
                string exitCodeName = Enum.GetName(typeof(Berrybrew.ErrorCodes), exitCode);

                if (exitCodeName == null) {
                    exitCodeName = "EXTERNAL_PROCESS_ERROR";
                }

                Console.Error.WriteLine("\nExit code:\n\t{0} - {1}", exitCode, exitCodeName);
            }

            Environment.Exit(exitCode);
        }

        public void ExportModules() {
            // Check if we're 'use'-ing a temporary instance. We don't allow
            // module exports within one.

            string usingTempInstance = Environment.GetEnvironmentVariable("BERRYBREW_TEMP_USE");

            if (usingTempInstance == "true") {
                Console.WriteLine("\nExporting modules is not allowed from a temp ('use') perl instance\n\n");
                Exit((int)ErrorCodes.PERL_TEMP_INSTANCE_NOT_ALLOWED);
            }

            StrawberryPerl perl = PerlOp.PerlInUse();

            if (string.IsNullOrEmpty(perl.Name)) {
                Console.Error.WriteLine("\nno Perl is in use. Run 'berrybrew switch' to enable one before exporting a module list\n");
                Exit((int)ErrorCodes.PERL_NONE_IN_USE);
            }

            if (perl.Name == "5.10.1_32") {
                Console.Error.WriteLine("\nmodules command requires a Perl version greater than 5.10");
                Exit((int)ErrorCodes.PERL_MIN_VER_GREATER_510);
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden};

            string moduleDir = rootPath + "modules\\";

            if (! Directory.Exists(moduleDir)) {
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
                if(line.Data != null) {
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if(line.Data != null) {
                    Console.Error.WriteLine(line.Data);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0) {
                Exit(process.ExitCode);
            }

            Console.WriteLine("\nsuccessfully wrote out {0} module list file", moduleFile);
        }

        private void Extract(StrawberryPerl perl, string archivePath) {
            ZipFile zf = null;

            try {
                Console.WriteLine("Extracting {0}", archivePath);
                FileStream fs = File.OpenRead(archivePath);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf) {
                    if (! zipEntry.IsFile) {
                        continue;
                    }

                    string entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    string fullZipToPath = Path.Combine(perl.installPath, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (! string.IsNullOrEmpty(directoryName)) {
                        Directory.CreateDirectory(directoryName);
                    }
                    else {
                        Console.Error.WriteLine("\nCould not get the zip archive's directory name.\n");
                        Exit((int)ErrorCodes.ARCHIVE_PATH_NAME_NOT_FOUND);
                    }

                    using (FileStream streamWriter = File.Create(fullZipToPath)) {
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

        private string Fetch(StrawberryPerl perl) {
            WebClient webClient = new WebClient();
            string archivePath = PerlOp.PerlArchivePath(perl);

            if (! File.Exists(archivePath)) {
                try {
                    Console.WriteLine("Downloading " + perl.Url + " to " + archivePath);
                    webClient.DownloadFile(perl.Url, archivePath);
                }
                catch (WebException) {
                    Console.Error.WriteLine("\nUnable to download file. Check your Internet connection and/or the download site\n");
                    Exit((int)ErrorCodes.FILE_DOWNLOAD_FAILED);
                }
            }

            Console.WriteLine("Confirming checksum ... ");

            using (var cryptoProvider = new SHA1CryptoServiceProvider()) {
                using (var stream = File.OpenRead(archivePath)) {
                    string hash = BitConverter.ToString(cryptoProvider.ComputeHash(stream)).Replace("-", "").ToLower();

                    if (perl.Sha1Checksum != hash) {
                        Console.WriteLine("Error checksum of downloaded archive \n"
                            + archivePath
                            + "\ndoes not match expected output\nexpected: "
                            + perl.Sha1Checksum
                            + "\n     got: " + hash);
                        stream.Dispose();
                        Console.Write("Would you like berrybrew to delete the corrupted download file? y/n [n]");

                        if (Console.ReadLine() == "y") {
                            string retval = FileRemove(archivePath);

                            if (retval == "True") {
                                Console.WriteLine("Deleted! Try to install it again!");
                            }
                            else {
                                Console.Error.WriteLine("Unable to delete " + archivePath);
                            }
                        }

                        Exit((int)ErrorCodes.PERL_ARCHIVE_CHECKSUM_FAILED);
                    }
                    else {
                        Console.WriteLine("Checksum OK");
                    }
                }
            }
            return archivePath;
        }

        public void FileAssoc(string action="", bool quiet=false) {
            string plExtSubKey = @".pl";
            string plHandlerNameOld = null;

            try {
                // assoc registry key
                RegistryKey plExtKey = Registry.ClassesRoot.CreateSubKey(plExtSubKey);
                plHandlerNameOld = (string) plExtKey.GetValue("");

                if (plHandlerNameOld == null) {
                    plHandlerNameOld = "";
                }

                if (action == "set") {
                    StrawberryPerl perl = PerlOp.PerlInUse();

                    if (String.IsNullOrEmpty(perl.PerlPath)) {
                        Console.Error.WriteLine("\nNo berrybrew Perl in use, can't set file association.\n");
                        Exit((int)ErrorCodes.PERL_NONE_IN_USE);
                    }

                    if (plHandlerNameOld == @"berrybrewPerl") {
                        RegistryKey plHandlerKeyOld = Registry.ClassesRoot.CreateSubKey(plHandlerNameOld + @"\shell\open\command");
                        plHandlerKeyOld.SetValue("", perl.PerlPath + "\\perl.exe \"%1\" %*");
                        return;
                    }

                    Options("file_assoc_old", plHandlerNameOld, true);
                    string plHandlerName = @"berrybrewPerl";
                    RegistryKey plHandlerKey = Registry.ClassesRoot.CreateSubKey(plHandlerName + @"\shell\open\command");
                    plHandlerKey.SetValue("", perl.PerlPath + "\\perl.exe \"%1\" %*");

                    plExtKey.SetValue("", plHandlerName);
                    Options("file_assoc", plHandlerName, true);

                    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                    Console.WriteLine("\nberrybrew is now managing the Perl file association");
                }
                else if (action == "unset") {
                    string old_file_assoc = Options("file_assoc_old", null, true);

                    plExtKey.SetValue("", old_file_assoc);
                    Options("file_assoc_old", null, true);
                    Options("file_assoc", old_file_assoc, true);

                    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                    Console.WriteLine("\nSet Perl file association back to default");
                }
                else {
                    if (Options("file_assoc", null, true) != plHandlerNameOld) {
                        Options("file_assoc", plHandlerNameOld, true);
                    }
                    if (! quiet) {
                        Console.WriteLine("\nPerl file association handling:");
                        Console.WriteLine("\n\tHandler:\t{0}", Options("file_assoc", null, true));
                    }
                }
            }
            catch (UnauthorizedAccessException err) {
                if (! quiet) {
                    Console.Error.WriteLine("\nChanging file associations requires Administrator privileges");
                }
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
            }
        }

        private static string FileRemove(string filename) {
            try {
                File.Delete(filename);
            }
            catch (Exception ex) {
                return ex.ToString();
            }

            return true.ToString();
        }

        public static void FilesystemResetAttributes(string currentDir) {
            if (! Directory.Exists(currentDir)) {
                return;
            }

            string[] subDirs = Directory.GetDirectories(currentDir);
            foreach(string dir in subDirs) {
                FilesystemResetAttributes(dir);
            }

            string[] files = Directory.GetFiles(currentDir);
            foreach (string file in files) {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        public void ImportModules(string version="") {
            string moduleDir = rootPath + "modules\\";

            if (! Directory.Exists(moduleDir)) {
                Directory.CreateDirectory((moduleDir));
            }

            if (version == "") {
                string[] moduleListFiles = Directory.GetFiles(moduleDir);

                if (moduleListFiles.Length == 0) {
                    Console.Error.WriteLine("\nno module lists to import from. Run 'berrybrew modules export', then re-run the import command...\n");
                    Exit((int)ErrorCodes.MODULE_IMPORT_FILE_UNAVAIL);
                }

                Console.Error.WriteLine("\nre-run the command with one of the following options:\n");

                foreach (string fileName in moduleListFiles) {
                    if (fileName.Contains("~")) {
                        continue;
                    }
                    Console.Error.WriteLine(Path.GetFileName(fileName));
                }

                Console.Error.WriteLine();
                Exit((int)ErrorCodes.MODULE_IMPORT_VERSION_REQUIRED);
            }
            else {
                ImportModulesExec(version, moduleDir + version);
            }
        }

        private void ImportModulesExec(string file, string path) {
            if (file == PerlOp.PerlInUse().Name) {
                Console.Error.WriteLine("\nCan't import modules exported from the same perl version\n");
                Console.Error.WriteLine("You're trying to use an export from version {0} and you're on {1}\n", file, arg1: PerlOp.PerlInUse().Name);
                Exit((int)ErrorCodes.MODULE_IMPORT_SAME_VERSION_ERROR);
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
                if(line.Data != null) {
                    Console.Out.WriteLine(line.Data);
                }
            };
            process.ErrorDataReceived += (proc, line)=>{
                if(line.Data != null) {
                    Console.Error.WriteLine(line.Data);
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process.ExitCode != 0) {
                Exit(process.ExitCode);
            }
        }

        public void Info(string want) {
            List <string> options = new List<string>(){"install_path", "bin_path", "root_path", "archive_path", "snapshot_path"};

            if (! options.Contains(want)) {
                Console.Error.WriteLine("\n'{0}' is not a valid option. Valid options are:\n", want);
                foreach (string opt in options){
                    Console.Error.WriteLine("\t{0}", opt);
                }
                Exit((int)ErrorCodes.INFO_OPTION_INVALID_ERROR);
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
                case "snapshot_path":
                    Console.WriteLine("\n\t{0}", snapshotPath);
                    break;
                default:
                    Console.Error.WriteLine("\nCould not fetch details for '{0}'", want);
                    Exit((int)ErrorCodes.INFO_OPTION_NOT_FOUND_ERROR);
                    break;
            }
        }

        public void Install(string version) {
            StrawberryPerl perl = new StrawberryPerl();

            try {
                perl = PerlOp.PerlResolveVersion(version);
            }
            catch (System.ArgumentException err) {
                Console.Error.WriteLine("\n'{0}' is an unknown version of Perl. Can't install.", version);
                if (Debug) {
                    Console.Error.WriteLine("\nDEBUG{0}", err);
                }
                Exit((int)ErrorCodes.PERL_UNKNOWN_VERSION);
            }

            if (PerlOp.PerlIsInstalled(perl)) {
                Console.Error.WriteLine("Perl version {0} is already installed.", perl.Name);
                Exit((int)ErrorCodes.PERL_ALREADY_INSTALLED);
            }

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

        public dynamic JsonParse(string type, bool raw=false) {
            string filename = string.Format("{0}.json", type);
            string jsonFile = configPath + filename;

            try {
                using (StreamReader r = new StreamReader(jsonFile)) {

                    string jsonData = r.ReadToEnd();

                    if (raw) {
                        return jsonData;
                    }

                    try {
                        dynamic json= JsonConvert.DeserializeObject(jsonData);
                        return json;
                    }
                    catch (JsonReaderException error){
                        Console.Error.WriteLine("\n{0} file is malformed. See berrybrew_error.txt in this directory for details.", jsonFile);
                        using (StreamWriter file = new StreamWriter(@"berrybrew_error.txt", true)) {
                            file.WriteLine(error);
                        }
                        Exit((int)ErrorCodes.JSON_FILE_MALFORMED_ERROR);
                    }
                }
            }
            catch (FileNotFoundException err) {
                Console.Error.WriteLine("\n{0} file can not be found in {1}", filename, jsonFile);

                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.FILE_NOT_FOUND_ERROR);
            }
            return "";
        }

        public void JsonWrite(string type, List<Dictionary<string, object>> data, bool fullList=false) {
            string jsonString;

            if (! fullList && type == "perls_custom") {
                dynamic customPerlList = JsonParse("perls_custom", true);
                var perlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(customPerlList);

                foreach (Dictionary<string, object> perl in data) {
                    foreach (Dictionary<string, object> existingPerl in perlList) {
                        if (perl["name"].Equals(existingPerl["name"])) {
                            Console.Error.Write("\n{0} instance is already registered...", perl["name"]);
                            Exit((int)ErrorCodes.PERL_VERSION_ALREADY_REGISTERED);
                        }
                    }

                    perlList.Add(perl);
                }
                jsonString = JsonConvert.SerializeObject(perlList, Formatting.Indented);
            }
            else if (! fullList && type == "perls_virtual") {
                dynamic virtualPerlList = JsonParse("perls_virtual", true);
                var perlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(virtualPerlList);

                foreach (Dictionary<string, object> perl in data) {
                    foreach (Dictionary<string, object> existingPerl in perlList) {
                        if (perl["name"].Equals(existingPerl["name"])) {
                            Console.Error.Write("\n{0} instance is already registered...", perl["name"]);
                            Exit((int)ErrorCodes.PERL_VERSION_ALREADY_REGISTERED);
                        }
                    }
                    perlList.Add(perl);
                }
                jsonString = JsonConvert.SerializeObject(perlList, Formatting.Indented);
            }
            else {
                List<string> perlVersions = new List<string>();

                foreach (var perl in data) {
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

                foreach (var ver in sortedPerlVersions) {
                    foreach (var perl in data) {
                        if (! perl["ver"].Equals(ver)) {
                            continue;
                        }

                        if (perlCache.Contains(perl["name"].ToString())) {
                            continue;
                        }

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

        public void List() {
            StrawberryPerl currentPerl = PerlOp.PerlInUse();

            List<int> nameLengths = new List<int>();
            List<StrawberryPerl> installedPerls = PerlOp.PerlsInstalled();

            // Ensure we list orphaned Perls
            bypassOrphanCheck = false;

            if (! installedPerls.Any()) {
                Console.Error.Write("\nNo versions of Perl are installed.\n");
                Exit((int)ErrorCodes.PERL_NONE_INSTALLED);
            }

            foreach (StrawberryPerl perl in installedPerls) {
                nameLengths.Add(perl.Name.Length);
            }

            int maxNameLength = nameLengths.Max();

            foreach(StrawberryPerl perl in installedPerls) {
                string perlNameToPrint = perl.Name + new String(' ', (maxNameLength - perl.Name.Length) + 2);
                Console.Write("\t" + perlNameToPrint);

                if (perl.Custom) {
                    Console.Write(" [custom]");
                }
                if (perl.Virtual) {
                    Console.Write(" [virtual]");
                }
                if (perl.Name == currentPerl.Name) {
                    Console.Write(" *");
                }
                Console.Write("\n");
            }
        }

        public void Off() {
            PathOp.PathRemovePerl(_perls);
            Console.Write("berrybrew perl disabled. Run 'berrybrew-refresh' to use the system perl\n");
        }

        public string Options(string option=null, string value=null, bool quiet=false) {
            RegistryKey registry = null;

            try {
                registry = Registry.LocalMachine.OpenSubKey(registrySubKey, true);
            }
            catch (NullReferenceException e) {
                if (Debug) {
                    Console.Error.WriteLine("\nberrybrew registry section doesn't exist:\n {0}", e);
                }
            }
            catch (System.Security.SecurityException) {
                try {
                    registry = Registry.LocalMachine.OpenSubKey(registrySubKey);
                }
                catch (NullReferenceException e) {
                    if (Debug) {
                        Console.Error.WriteLine("\nberrybrew registry section doesn't exist:\n {0}", e);
                    }
                }
            }
            if (registry == null) {
                try {
                    registry = Registry.LocalMachine.CreateSubKey(registrySubKey, true);
                }
                catch (UnauthorizedAccessException e) {
                    Console.WriteLine("\nThe command you specified requires Administrator privileges.\n");
                    if (Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", e);
                    }
                }
                Exit((int)ErrorCodes.ADMIN_REGISTRY_WRITE);
            }

            if (option == null) {
                Console.WriteLine("\nOption configuration:\n");

                foreach (string opt in validOptions) {
                    string optStr = String.Format("\t{0}:", opt);
                    Console.Write(optStr.PadRight(20, ' '));
                    string optVal = (string) registry.GetValue(opt, "");
                    Console.WriteLine(optVal);
                }
                return "";
            }

            if (! validOptions.Contains(option)) {
                Console.Error.WriteLine("\n'{0}' is an invalid option...\n", option);
                Exit((int)ErrorCodes.OPTION_INVALID_ERROR);
            }
            else {
                if (value == null) {
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
                    catch (UnauthorizedAccessException err) {
                        Console.Error.WriteLine("Setting options in the registry requires Administrator privileges.\n");
                        if (Debug) {
                            Console.Error.WriteLine("DEBUG: {0}", err);
                        }
                        Exit((int)ErrorCodes.ADMIN_REGISTRY_WRITE);
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

        public void OptionsUpdate(bool force=false) {
            dynamic jsonConf = JsonParse("config");

            try {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(registrySubKey, true);

                string[] regValues = regKey.GetValueNames();

                foreach (string confKey in validOptions) {
                    if (Debug) {
                        Console.WriteLine("DEBUG: {0}: {1}", confKey, jsonConf[confKey]);
                    }
                    if (force) {
                        Console.WriteLine("Adding {0} to the registry configuration", confKey);
                        regKey.SetValue(confKey, jsonConf[confKey]);
                    }
                    else {
                        if (! regValues.Contains(confKey)) {
                            Console.WriteLine("Adding {0} to the registry configuration", confKey);
                            regKey.SetValue(confKey, jsonConf[confKey]);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException err) {
                Console.Error.WriteLine("\nInitializing berrybrew requires Administrator privileges");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.ADMIN_BERRYBREW_INIT);
            }
        }

        public void OrphanedPerls() {
            List<string> orphans = PerlOp.PerlOrphansFind();

            if (orphans.Count > 0 && ! bypassOrphanCheck) {
                Message.Print("perl_orphans");
                foreach (string orphan in orphans) {
                    Console.WriteLine("  {0}", orphan);
                }
            }
        }

        public Process ProcessCreate(string cmd=null, bool hidden=true) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            if (hidden) {
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + cmd;
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            return process;
        }
        
        public void SnapshotCompress(string instanceName, string snapshotName = null) {
            SnapshotInit();
            List<StrawberryPerl> installedPerls = PerlOp.PerlsInstalled();
 
            bool instanceFound = false;

            foreach (StrawberryPerl installedPerl in installedPerls) {
                if (installedPerl.Name == instanceName) {
                    instanceFound = true;
                }
            }

            if (! instanceFound) {
                Console.Error.WriteLine("\nPerl instance {0} not found", instanceName);
                Exit((int)ErrorCodes.PERL_UNKNOWN_VERSION);           
            }

            StrawberryPerl perl = PerlOp.PerlResolveVersion(instanceName);
            
            string snapshotFile = "";
            
            if (snapshotName == null) {
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss"); 
                snapshotFile = perl.Name + @"." + timeStamp;
            }
            else {
                snapshotFile = snapshotName;
            }

            if (! Regex.Match(snapshotFile, @".zip$").Success) {
                snapshotFile = snapshotFile + @".zip";
            }

            snapshotFile = snapshotPath + snapshotFile;

            Console.WriteLine(
                "Creating snapshot of perl '{0}' to file '{1}'",
                perl.Name,
                snapshotFile
            );
            
            FastZip _FastZip = new FastZip();
            _FastZip.CreateZip(snapshotFile, perl.installPath, true, "");
        }

        public void SnapshotExtract(string snapshotName, string instanceName = null) {
            SnapshotInit();

            if (instanceName == null) {
                // Remove the timestamp 
                instanceName = Regex.Replace(snapshotName, @".\d{14}", "");
            }

            List<string> perlsAvailable = AvailableList();

            if (perlsAvailable.Contains(instanceName)) {
                Console.Error.WriteLine(
                    "\nname portion ({0}) of snapshot ({1}) can't match an existing official perl name. You must specify an alternate instance name",
                    instanceName,
                    snapshotName
                );
                Exit((int) ErrorCodes.PERL_NAME_COLLISION);
            }
        
            List<StrawberryPerl> installedPerls = PerlOp.PerlsInstalled();
 
            foreach (StrawberryPerl installedPerl in installedPerls) {
                if (installedPerl.Name == instanceName) {
                    Console.Error.WriteLine(
                        "\nPerl instance name '{0}' already installed...",
                        instanceName 
                    );
                    Exit((int) ErrorCodes.PERL_ALREADY_INSTALLED);                   
                }
            }
            
            ZipFile zf = null;

            try {
                string snapshotFile = snapshotPath + snapshotName + @".zip";

                if (! File.Exists(snapshotFile)) {
                    Console.Error.WriteLine(
                        "\nSnapshot file {0} can't be found is the {1} name correct?",
                        snapshotFile,
                        snapshotName
                    );
                    Exit((int) ErrorCodes.FILE_NOT_FOUND_ERROR);
                }
                
                string instanceInstallDir = rootPath + instanceName;

                if (Directory.Exists(instanceInstallDir)) {
                    Console.Error.WriteLine(
                        "\nDirectory {0} already exists. Can't extract snapshot {1} to perl instance name '{2}'\n",
                        instanceInstallDir,
                        snapshotFile,
                        instanceName
                    );
                    Exit((int) ErrorCodes.DIRECTORY_ALREADY_EXIST);                       
                }

                Console.WriteLine("Extracting snapshot '{0}' from file {1} to {2}\n",
                    snapshotName,
                    snapshotFile,
                    instanceInstallDir
                );

                FileStream fs = File.OpenRead(snapshotFile);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf) {
                    if (!zipEntry.IsFile) {
                        continue;
                    }

                    string entryFileName = zipEntry.Name;

                    byte[] buffer = new byte[4096]; // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);
                    
   
                    string fullZipToPath = Path.Combine(instanceInstallDir, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (! string.IsNullOrEmpty(directoryName)) {
                        Directory.CreateDirectory(directoryName);
                    }
                    else {
                        Console.Error.WriteLine(
                            "\nCould not get the zip archive's directory name.\n");
                        Exit((int) ErrorCodes.ARCHIVE_PATH_NAME_NOT_FOUND);
                    }

                    using (FileStream
                        streamWriter = File.Create(fullZipToPath)) {
                        ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipStream,
                            streamWriter, buffer);
                    }
                }
            }
            finally {
                if (zf != null){
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }

            PerlOp.PerlRegisterCustomInstall(instanceName);
        }

        private void SnapshotInit() {
            if (!Directory.Exists(snapshotPath)) {
                try {
                    Directory.CreateDirectory(snapshotPath);
                }
                catch (Exception err) {
                    Console.Error.WriteLine(
                        "\nCouldn't create snapshot dir {0}. Please create it manually and run your command again",
                        snapshotPath
                    );
                    
                    if (Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", err);
                    }

                    Exit((int) ErrorCodes.DIRECTORY_CREATE_FAILED);
                }
            }
        }

        public void SnapshotList() {
            SnapshotInit();

            string[] files = Directory.GetFiles(snapshotPath);

            if (files.Length == 0) {
                Console.WriteLine("no snapshots have been saved...");
            }
            else {
                Console.WriteLine(
                    "snapshot directory {0} has the following snapshots...\n",
                    snapshotPath
                );

                foreach (string file in files) {
                    string fileName = Path.GetFileName(file);
                    fileName = Regex.Replace(fileName, @".zip", "");
                    Console.WriteLine("\t{0}", fileName);
                }
            }
        }

        public void Switch(string switchToVersion, bool switchQuick=false) {
            switchToVersion = BitSuffixCheck(switchToVersion);

            try {
                StrawberryPerl perl = PerlOp.PerlResolveVersion(switchToVersion);

                if (! PerlOp.PerlIsInstalled(perl)) {
                    Console.Error.WriteLine(
                            "Perl version {0} is not installed. Run the command:\n\n\tberrybrew install",
                            perl.Name
                    );
                    Exit((int)ErrorCodes.PERL_NOT_INSTALLED);
                }

                PathOp.PathRemovePerl(_perls);
                PathOp.PathAddPerl(perl);

                if (switchQuick) {
                    SwitchQuick();
                }

                if (Options("file_assoc", null, true) == "berrybrewPerl") {
                    FileAssoc("set", true);
                }

                Console.WriteLine("\nSwitched to Perl version {0}...\n\n",switchToVersion);

                if (! switchQuick) {
                    Console.WriteLine("Run 'berrybrew-refresh' to use it.\n");
                }
            }
            catch (SecurityException err) {
                Console.Error.WriteLine("\nSwitching Perls requires Administrator privileges");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.ADMIN_REGISTRY_WRITE);
            }
            catch (ArgumentException) {
                Message.Error("perl_unknown_version");
                Exit((int)ErrorCodes.PERL_UNKNOWN_VERSION);
            }
            catch (UnauthorizedAccessException err) {
                Console.Error.WriteLine("\nSwitching Perls requires Administrator privileges");
                if (Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }
                Exit((int)ErrorCodes.ADMIN_REGISTRY_WRITE);
            }
        }

        public void SwitchQuick () {
            string procName = Process.GetCurrentProcess().ProcessName;

            Process[] procList = Process.GetProcessesByName(procName);
            PerformanceCounter myParentID = new PerformanceCounter("Process", "Creating Process ID", procName);
            float parentPID = myParentID.NextValue();

            for (int i = 1; i < procList.Length; i++) {
                PerformanceCounter myParentMultiProcID =
                    new PerformanceCounter("Process", "ID Process", procName + "#" + i);

                parentPID = myParentMultiProcID.NextValue();
            }

            string cwd = Directory.GetCurrentDirectory();

            Process replacement = new Process();
            replacement.StartInfo.FileName = "cmd.exe";
            replacement.StartInfo.WorkingDirectory = cwd;
            replacement.StartInfo.EnvironmentVariables.Remove("PATH");
            replacement.StartInfo.EnvironmentVariables.Add("PATH", PathOp.PathGet());
            replacement.StartInfo.UseShellExecute = false;
            replacement.StartInfo.RedirectStandardOutput = false;
            replacement.Start();

            // kill the original parent proc's cmd window

            Process.GetProcessById((int) parentPID).Kill();
        }

        public void Unconfig() {
            PathOp.PathRemoveBerrybrew(binPath);
            Message.Print("unconfig");
        }

        public void UseCompile(string usePerlStr, bool newWindow = false) {
            List<StrawberryPerl> perlsInstalled = PerlOp.PerlsInstalled();
            List<StrawberryPerl> useWith = new List<StrawberryPerl>();

            string[] perls = usePerlStr.Split(new char[] {','});

            foreach (string perlName in perls) {
                bool perlAdded = false;

                foreach (StrawberryPerl perl in perlsInstalled) {
                    if (BitSuffixCheck(perlName).Equals(perl.Name)) {
                        useWith.Add(perl);
                        perlAdded = true;
                    }
                }

                if (! perlAdded) {
                    Console.Error.WriteLine(
                        "Can't launch Perl version {0}. It isn't installed.",
                        perlName
                    );
                }
            }

            if (! useWith.Any()) {
                Console.Error.WriteLine("\nThe selected Perl versions you specified are not installed.\n");
                Exit((int)ErrorCodes.PERL_NOT_INSTALLED);
            }

            string sysPath = PathOp.PathGet();
            string usrPath = PathOp.PathGetUsr();

            foreach (StrawberryPerl perl in useWith) {
                if (newWindow) {
                    UseInNewWindow(perl, sysPath, usrPath);
                }
                else {
                    UseInSameWindow(perl, sysPath, usrPath);
                }
            }
        }

        private void UseInNewWindow(StrawberryPerl perl, string sysPath, string usrPath) {
            try {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Normal};

                var newPath = perl.Paths;
                newPath.AddRange(Environment.ExpandEnvironmentVariables(sysPath).Split(new char[] {';'}).ToList());
                newPath.AddRange(Environment.ExpandEnvironmentVariables(usrPath).Split(new char[] {';'}).ToList());

                Environment.SetEnvironmentVariable("PATH", string.Join(";", newPath));
                Environment.SetEnvironmentVariable("BERRYBREW_TEMP_USE", "true");

                string prompt = Environment.GetEnvironmentVariable("PROMPT");
                Environment.SetEnvironmentVariable("PROMPT", "$Lberrybrew use perl-" + perl.Name + "$G" + "$_" + "$P$G");

                if (Options("shell", null, true) == "powershell") {
                    // Spawn with Powershell
                    string args = "-NoExit -Command \"& {$host.ui.RawUI.WindowTitle='berrybrew use perl-" + perl.Name + "'}; cd $home\"";
                    startInfo.Arguments = args;
                    startInfo.FileName = "powershell.exe";
                }
                else {
                    // Spawn with cmd
                    startInfo.Arguments = "/k TITLE berrybrew use perl-" + perl.Name;
                    startInfo.FileName = "cmd.exe";
                }

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
                // possible test syntax: (staging\berrybrew use 5.12,5.12.3_32) | perl -e "@pid = grep s/^^berrybrew use.*: spawned in new command-window, with PID=(\d+)\s*$/$1/, <STDIN>; sleep(2); print $_,$/ for @pid; sleep(2); kill 9, $_ for @pid"
            }
            catch(Exception objException) {
                Console.Error.WriteLine(objException);
                Exit((int)ErrorCodes.GENERIC_ERROR);
            }
        }

        private void UseInSameWindow(StrawberryPerl perl, string sysPath, string usrPath) {
            Console.WriteLine("perl-" + perl.Name + "\n==============");
            Environment.SetEnvironmentVariable("BERRYBREW_TEMP_USE", "true");

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

                if(null != Environment.GetEnvironmentVariable("BBTEST_SHOW_PID")) {
                    Console.WriteLine( "berrybrew use " + perl.Name + ": running in this command window, with PID=" + process.Id );
                }

                process.WaitForExit();

                Environment.SetEnvironmentVariable("PROMPT", prompt);   // reset before moving on
                Console.WriteLine("\nExiting <berrybrew use " + perl.Name + ">\n");
            }
            catch(Exception objException) {
                Console.Error.WriteLine(objException);
                Exit((int)ErrorCodes.GENERIC_ERROR);
            }
        }

        public string Version() {
            return @"1.40";
        }
    }
}
