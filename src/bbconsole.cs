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
                    bb.InstallPath, bb.RootPath, bb.ArchivePath
                );
            }

            if (args.Length == 0){
                bb.Message.Print("help");
                Environment.Exit(0);
            }

            switch (args[0]){
                case "available":
                    bb.Available();
                    break;

                case "list":
                    bb.List();
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
                    bool allPerls = false;
                    if (args.Length > 1){
                        if (args[1].StartsWith("h"))
                            bb.Message.Say("subcmd.fetch");
                        else
                            allPerls = args[1].Equals("all");
                    }

                    bb.PerlUpdateAvailableList(allPerls);
                    bb.PerlUpdateAvailableListOrphans();
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

                case "off":
                    bb.Off();
                    break;

                case "register":
                    if (args.Length == 1)
                        bb.Message.Say("register_ver_required");

                    bb.PerlRegisterCustomInstall(args[1]);
                    break;

                case "remove":
                    if (args.Length == 1)
                        bb.Message.Say("remove_ver_required");

                    bb.PerlRemove(args[1]);
                    break;

                case "switch":
                    if (args.Length == 1)
                        bb.Message.Say("switch_ver_required");

                    bb.Switch(args[1]);
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
