using BerryBrew.PerlInstance;
using BerryBrew.PathOperations;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace BerryBrew.PerlOperations {
    public class PerlOp {
        private Berrybrew bb = null;
		public PathOp PathOp = null;

        public PerlOp(Berrybrew berrybrew) {
            bb = berrybrew;
			PathOp = new PathOp(bb);
        }

        internal static string PerlArchivePath(StrawberryPerl perl) {
            string path;

            try {
                if (! Directory.Exists(perl.archivePath)) {
                    Directory.CreateDirectory(perl.archivePath);
                }
                return perl.archivePath + @"\" + perl.File;
            }

            catch (UnauthorizedAccessException) {
                Console.Error.WriteLine("Error, do not have permissions to create directory: " + perl.archivePath);
            }

            Console.WriteLine("Creating temporary directory instead");

            do {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(path));

            Directory.CreateDirectory(path);

            return path + @"\" + perl.File;
        }

        internal List<StrawberryPerl> PerlGenerateObjects() {
            List<StrawberryPerl> perlObjects = new List<StrawberryPerl>();

            var perls           = bb.JsonParse("perls");
            var customPerls     = bb.JsonParse("perls_custom");
            var virtualPerls    = bb.JsonParse("perls_virtual");

            foreach (var perl in perls) {
                perlObjects.Add(
                    new StrawberryPerl(
                        bb,
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

            foreach (var perl in customPerls) {
                perlObjects.Add(
                    new StrawberryPerl(
                        bb,
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
                        bb,
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

            return perlObjects;
        }

        public StrawberryPerl PerlInUse() {
            string path = PathOp.PathGet();

            StrawberryPerl currentPerl = new StrawberryPerl();

            if (path != null) {
                string[] paths = path.Split(new char[] {';'});
                foreach (StrawberryPerl perl in bb._perls.Values) {
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

        internal static bool PerlIsInstalled(StrawberryPerl perl) {
            return Directory.Exists(perl.installPath)
                   && File.Exists(perl.PerlPath + @"\perl.exe");
        }

        public List<StrawberryPerl> PerlsInstalled() {
            PerlGenerateObjects();
            return bb._perls.Values.Cast<StrawberryPerl>().Where(PerlIsInstalled).ToList();
        }

        internal List<string> PerlOrphansFind() {
            List<StrawberryPerl> perls = PerlsInstalled();

            try {
                Directory.GetDirectories(bb.instancePath);
            }
            catch (Exception err) {
                if (bb.Debug) {
                    Console.Error.WriteLine("DEBUG: failure getting directories of root");
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }

                bb.Exit((int) Berrybrew.ErrorCodes.DIRECTORY_LIST_FAILED);
            }

            List<string> dirs = new List<string>(Directory.GetDirectories(bb.instancePath));
            List<string> perlInstallations = new List<string>();

            foreach (StrawberryPerl perl in perls) {
                perlInstallations.Add(perl.installPath);
            }

            List<string> orphans = new List<string>();
            Dictionary<string, bool> orphansIgnored = PerlOrphansIgnore();
            
            foreach (string dir in dirs) {
                if (dir == bb.archivePath) {
                    continue;
                }

                // Skip valid known extracurrirular directories
                
				// Valid perl instance directory
				if (perlInstallations.Contains(dir)) {
                    continue;
                }

                // Ignored orphans
                Match ignoredOrphanFound = Regex.Match(dir, @".*\\(.*)$");

                if (ignoredOrphanFound.Success) {
                    string ignoredOrphan = ignoredOrphanFound.Groups[1].Captures[0].Value;
                    
                    if (orphansIgnored.ContainsKey(ignoredOrphan)) { 
                        continue;
                    }
                }

                string dirBaseName = dir.Remove(0, bb.instancePath.Length);
                orphans.Add(dirBaseName);
            }

            return orphans;
        }

        public Dictionary<string, bool> PerlOrphansIgnore() {
            Dictionary<string, bool> ignoreList = new Dictionary<string, bool>();

            List<string> ignoreDirs = new List<string> {
            };

            // Since we've separated the installPath for staging and testing,
            // and moved perl instances to an 'instance' sub dir, no ignore
            // items are currently needed
            
            foreach (string dir in ignoreDirs) {
                ignoreList.Add(dir, true);
            }

            if (bb.Testing) {
                ignoreList.Add("unit_test", true);
            }
            
            return ignoreList;
        }

        public void PerlRegisterCustomInstall(string perlName, StrawberryPerl perlBase=new StrawberryPerl()) {
            perlName = bb.BitSuffixCheck(perlName);

            if (! Directory.Exists(bb.instancePath + perlName)) {
                Console.Error.WriteLine("installation directory '" + perlName + "' does not exist");
                bb.Exit((int) Berrybrew.ErrorCodes.DIRECTORY_NOT_EXIST);
            }

            if (! File.Exists(bb.instancePath + perlName + @"\perl\bin\perl.exe")) {
                Console.Error.WriteLine("{0} is not a valid Perl installation", perlName);
                bb.Exit((int) Berrybrew.ErrorCodes.PERL_INVALID_ERROR);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            data["name"] 	= perlName;
            data["custom"] 	= perlBase.Custom;
            data["file"] 	= perlBase.File ?? "";
            data["url"] 	= perlBase.Url ?? "";
            data["ver"] 	= perlBase.Version ?? "0.0.0";
            data["csum"] 	= perlBase.Sha1Checksum ?? "";

            List<Dictionary<string, object>> perlList = new List<Dictionary<string, object>> {data};

            bb.JsonWrite("perls_custom", perlList);

            Console.WriteLine("Successfully registered {0}", perlName);

            bb.bypassOrphanCheck = true;
        }

        public void PerlRegisterVirtualInstall(string perlName) {
            if (! Berrybrew.CheckName(perlName)) {
                bb.Exit((int) Berrybrew.ErrorCodes.PERL_NAME_INVALID);
            }

            Console.Write("\nSpecify the path to the perl binary: ");
            string perlPath = Console.ReadLine();

            Console.Write("\nSpecify the library path: ");
            string libPath = Console.ReadLine();

            Console.Write("\nSpecify an additional path: ");
            string auxPath = Console.ReadLine();

            Console.Write("\n");

            bool perlPathValid = false;

            if (File.Exists(String.Format("{0}/perl.exe", perlPath))) {
                perlPathValid = true;
            }

            if (! perlPathValid) {
                Console.Error.WriteLine(
                    "ERROR: {0} does not have a perl.exe binary. Can't register '{1}' perl instance'\n",
                    perlPath,
                    perlName
                );
                bb.Exit((int) Berrybrew.ErrorCodes.PERL_INVALID_ERROR);
            }
            if (! string.IsNullOrEmpty(libPath) && ! Directory.Exists(libPath)) {
                Console.Error.WriteLine("\n'{0}' library directory doesn't exist. Can't continue...\n", libPath);
                bb.Exit((int) Berrybrew.ErrorCodes.DIRECTORY_NOT_EXIST);
            }
            if (! string.IsNullOrEmpty(auxPath) && ! Directory.Exists(auxPath)) {
                Console.Error.WriteLine("\n'{0}' auxillary directory doesn't exist. Can't continue...\n", auxPath);
                bb.Exit((int) Berrybrew.ErrorCodes.DIRECTORY_NOT_EXIST);
            }

            string instanceName = bb.instancePath + perlName;

            if (! Directory.Exists(instanceName)) {
                Directory.CreateDirectory(bb.instancePath + perlName);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            data["name"] 		= perlName;
            data["custom"] 		= false;
            data["virtual"] 	= true;
            data["file"] 		= "";
            data["url"] 		= "";
            data["ver"] 		= "0.0.0";
            data["csum"] 		= "";
            data["perl_path"]	= perlPath;
            data["lib_path"] 	= libPath;
            data["aux_path"] 	= auxPath;

            List<Dictionary<string, object>> virtualPerlList = new List<Dictionary<string, object>> {data};

            bb.JsonWrite("perls_virtual", virtualPerlList);

            Console.WriteLine("\nSuccessfully registered virtual perl {0}", perlName);

            bb.bypassOrphanCheck = true;
        }

        public void PerlRemove(string perlVersionToRemove) {
            try {
                perlVersionToRemove = bb.BitSuffixCheck(perlVersionToRemove);
                StrawberryPerl perl = PerlResolveVersion(perlVersionToRemove);
                StrawberryPerl currentPerl = PerlInUse();

                if (perl.Name == currentPerl.Name) {
                    Console.WriteLine("Removing Perl " + perlVersionToRemove +
                                      " from PATH");
                    PathOp.PathRemovePerl(bb._perls);
                }

                if (Directory.Exists(perl.installPath)) {
                    try {
                        Console.WriteLine("Removing Strawberry Perl " +
                                          perlVersionToRemove);
                        Berrybrew.FilesystemResetAttributes(perl.installPath);
                        Directory.Delete(perl.installPath, true);
                        Console.WriteLine(
                            "Successfully removed Strawberry Perl " +
                            perlVersionToRemove);
                    }
                    catch (IOException err) {
                        Console.Error.WriteLine(
                            "Unable to completely remove Strawberry Perl " +
                            perlVersionToRemove + " some files may remain");

                        if (bb.Debug) {
                            Console.Error.WriteLine("DEBUG: {0}", err);
                        }

                        bb.Exit((int) Berrybrew.ErrorCodes.PERL_REMOVE_FAILED);
                    }
                }
                else {
                    Console.Error.WriteLine("Strawberry Perl " +
                                            perlVersionToRemove +
                                            " not found (are you sure it's installed?)");
                    bb.Exit((int) Berrybrew.ErrorCodes.PERL_REMOVE_FAILED);
                }

                if (perl.Custom) {
                    dynamic customPerlList = bb.JsonParse("perls_custom", true);
                    customPerlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(customPerlList);

                    List<Dictionary<string, object>> updatedPerls =
                        new List<Dictionary<string, object>>();

                    foreach (Dictionary<string, object> perlStruct in customPerlList) {
                        if (! perlVersionToRemove.Equals(perlStruct["name"].ToString())) {
                            updatedPerls.Add(perlStruct);
                        }
                    }

                    bb.JsonWrite("perls_custom", updatedPerls, true);
                }

                if (perl.Virtual) {
                    dynamic virtualPerlList = bb.JsonParse("perls_virtual", true);
                    virtualPerlList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(virtualPerlList);

                    List<Dictionary<string, object>> updatedPerls =
                        new List<Dictionary<string, object>>();

                    foreach (Dictionary<string, object> perlStruct in virtualPerlList) {
                        if (! perlVersionToRemove.Equals(perlStruct["name"].ToString())) {
                            updatedPerls.Add(perlStruct);
                        }
                    }

                    bb.JsonWrite("perls_virtual", updatedPerls, true);
                }
            }
            catch (ArgumentException err) {
                if (bb.Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }

                bb.Message.Error("perl_unknown_version");
                bb.Exit((int) Berrybrew.ErrorCodes.PERL_UNKNOWN_VERSION);
            }
            catch (UnauthorizedAccessException err) {
                if (bb.Debug) {
                    Console.Error.WriteLine("DEBUG: {0}", err);
                }

                Console.Error.WriteLine("Unable to remove Strawberry Perl " +
                                        perlVersionToRemove +
                                        " permission was denied by System");
                bb.Exit((int) Berrybrew.ErrorCodes.PERL_REMOVE_FAILED);
            }
            catch (System.NullReferenceException err) {
                 if (bb.Debug) {
                     Console.Error.WriteLine("DEBUG: {0}", err);
                 }
 
                 Console.Error.WriteLine("Unable to remove Strawberry Perl " +
                                         perlVersionToRemove +
                                         " there was a problem writing to a custom/virtual JSON file");
                 bb.Exit((int) Berrybrew.ErrorCodes.JSON_WRITE_FAILED);               
            }
        }

        internal StrawberryPerl PerlResolveVersion(string version) {
            version = bb.BitSuffixCheck(version);

            foreach (StrawberryPerl perl in bb._perls.Values) {
                if (perl.Name == version) {
                    return perl;
                }
            }

            throw new ArgumentException("Unknown version: " + version);
        }

        public void PerlUpdateAvailableList() {
            Console.WriteLine("Attempting to fetch the updated Perls list...");

            using (WebClient client = new WebClient()) {
                string jsonData = null;

                try {
                    jsonData = client.DownloadString(bb.downloadURL);
                }
                catch (WebException err){
                    Console.Error.Write("\nCan't open file {0}. Can not continue...\n", bb.downloadURL);
                    if (bb.Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", err);
                    }
                    bb.Exit((int) Berrybrew.ErrorCodes.FILE_OPEN_FAILED);
                }

                dynamic json = null;

                try {
                    json = JsonConvert.DeserializeObject(jsonData);
                }
                catch (JsonReaderException err) {
                    Console.Error.Write("\nCan't read the JSON data. It may be invalid\n");
                    if (bb.Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", err);
                    }
                    bb.Exit((int) Berrybrew.ErrorCodes.JSON_INVALID_ERROR);
                }

                List<String> perls = new List<String>();

                // output data
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                foreach (var release in json){
                    string nameString = release.name;

                    if (Regex.IsMatch(nameString, @"(with USE_64_BIT_INT|with USE_LONG_DOUBLE)")) {
                        continue;
                    }

                    Match versionString = Regex.Match(nameString, @"(\d{1}\.\d{1,2}\.\d{1,2})");

                    if (versionString.Success) {
                        Match bitString = Regex.Match(nameString, @"(\d{2})bit");

                        if (bitString.Success) {
                            string version = versionString.Groups[1].Value;
                            string bits = bitString.Groups[1].Value;
                            string bbVersion = version + "_" + bits;

                            string[] majorVersionParts = version.Split(new char[] {'.'});
                            string majorVersion = majorVersionParts[0] + "." + majorVersionParts[1];
                            string bbMajorVersion = majorVersion + "_" + bits;

                            Dictionary<string, object> perlInstance = new Dictionary<string, object>();

                            if (release.edition.portable != null) {
                                perlInstance.Add("name", bbVersion);
                                perlInstance.Add("url", release.edition.portable.url);
                                string file = release.edition.portable.url;
                                file = file.Split(new char[] {'/'}).Last();
                                perlInstance.Add("file", file);
                                perlInstance.Add("csum", release.edition.portable.sha1);
                                perlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (! perls.Contains(bbMajorVersion)) {
                                     perlInstance.Add("newest", true);
                                }
                                else {
                                    perlInstance.Add("newest", false);
                                }

                                if (bb.Debug) {
                                    Console.WriteLine(
                                        "DEBUG: {0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }
                            }
                            else if (release.edition.zip != null) {
                                perlInstance.Add("name", bbVersion);
                                perlInstance.Add("url", release.edition.zip.url);
                                string file = release.edition.zip.url;
                                file = file.Split(new char[] {'/'}).Last();
                                perlInstance.Add("file", file);
                                perlInstance.Add("csum", release.edition.zip.sha1);
                                perlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (! perls.Contains(bbMajorVersion)) {
                                     perlInstance.Add("newest", true);
                                }
                                else {
                                    perlInstance.Add("newest", false);
                                }

                                if (bb.Debug) {
                                    Console.WriteLine(
                                        "DEBUG: {0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }
                            }

                            data.Add(perlInstance);

                            Dictionary<string, object> pdlInstance = new Dictionary<string, object>();

                            if (release.edition.pdl != null) {
                                string pdlVersion = bbVersion + "_" + "PDL";
                                pdlInstance.Add("name", pdlVersion);
                                pdlInstance.Add("url", release.edition.pdl.url);
                                string file = release.edition.pdl.url;
                                file = file.Split(new char[] {'/'}).Last();
                                pdlInstance.Add("file", file);
                                pdlInstance.Add("csum", release.edition.pdl.sha1);
                                pdlInstance.Add("ver", bbVersion.Split(new char[] {'_'}).First());

                                if (bb.Debug) {
                                    Console.WriteLine(
                                        "DEBUG: {0}:\n\t{1}\n\t{2}\n\t{3}\n\n",
                                        perlInstance["name"],
                                        perlInstance["file"],
                                        perlInstance["url"],
                                        perlInstance["csum"]
                                    );
                                }

                                if (! perls.Contains(bbMajorVersion)) {
                                     pdlInstance.Add("newest", true);
                                }
                                else {
                                    pdlInstance.Add("newest", false);
                                }

                                data.Add(pdlInstance);
                            }

                            perls.Add(bbMajorVersion);
                        }
                    }
                } // end build data

                try {
                    bb.JsonWrite("perls", data, true);
                }
                catch (System.UnauthorizedAccessException err){
                    Console.Error.WriteLine("\nYou need to be running with elevated prvileges to run this command\n");

                    if (bb.Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", err);
                    }

                    bb.Exit((int) Berrybrew.ErrorCodes.JSON_WRITE_FAILED);
                }

                Console.WriteLine("Successfully updated the available Perls list...");
            }

            PerlUpdateAvailableListOrphans();
        }

        public void PerlUpdateAvailableListOrphans() {
            List<string> orphans = PerlOrphansFind();

            foreach(var orphan in orphans) {
                Console.WriteLine("Registering legacy Perl '{0}' as custom...", orphan);
                PerlRegisterCustomInstall(orphan);
            }
        }
    }
}
