using System;
using System.IO;
using System.Linq;
using BerryBrew;
using System.Collections.Generic;
            
namespace berrybrew {
    internal class Bbconsole {
        private static int Main(string[] args){

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
                return 0;
            }
                
            switch (args[0]){

                case "associate":
                    if (args.Length > 1) {
                        if(args[1] == "-h" || args[1] == "help") {
                            bb.Message.Print("subcmd.associate");
                            return 0;
                        }
                        else {
                            bb.FileAssoc(args[1]);
                            return 0;
                        }
                    }
                    bb.FileAssoc();
                    return 0;

                case "available":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.available");
                            return 0;
                        }
                        else if (args[1] == "all") {
                            bb.Available(true);
                            return 0;
                        }
                    }
                                       
                    bb.Available();
                    return 0;

                case "currentperl":
                    Console.WriteLine(bb.PerlInUse().Name);
                    return 0;

                case "clean":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.clean");
                            return 0;
                        }
                        else {
                            bb.Clean(args[1]);
                            return 0;
                        }
                    }		
                    else {
                        bb.Clean();
                        return 0;
                    }

                case "clone":
                    if (args.Length != 3) {
                        bb.Message.Print("clone_command_usage");
                        return 0;
                    }

                    bool ok = bb.Clone(args[1], args[2]);

                    if (! ok) {
                        return -1;
                    }

                    return 0;

                case "config":
                    string cwd = Directory.GetCurrentDirectory();

                    if (String.Equals(cwd, @"c:\berrybrew", StringComparison.OrdinalIgnoreCase)) {
                        Console.WriteLine("\nAt this time, berrybrew can not be installed in C:\\berrybrew. Please move the directory and try again\n");
                        return -1;
                    }

                    bb.Config();
                    return 0;

                case "exec":
                    if (args.Length == 1) {
                        bb.Message.Print("exec_command_required");
                        return -1;
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.exec");
                        return 0;
                    }

                    List<String> newArgs = args.ToList();
                    newArgs.RemoveAt(0);
                    bb.ExecCompile(newArgs);
                    return 0;

                case "fetch":
                    bb.PerlUpdateAvailableList();
                    return 0;

                case "help":
                    if (args.Length == 1) {
                        bb.Message.Print("help");
                        return 0;
                    } else {
                        switch (args[1].ToLower()) {
                            case "clean":
                                bb.Message.Print("subcmd.clean");
                                return 0;

                            case "exec":
                                bb.Message.Print("subcmd.exec");
                                return 0;

                            case "fetch":
                                bb.Message.Print("subcmd.fetch");
                                return 0;

                            case "use":
                                bb.Message.Print("subcmd.use");
                                return 0;

                            default:
                                bb.Message.Print("help");
                                return 0;
                        }
                    }

                case "info":
                    if (args.Length == 1) {
                        bb.Message.Print("info_option_required");
                        return 0;
                    }

                    bb.Info(args[1]);
                    return 0;

                case "install":
                    if (args.Length == 1) {
                        bb.Message.Print("install_ver_required");
                        return 0;
                    }

                    try {
                        bb.Install(args[1]);
                        return 0;
                    }
                    catch (ArgumentException error){
                        if (bb.Debug) {
                            Console.WriteLine(error);
                        }

                        bb.Message.Print("install_ver_unknown");
                        return -1;
                    }

                case "license":
                    if (args.Length == 1) {
                        bb.Message.Print("license");
                        return 0;
                    }
                    return 0;

                case "list":
                    bb.List();
                    return 0;

                case "modules":
                    if (args.Length == 1) {
                        bb.Message.Print("modules_command_required");
                        return 0;
                    }

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help") {
                        bb.Message.Print("subcmd.modules");
                        return 0;
                    }
                    
                    if (args[1] != "import" && args[1] != "export") {
                        Console.WriteLine("\ninvalid option...\n");
                        bb.Message.Print("subcmd.modules");
                        return 0;
                    }
                    
                    if (args[1] == "import") {
                        if (args.Length < 3) {
                            bb.ImportModules();
                            return 0;
                        }
                        else {
                            bb.ImportModules(args[2]);
                            return 0;
                        }
                    }

                    if (args[1] == "export") {
                        bb.ExportModules();
                        return 0;
                    }
                    
                    return 0;                

                case "off":
                    bb.Off();
                    return 0;

                case "options":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.options");
                            return 0;
                        }
                    }
                    if (args.Length == 1) {
                        bb.Options();
                        return 0;
                    }
                    if (args.Length == 2) {
                        bb.Options(args[1]);
                        return 0;
                    }
                    if (args.Length == 3) {
                        bb.Options(args[1], args[2]);                   
                        return 0;	
                    }
                    return 0;
  
                case "options-update":
                    bb.OptionsUpdate();
                    return 0;

                case "options-update-force":
                    bb.OptionsUpdate(true);
                    return 0;

                case "register":
                    if (args.Length == 1) {
                        bb.Message.Print("register_ver_required");
                        return 0;
                    }

                    bb.PerlRegisterCustomInstall(args[1]);
                    return 0;

                case "register-orphans":
                    bb.PerlUpdateAvailableListOrphans();
                    return 0;

                case "remove":
                    if (args.Length == 1) {
                        bb.Message.Print("remove_ver_required");
                        return 0;
                    }

                    bb.PerlRemove(args[1]);
                    return 0;

                case "switch":
                    if (args.Length == 1) {
                        bb.Message.Print("switch_ver_required");
                        return 0;
                    }

                    if (args.Length == 2) {
                        if (args[1].StartsWith("h")) {
                            bb.Message.Print("subcmd.switch");
                            return 0;
                        }
                    }

                    bool switchQuick = false;
                    
                    if (args.Length == 3 && args[2] == "quick") {
                        switchQuick = true;
                    }
                   
                    bb.Switch(args[1], switchQuick);
                    return 0;

                case "unconfig":
                    bb.Unconfig();
                    return 0;

                case "upgrade":
                    bb.Upgrade();
                    return 0;
                    
                case "use":
                    if (args.Length == 1) {
                        bb.Message.Print("use_ver_required");
                        return 0;
                    }
                    switch(args[1].ToLower()) {
                        case "-h":
                        case "help":
                        case "-help":
                        case "--help":
                            bb.Message.Print("subcmd.use");
                            return 0;
                        case "--win":
                        case "--window":
                        case "--windowed":
                            if(args.Length<3) {
                                bb.Message.Print("use_ver_required");
                                return 0;
                            }
                            else {
                                bb.UseCompile(args[2], true);
                                return 0;
                            }

                        default:
                            bb.UseCompile(args[1]);
                            return 0;
                    }

                case "virtual":
                    if (args.Length == 1) {
                        bb.Message.Print("virtual_command_required");
                        return 0;
                    }
                    bb.PerlRegisterVirtualInstall(args[1]);
                    return 0;

                case "version":
                    Console.WriteLine(bb.Version());
                    return 0;

                default:
                    bb.Message.Print("help");
                    return 0;
            }
        }
    }
}
