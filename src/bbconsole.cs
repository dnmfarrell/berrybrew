using BerryBrew;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace berrybrew {
    internal class Bbconsole {
        private static void Main(string[] args){

            Berrybrew bb = new Berrybrew();

            if (args.Length != 0 && args[0] == "debug") {
                bb.Debug = true;
                args = args.Skip(1).ToArray();
            }

            if (bb.Debug) {
                Console.WriteLine("\nberrybrew debugging enabled...\n");
                Console.WriteLine(
                    "install dir: {0}\nperl instance dir: {1}\ntemp dir: {2}\n",
                    bb.installPath, bb.instancePath, bb.archivePath
                );
            }

            if (args.Length != 0 && args[0] == "test") {
                bb.Testing = true;
                Console.WriteLine("\nberrybrew testing enabled");
                args = args.Skip(1).ToArray();
            }

            if (args.Length != 0 && args[0] == "trace") {
                bb.Trace = true;
                args = args.Skip(1).ToArray();
            }

            if (args.Length != 0 && args[0] == "status") {
                bb.Status = true;
                args = args.Skip(1).ToArray();
            }

            if (args.Length == 0){
                bb.Message.Print("help");
                bb.Exit(0);
            }

            switch (args[0]){
                case "archives":
                    List<string> archiveFileNames = bb.ArchiveList();

                    if (archiveFileNames.Count > 0) {
                        Console.WriteLine("Downloaded Perl instance archive files:\n");

                        foreach (string zipName in archiveFileNames) {
                            Console.WriteLine("\t{0}", zipName);
                        }
                    }
                    else {
                        Console.WriteLine("No Perl instance archive files have been downloaded");
                    }

                    bb.Exit(0);
                    break;
                
                case "assoc":
                    if (args.Length > 1) {
                        if(args[1] == "-h" || args[1] == "help") {
                            bb.Message.Print("subcmd.associate");
                            bb.Exit(0);
                        }
                        else {
                            bb.FileAssoc(args[1]);
                            bb.Exit(0);
                        }
                    }
                    bb.FileAssoc();
                    bb.Exit(0);
                    break;

                case "associate":
                    if (args.Length > 1) {
                        if(args[1] == "-h" || args[1] == "help") {
                            bb.Message.Print("subcmd.associate");
                            bb.Exit(0);
                        }
                        else {
                            bb.FileAssoc(args[1]);
                            bb.Exit(0);
                        }
                    }
                    bb.FileAssoc();
                    bb.Exit(0);
                    break;

                case "available":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.available");
                            bb.Exit(0);
                        }
                        else if (args[1] == "all") {
                            bb.Available(true);
                            bb.Exit(0);
                        }
                    }

                    bb.Available();
                    bb.Exit(0);
                    break;

                case "currentperl":
                    Console.WriteLine(bb.PerlOp.PerlInUse().Name);
                    bb.Exit(0);
                    break;

                case "clean":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.clean");
                        }
                        else {
                            bb.Clean(args[1]);
                        }
                    }
                    else {
                        bb.Clean();
                        bb.Exit(0);
                    }
                    break;

                case "clone":
                    if (args.Length != 3) {
                        bb.Message.Print("clone_command_usage");
                        bb.Exit(0);
                    }

                    bb.Clone(args[1], args[2]);
                    bb.Exit(0);
                    break;

                case "config":
                    string cwd = Directory.GetCurrentDirectory();

                    if (String.Equals(cwd, @"c:\berrybrew", StringComparison.OrdinalIgnoreCase)) {
                        Console.Error.WriteLine("\nAt this time, berrybrew can not be installed in C:\\berrybrew. Please move the directory and try again\n");
                        bb.Exit(-1);
                    }

                    bb.Config();
                    bb.Exit(0);
                    break;

                case "download":
                    if (args.Length == 1) {
                        Console.Error.WriteLine("'download' requires a version or the 'all' argument\n");
                        bb.Exit(-1);
                    }

                    bb.Download(args[1]);
                    bb.Exit(0);
                    break;

                case "exit":
                    if (args.Length == 1) {
                        Console.Error.WriteLine("'exit' requires an error code integer\n");
                        bb.Exit(-1);
                    }
                    bb.Exit(Int32.Parse(args[1]));
                    break;

                case "error":
                    if (args.Length == 1) {
                        bb.Message.Error("error_number_required");
                        bb.Exit(-1);
                    }

                    string errorName = Enum.GetName(typeof(Berrybrew.ErrorCodes), Int32.Parse(args[1]));
                    if (errorName == null) {
                        errorName = "EXTERNAL_PROCESS_ERROR";
                    }
                    Console.WriteLine("\nError Code {0}: {1}\n", args[1], errorName);
                    bb.Exit(0);
                    break;

                case "error-codes":
                    bb.bypassOrphanCheck = true;
                    foreach (int code in Enum.GetValues(typeof(Berrybrew.ErrorCodes))) {
                        string exitCodeName = Enum.GetName(typeof(Berrybrew.ErrorCodes), code);
                        Console.WriteLine("{0} - {1}", code, exitCodeName);
                    }
                    break;

                case "exec":
                    if (args.Length == 1) {
                        bb.Message.Print("exec_command_required");
                        Console.Error.WriteLine("'exec' requires a command");
                        bb.Exit(-1);
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.exec");
                        bb.Exit(0);
                    }

                    List<String> newArgs = args.ToList();
                    newArgs.RemoveAt(0);
                    bb.ExecCompile(newArgs);
                    bb.Exit(0);
                    break;

                case "fetch":
                    bb.PerlOp.PerlUpdateAvailableList();
                    bb.Exit(0);
                    break;

                case "help":
                    if (args.Length == 1) {
                        bb.Message.Print("help");
                        bb.Exit(0);
                    } else {
                        switch (args[1].ToLower()) {
                            case "clean":
                                bb.Message.Print("subcmd.clean");
                                bb.Exit(0);
                                break;

                            case "exec":
                                bb.Message.Print("subcmd.exec");
                                bb.Exit(0);
                                break;

                            case "fetch":
                                bb.Message.Print("subcmd.fetch");
                                bb.Exit(0);
                                break;

                            case "use":
                                bb.Message.Print("subcmd.use");
                                bb.Exit(0);
                                break;

                            default:
                                bb.Message.Print("help");
                                bb.Exit(0);
                                break;
                        }
                    }
                    break;

                case "hidden":
                    bb.Message.Say("hidden");
                    bb.Exit(0);
                    break;

                case "info":
                    if (args.Length == 1) {
                        bb.Message.Print("info_option_required");
                        bb.Exit(0);
                    }

                    bb.Info(args[1]);
                    bb.Exit(0);
                    break;

                case "install":
                    if (args.Length == 1) {
                        bb.Message.Print("install_ver_required");
                        bb.Exit(0);
                    }

                    try {
                        bb.Install(args[1]);
                        bb.Exit(0);
                    }
                    catch (ArgumentException error){
                        if (bb.Debug) {
                            Console.WriteLine(error);
                        }

                        bb.Message.Print("install_ver_unknown");
                        Console.Error.WriteLine(error);
                        bb.Exit(-1);
                    }
                    break;

                case "license":
                    if (args.Length == 1) {
                        bb.Message.Print("license");
                        bb.Exit(0);
                    }
                    bb.Exit(0);
                    break;

                case "list":
                    bb.List();
                    bb.Exit(0);
                    break;

                case "modules":
                    if (args.Length == 1) {
                        bb.Message.Print("modules_command_required");
                        bb.Exit(0);
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.modules");
                        bb.Exit(0);
                    }

                    if (args[1] != "import" && args[1] != "export") {
                        Console.Error.WriteLine("\ninvalid option...\n");
                        bb.Message.Print("subcmd.modules");
                        bb.Exit(-1);
                    }

                    if (args[1] == "import") {
                        if (args.Length < 3) {
                            bb.ImportModules();
                            bb.Exit(0);
                        }
                        else {
                            bb.ImportModules(args[2]);
                            bb.Exit(0);
                        }
                    }

                    if (args[1] == "export") {
                        bb.ExportModules();
                        bb.Exit(0);
                    }

                    bb.Exit(0);
                    break;

                case "off":
                    bb.Off();
                    bb.Exit(0);
                    break;

                case "options":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.options");
                            bb.Exit(0);
                        }
                    }
                    if (args.Length == 1) {
                        bb.Options();
                        bb.Exit(0);
                    }
                    if (args.Length == 2) {
                        bb.Options(args[1]);
                        bb.Exit(0);
                    }
                    if (args.Length == 3) {
                        bb.Options(args[1], args[2]);
                        bb.Exit(0);
                    }
                    bb.Exit(0);
                    break;

                case "options-update":
                    bb.OptionsUpdate();
                    bb.Exit(0);
                    break;

                case "options-update-force":
                    bb.OptionsUpdate(true);
                    bb.Exit(0);
                    break;

                case "orphans":
                    bb.bypassOrphanCheck = false;
                    bb.OrphanedPerls();
                    bb.bypassOrphanCheck = true;
                    bb.Exit(0);
                    break;

                case "orphans-ignored":
                    Dictionary<string, bool> ignoredOrphans = bb.PerlOp.PerlOrphansIgnore();

                    Console.WriteLine("The following perl directories are ignored when listing orphans:\n");
                    
                    foreach (string ignored in ignoredOrphans.Keys) {
                        Console.WriteLine("\t{0}", ignored);
                    }

                    bb.Exit(0);
                    break;

                case "register":
                    if (args.Length == 1) {
                        bb.Message.Print("register_ver_required");
                        bb.Exit(0);
                    }

                    bb.PerlOp.PerlRegisterCustomInstall(args[1]);
                    bb.Exit(0);
                    break;

                case "register-orphans":
                    bb.PerlOp.PerlUpdateAvailableListOrphans();
                    bb.Exit(0);
                    break;

                case "remove":
                    if (args.Length == 1) {
                        bb.Message.Print("remove_ver_required");
                        bb.Exit(0);
                    }

                    bb.PerlOp.PerlRemove(args[1]);
                    bb.Exit(0);
                    break;

                case "snapshot":
                    if (args.Length < 2) {
                        bb.Message.Print("snapshot_arguments_required");
                        bb.Message.Print("subcmd.snapshot");
                        bb.Exit(0);
                    }

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.snapshot");
                        bb.Exit(0);
                    }

                    if (args[1] == "list") {
                        bb.SnapshotList();
                        bb.Exit(0);
                    }

                    if (args.Length < 3) {
                        bb.Message.Print("snapshot_arguments_required");
                        bb.Message.Print("subcmd.snapshot");
                        bb.Exit(0);
                    }

                    if (args[1] != "export" && args[1] != "import") {
                         bb.Message.Print("snapshot_arguments_required");
                         bb.Message.Print("subcmd.snapshot");
                         bb.Exit(0);                        
                    }
                    
                    if (args[1] == "export") {
                        if (args.Length == 3) {
                            // instance
                            bb.SnapshotCompress(args[2]);
                            bb.Exit(0);
                        }
                        if (args.Length == 4) {
                            // instance + zipfile
                            bb.SnapshotCompress(args[2], args[3]);
                            bb.Exit(0);
                        }                                          
                    }
                    if (args[1] == "import") {
                        if (args.Length == 3) {
                            // snapshot_name
                            bb.SnapshotExtract(args[2]);
                            bb.Exit(0);
                        }
                        if (args.Length == 4) {
                            // snapshot_name + new_instance_name
                            bb.SnapshotExtract(args[2], args[3]);
                            bb.Exit(0);
                        }                                          
                    }                   
                    break;                   
 
                case "switch":
                    if (args.Length == 1) {
                        bb.Message.Print("switch_ver_required");
                        bb.Exit(0);
                    }

                    if (args.Length == 2) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.switch");
                            bb.Exit(0);
                        }
                    }

                    bool switchQuick = false;

                    if (args.Length == 3 && args[2] == "quick") {
                        switchQuick = true;
                    }

                    bb.Switch(args[1], switchQuick);
                    bb.Exit(0);
                    break;

                case "unconfig":
                    bb.Unconfig();
                    bb.Exit(0);
                    break;

                case "use":
                    if (args.Length == 1) {
                        bb.Message.Print("use_ver_required");
                        bb.Exit(0);
                    }
                    switch(args[1].ToLower()) {
                        case "-h":
                        case "help":
                        case "-help":
                        case "--help":
                            bb.Message.Print("subcmd.use");
                            bb.Exit(0);
                            break;
                        case "--win":
                        case "--window":
                        case "--windowed":
                            if(args.Length<3) {
                                bb.Message.Print("use_ver_required");
                                bb.Exit(0);
                            }
                            else {
                                bb.UseCompile(args[2], true);
                                bb.Exit(0);
                            }
                            break;

                        default:
                            bb.UseCompile(args[1]);
                            bb.Exit(0);
                            break;
                    }
                    break;

                case "virtual":
                    if (args.Length == 1) {
                        bb.Message.Print("virtual_command_required");
                        bb.Exit(0);
                    }
                    bb.PerlOp.PerlRegisterVirtualInstall(args[1]);
                    bb.Exit(0);
                    break;

                case "version":
                    Console.WriteLine(bb.Version());
                    bb.Exit(0);
                    break;

                default:
                    bb.Message.Print("help");
                    bb.Exit(0);
                    break;
            }
        }
    }
}
