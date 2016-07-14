using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerryBrew;

namespace BBConsole
{
    class bbconsole
    {
        static void Main(string[] args)
        {
            Berrybrew BB = new Berrybrew();
            
            if (BB.Debug)
            {
                Console.WriteLine("\nberrybrew debugging enabled...\n");
            }

            if (args.Length == 0)
            {
                BB.Message.Print("help");
                Environment.Exit(0);
            }

            switch (args[0])
            {
                case "version":
                    BB.Message.Print("version");
                    break;

                case "install":
                    if (args.Length == 1)
                    {
                        BB.Message.Print("install_ver_required");
                        Environment.Exit(0);
                    }
                    try
                    {
                        BB.Install(args[1]);
                    }
                    catch (ArgumentException error)
                    {
                        if (BB.Debug)
                            Console.WriteLine(error);

                        BB.Message.Print("install_ver_unknown");
                        Environment.Exit(0);
                    }
                    break;

                case "clean":
                    BB.Clean();
                    break;

                case "off":
                    BB.Off();
                    break;

                case "switch":
                    if (args.Length == 1)
                    {
                        BB.Message.Print("switch_ver_required");
                        Environment.Exit(0);
                    }
                    BB.Switch(args[1]);
                    break;

                case "available":
                    BB.Available();
                    break;

                case "config":
                    BB.Config();
                    break;

                case "remove":
                    if (args.Length == 1)
                    {
                        BB.Message.Print("remove_ver_required");
                        Environment.Exit(0);
                    }
                    BB.PerlRemove(args[1]);
                    break;

                case "exec":
                    if (args.Length == 1)
                    {
                        BB.Message.Print("exec_command_required");
                        Environment.Exit(0);
                    }
                    args[0] = "";
                    BB.ExecCompile(String.Join(" ", args).Trim());
                    break;

                case "license":
                    if (args.Length == 1)
                    {
                        BB.Message.Print("license");
                        Environment.Exit(0);
                    }
                    break;

                default:
                    BB.Message.Print("help");
                    break;
            }
        } 
    }
}