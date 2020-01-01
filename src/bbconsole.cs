using System;
using System.IO;
using System.Linq;
using BerryBrew;
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
                    "install dir: {0}\nperl root dir: {1}\ntemp dir: {2}",
                    bb.installPath, bb.rootPath, bb.archivePath
                );
            }

            if (args.Length != 0 && args[0] == "test") {
                bb.Testing = true;
                Console.WriteLine("\nberrybrew testing enabled");
                args = args.Skip(1).ToArray();
            }

            if (args.Length == 0){
                bb.Message.Print("help");
                Environment.Exit(0);
            }
                
            switch (args[0]){

                case "associate":
                    if (args.Length > 1) {
                        if(args[1] == "-h" || args[1] == "help") {
                            bb.Message.Print("subcmd.associate");
                            Environment.Exit(0);
                        }
                        else {
                            bb.FileAssoc(args[1]);
                            Environment.Exit(0);
                        }
                    }
                    bb.FileAssoc();
                    Environment.Exit(0);
                    break;

                case "available":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.available");
                            Environment.Exit(0);
                        }
                        else if (args[1] == "all") {
                            bb.Available(true);
                            Environment.Exit(0);
                        }
                    }
                                       
                    bb.Available();
                    Environment.Exit(0);
                    break;

                case "currentperl":
                    Console.WriteLine(bb.PerlInUse().Name);
                    Environment.Exit(0);
                    break;

                case "clean":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.clean");
                            Environment.Exit(0);
                        }
                        else {
                            bb.Clean(args[1]);
                            Environment.Exit(0);
                        }
                    }		
                    else {
                        bb.Clean();
                        Environment.Exit(0);
                    }
                    break;

                case "clone":
                    if (args.Length != 3) {
                        bb.Message.Print("clone_command_usage");
                        Environment.Exit(0);
                    }

                    bb.Clone(args[1], args[2]);
                    Environment.Exit(0);
                    break;

                case "config":
                    string cwd = Directory.GetCurrentDirectory();

                    if (String.Equals(cwd, @"c:\berrybrew", StringComparison.OrdinalIgnoreCase)) {
                        Console.Error.WriteLine("\nAt this time, berrybrew can not be installed in C:\\berrybrew. Please move the directory and try again\n");
                        Environment.Exit(-1);
                    }

                    bb.Config();
                    Environment.Exit(0);
                    break;

                case "exec":
                    if (args.Length == 1) {
                        bb.Message.Print("exec_command_required");
                        Console.Error.WriteLine("'exec' requires a command");
                        Environment.Exit(-1);
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.exec");
                        Environment.Exit(0);
                    }

                    List<String> newArgs = args.ToList();
                    newArgs.RemoveAt(0);
                    bb.ExecCompile(newArgs);
                    Environment.Exit(0);
                    break;

                case "fetch":
                    bb.PerlUpdateAvailableList();
                    Environment.Exit(0);
                    break;

                case "help":
                    if (args.Length == 1) {
                        bb.Message.Print("help");
                        Environment.Exit(0);
                    } else {
                        switch (args[1].ToLower()) {
                            case "clean":
                                bb.Message.Print("subcmd.clean");
                                Environment.Exit(0);
                                break;

                            case "exec":
                                bb.Message.Print("subcmd.exec");
                                Environment.Exit(0);
                                break;

                            case "fetch":
                                bb.Message.Print("subcmd.fetch");
                                Environment.Exit(0);
                                break;

                            case "use":
                                bb.Message.Print("subcmd.use");
                                Environment.Exit(0);
                                break;

                            default:
                                bb.Message.Print("help");
                                Environment.Exit(0);
                                break;
                        }
                    }
                    break;

                case "info":
                    if (args.Length == 1) {
                        bb.Message.Print("info_option_required");
                        Environment.Exit(0);
                    }

                    bb.Info(args[1]);
                    Environment.Exit(0);
                    break;

                case "install":
                    if (args.Length == 1) {
                        bb.Message.Print("install_ver_required");
                        Environment.Exit(0);
                    }

                    try {
                        bb.Install(args[1]);
                        Environment.Exit(0);
                    }
                    catch (ArgumentException error){
                        if (bb.Debug) {
                            Console.WriteLine(error);
                        }

                        bb.Message.Print("install_ver_unknown");
                        Console.Error.WriteLine(error);
                        Environment.Exit(-1);
                    }
                    break;

                case "license":
                    if (args.Length == 1) {
                        bb.Message.Print("license");
                        Environment.Exit(0);
                    }
                    Environment.Exit(0);
                    break;

                case "list":
                    bb.List();
                    Environment.Exit(0);
                    break;

                case "modules":
                    if (args.Length == 1) {
                        bb.Message.Print("modules_command_required");
                        Environment.Exit(0);
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.modules");
                        Environment.Exit(0);
                    }
                    
                    if (args[1] != "import" && args[1] != "export") {
                        Console.WriteLine("\ninvalid option...\n");
                        bb.Message.Print("subcmd.modules");
                        Environment.Exit(0);
                    }
                    
                    if (args[1] == "import") {
                        if (args.Length < 3) {
                            bb.ImportModules();
                            Environment.Exit(0);
                        }
                        else {
                            bb.ImportModules(args[2]);
                            Environment.Exit(0);
                        }
                    }

                    if (args[1] == "export") {
                        bb.ExportModules();
                        Environment.Exit(0);
                    }
                    
                    Environment.Exit(0);
                    break;

                case "off":
                    bb.Off();
                    Environment.Exit(0);
                    break;

                case "options":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.options");
                            Environment.Exit(0);
                        }
                    }
                    if (args.Length == 1) {
                        bb.Options();
                        Environment.Exit(0);
                    }
                    if (args.Length == 2) {
                        bb.Options(args[1]);
                        Environment.Exit(0);
                    }
                    if (args.Length == 3) {
                        bb.Options(args[1], args[2]);                   
                        Environment.Exit(0);
                    }
                    Environment.Exit(0);
                    break;
  
                case "options-update":
                    bb.OptionsUpdate();
                    Environment.Exit(0);
                    break;

                case "options-update-force":
                    bb.OptionsUpdate(true);
                    Environment.Exit(0);
                    break;

                case "register":
                    if (args.Length == 1) {
                        bb.Message.Print("register_ver_required");
                        Environment.Exit(0);
                    }

                    bb.PerlRegisterCustomInstall(args[1]);
                    Environment.Exit(0);
                    break;

                case "register-orphans":
                    bb.PerlUpdateAvailableListOrphans();
                    Environment.Exit(0);
                    break;

                case "remove":
                    if (args.Length == 1) {
                        bb.Message.Print("remove_ver_required");
                        Environment.Exit(0);
                    }

                    bb.PerlRemove(args[1]);
                    Environment.Exit(0);
                    break;

                case "switch":
                    if (args.Length == 1) {
                        bb.Message.Print("switch_ver_required");
                        Environment.Exit(0);
                    }

                    if (args.Length == 2) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.switch");
                            Environment.Exit(0);
                        }
                    }

                    bool switchQuick = false;
                    
                    if (args.Length == 3 && args[2] == "quick") {
                        switchQuick = true;
                    }
                   
                    bb.Switch(args[1], switchQuick);
                    Environment.Exit(0);
                    break;

                case "unconfig":
                    bb.Unconfig();
                    Environment.Exit(0);
                    break;

                case "upgrade":
                    bb.Upgrade();
                    Environment.Exit(0);
                    break;
                    
                case "use":
                    if (args.Length == 1) {
                        bb.Message.Print("use_ver_required");
                        Environment.Exit(0);
                    }
                    switch(args[1].ToLower()) {
                        case "-h":
                        case "help":
                        case "-help":
                        case "--help":
                            bb.Message.Print("subcmd.use");
                            Environment.Exit(0);
                            break;
                        case "--win":
                        case "--window":
                        case "--windowed":
                            if(args.Length<3) {
                                bb.Message.Print("use_ver_required");
                                Environment.Exit(0);
                            }
                            else {
                                bb.UseCompile(args[2], true);
                                Environment.Exit(0);
                            }
                            break;

                        default:
                            bb.UseCompile(args[1]);
                            Environment.Exit(0);
                            break;
                    }
                    break;

                case "virtual":
                    if (args.Length == 1) {
                        bb.Message.Print("virtual_command_required");
                        Environment.Exit(0);
                    }
                    bb.PerlRegisterVirtualInstall(args[1]);
                    Environment.Exit(0);
                    break;

                case "version":
                    Console.WriteLine(bb.Version());
                    Environment.Exit(0);
                    break;

                default:
                    bb.Message.Print("help");
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
