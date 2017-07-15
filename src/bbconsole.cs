using System;
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
                    
                    if (! BB.Clone(args[1], args[2]))
                        Environment.Exit(0);

                    break;

                case "config":
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
                    BB.PerlUpdateAvailableList();
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
