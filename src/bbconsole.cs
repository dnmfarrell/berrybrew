using System;
using System.IO;
using System.Linq;
using BerryBrew;

namespace BBConsole {

    class bbconsole {

        static void Main(string[] args){

            Berrybrew BB = new Berrybrew();

            if (args.Length != 0 && args[0] == "debug"){
                BB.Debug = true;
                args = args.Skip(1).ToArray();
            }
            if (BB.Debug){
                Console.WriteLine("\nberrybrew debugging enabled...\n");
                Console.WriteLine(
                    "install dir: {0}\nperl root dir: {1}\ntemp dir: {2}",
                    BB.installPath, BB.rootPath, BB.archivePath
                );
            }

            if (args.Length == 0){
                BB.Message.Print("help");
                Environment.Exit(0);
            }

            switch (args[0]){
                case "available":
                    BB.Available();
                    break;

                case "clean":
                    if (args.Length > 1){
                        if (args[1].StartsWith("h"))
                            BB.Message.Say("subcmd.clean");
                        else
                            BB.Clean(args[1]);
                    }
                    else
                        BB.Clean();

                    break;

                case "clone":
                    if (args.Length != 3)
                        BB.Message.Say("clone_command_usage");

                    bool ok = BB.Clone(args[1], args[2]);

                    if (! ok)
                        Environment.Exit(0);

                    break;

                case "config":
                    string cwd = Directory.GetCurrentDirectory();

                    if (String.Equals(cwd, @"c:\berrybrew", StringComparison.OrdinalIgnoreCase)){
                        Console.WriteLine("\nAt this time, berrybrew can not be installed in C:\\berrybrew. Please move the directory and try again\n");
                        Environment.Exit(0);
                    }

                    BB.Config();
                    break;

                case "exec":
                    if (args.Length == 1)
                        BB.Message.Say("exec_command_required");

                    args[0] = "";

                    if (args[1] == "-h" || args[1] == "help")
                        BB.Message.Say("subcmd.exec");

                    BB.ExecCompile(String.Join(" ", args).Trim());
                    break;

                case "fetch":
                    bool allPerls = false;
                    if (args.Length > 1){
                        if (args[1].StartsWith("h"))
                            BB.Message.Say("subcmd.fetch");
                        else
                            allPerls = args[1].Equals("all");
                    }

                    BB.PerlUpdateAvailableList(allPerls);
                    BB.PerlUpdateAvailableListOrphans();
                    break;

                case "help":
                    if (args.Length == 1) {
                        BB.Message.Say("help");
                    } else {
                        switch (args[1].ToLower()) {
                            case "clean":
                                BB.Message.Say("subcmd.clean");
                                break;

                            case "exec":
                                BB.Message.Say("subcmd.exec");
                                break;

                            case "fetch":
                                BB.Message.Say("subcmd.fetch");
                                break;

                            case "use":
                                BB.Message.Say("subcmd.use");
                                break;

                            default:
                                BB.Message.Say("help");
                                break;
                        }
                    }
                    break;

                case "install":
                    if (args.Length == 1)
                        BB.Message.Say("install_ver_required");

                    try {
                        BB.Install(args[1]);
                    }

                    catch (ArgumentException error){
                        if (BB.Debug)
                            Console.WriteLine(error);

                        BB.Message.Say("install_ver_unknown");
                    }

                    break;

                case "license":
                    if (args.Length == 1)
                        BB.Message.Say("license");

                    break;

                case "off":
                    BB.Off();
                    break;

                case "register":
                    if (args.Length == 1)
                        BB.Message.Say("register_ver_required");

                    BB.PerlRegisterCustomInstall(args[1]);
                    break;

                case "remove":
                    if (args.Length == 1)
                        BB.Message.Say("remove_ver_required");

                    BB.PerlRemove(args[1]);
                    break;

                case "switch":
                    if (args.Length == 1)
                        BB.Message.Say("switch_ver_required");

                    BB.Switch(args[1]);
                    break;

                case "unconfig":
                    BB.Unconfig();
                    break;

                case "upgrade":
                    BB.Upgrade();
                    break;

                case "use": // pryrt's added feature
                    if (args.Length == 1) {
                        BB.Message.Say("use_ver_required");
                    }
                    switch(args[1].ToLower()) {
                        case "-h":
                        case "help":
                        case "-help":
                        case "--help":
                            BB.Message.Say("subcmd.use");
                            break;
                        case "--win":
                        case "--window":
                        case "--windowed":
                            if(args.Length<3)
                                BB.Message.Say("use_ver_required");
                            else
                                BB.UseCompile(args[2], true);
                            break;
                        default:
                            BB.UseCompile(args[1], false);
                            break;
                    }
                    break;

                case "version":
                    Console.WriteLine(BB.Version());
                    break;

                default:
                    BB.Message.Say("help");
                    break;
            }
        }
    }
}
