using System;
using System.IO;
using System.Linq;
using BerryBrew;
using System.Collections.Generic;
            
namespace berrybrew {
    internal class Bbconsole {
        private static void Main(string[] args){

            Berrybrew bb = new Berrybrew();

            if (args.Length != 0 && args[0] == "debug"){
                bb.Debug = true;
                args = args.Skip(1).ToArray();
            }

            if (bb.Debug){
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
                        if(args[1] == "-h" || args[1] == "help")
                            bb.Message.Say("subcmd.associate");
                        else
                            bb.FileAssoc(args[1]);
                    }
                    bb.FileAssoc();
                    break;

                case "available":
                    if (args.Length > 1){
                        if (args[1].StartsWith("h"))
                            bb.Message.Say("subcmd.available");
                        else if (args[1] == "all") {
                            bb.Available(true);
                            Environment.Exit(0);
                        }
                    }
                                       
                    bb.Available();
                    break;

                case "currentperl":
                    Console.WriteLine(bb.PerlInUse().Name);
                    break;

                case "clean":
                    if (args.Length > 1){
                        if (args[1].StartsWith("h"))
                            bb.Message.Say("subcmd.clean");
                        else
                            bb.Clean(args[1]);
                    }
                    else
                        bb.Clean();

                    break;

                case "clone":
                    if (args.Length != 3)
                        bb.Message.Say("clone_command_usage");

                    bool ok = bb.Clone(args[1], args[2]);

                    if (! ok)
                        Environment.Exit(0);

                    break;

                case "config":
                    string cwd = Directory.GetCurrentDirectory();

                    if (String.Equals(cwd, @"c:\berrybrew", StringComparison.OrdinalIgnoreCase)){
                        Console.WriteLine("\nAt this time, berrybrew can not be installed in C:\\berrybrew. Please move the directory and try again\n");
                        Environment.Exit(0);
                    }

                    bb.Config();
                    break;

                case "exec":
                    if (args.Length == 1)
                        bb.Message.Say("exec_command_required");

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help")
                        bb.Message.Say("subcmd.exec");

                    List<String> newArgs = args.ToList();
                    newArgs.RemoveAt(0);
                    bb.ExecCompile(newArgs);
                    break;

                case "fetch":
                    bb.PerlUpdateAvailableList();
                    break;

                case "help":
                    if (args.Length == 1) {
                        bb.Message.Say("help");
                    } else {
                        switch (args[1].ToLower()) {
                            case "clean":
                                bb.Message.Say("subcmd.clean");
                                break;

                            case "exec":
                                bb.Message.Say("subcmd.exec");
                                break;

                            case "fetch":
                                bb.Message.Say("subcmd.fetch");
                                break;

                            case "use":
                                bb.Message.Say("subcmd.use");
                                break;

                            default:
                                bb.Message.Say("help");
                                break;
                        }
                    }
                    break;

                case "info":
                    if (args.Length == 1)
                        bb.Message.Say("info_option_required");

                    bb.Info(args[1]);
               
                    break;
                
                case "install":
                    if (args.Length == 1)
                        bb.Message.Say("install_ver_required");

                    try {
                        bb.Install(args[1]);
                    }

                    catch (ArgumentException error){
                        if (bb.Debug)
                            Console.WriteLine(error);

                        bb.Message.Say("install_ver_unknown");
                    }

                    break;

                case "license":
                    if (args.Length == 1)
                        bb.Message.Say("license");

                    break;

                case "list":
                    bb.List();
                    break;

                case "modules":
                    if (args.Length == 1)
                        bb.Message.Say("modules_command_required");

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help")
                        bb.Message.Say("subcmd.modules");

                    if (args[1] != "import" && args[1] != "export")
                    {
                        Console.WriteLine("\ninvalid option...\n");
                        bb.Message.Say("subcmd.modules");
                    }
                    
                    if (args[1] == "import")
                    {
                        if (args.Length < 3)
                        {
                            bb.ImportModules();
                        }
                        else
                        {
                            bb.ImportModules(args[2]);
                        }
                    }

                    if (args[1] == "export")
                    {
                        bb.ExportModules();
                    }
                    
                    break;
                
                case "off":
                    bb.Off();
                    break;

                case "options":
                    if (args.Length > 1) {
                        if (args[1].StartsWith("h"))
                            bb.Message.Say("subcmd.options");
                    }

                    if (args.Length == 1)
                        bb.Options();
                    if (args.Length == 2)
                        bb.Options(args[1]);
                     if (args.Length == 3)
                         bb.Options(args[1], args[2]);                   
                    break;
  
                case "options-update":
                    bb.OptionsUpdate();
                    break;               
               
                case "register":
                    if (args.Length == 1)
                        bb.Message.Say("register_ver_required");

                    bb.PerlRegisterCustomInstall(args[1]);
                    break;

                case "register-orphans":
                    bb.PerlUpdateAvailableListOrphans();
                    break;

                case "remove":
                    if (args.Length == 1)
                        bb.Message.Say("remove_ver_required");

                    bb.PerlRemove(args[1]);
                    break;

                case "switch":
                    if (args.Length == 1) 
                        bb.Message.Say("switch_ver_required");

                    if (args.Length == 2) {
                        if (args[1].StartsWith("h"))
                        {
                            bb.Message.Say("subcmd.switch");
                        }
                    }

                    bool switchQuick = false;
                    
                    if (args.Length == 3 && args[2] == "quick") {
                        switchQuick = true;
                    }
                   
                   bb.Switch(args[1], switchQuick);
                   break;

                case "unconfig":
                    bb.Unconfig();
                    break;

                case "upgrade":
                    bb.Upgrade();
                    break;
                    
                case "use": // pryrt's added feature
                    if (args.Length == 1) {
                        bb.Message.Say("use_ver_required");
                    }
                    switch(args[1].ToLower()) {
                        case "-h":
                        case "help":
                        case "-help":
                        case "--help":
                            bb.Message.Say("subcmd.use");
                            break;
                        case "--win":
                        case "--window":
                        case "--windowed":
                            if(args.Length<3)
                                bb.Message.Say("use_ver_required");
                            else
                                bb.UseCompile(args[2], true);
                            break;
                        default:
                            bb.UseCompile(args[1]);
                            break;
                    }
                    break;

                case "virtual":
                    if (args.Length == 1)
                        bb.Message.Say("virtual_command_required");

                    bb.PerlRegisterVirtualInstall(args[1]);
                    break;

                case "version":
                    Console.WriteLine(bb.Version());
                    break;

                default:
                    bb.Message.Say("help");
                    break;
            }
        }
    }
}
