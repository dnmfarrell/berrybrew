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

            if (args.Length == 0)
            {
                BB.Print("help");
                Environment.Exit(0);
            }

            switch (args[0])
            {
                case "version":
                    BB.Print("version");
                    break;

                case "install":
                    if (args.Length == 1)
                    {
                        string install_ver_required = BB.Messages("install_ver_required");
                        Console.WriteLine(install_ver_required);
                        Environment.Exit(0);
                    }
                    try
                    {
                        StrawberryPerl perl = BB.ResolveVersion(args[1]);
                        string archive_path = BB.Fetch(perl);
                        BB.Extract(perl, archive_path);
                        BB.Available();
                    }
                    catch (ArgumentException)
                    {
                        string install_ver_unknown = BB.Messages("install_ver_unknown");
                        Console.WriteLine(install_ver_unknown);
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
                        string switch_ver_required = BB.Messages("switch_ver_required");
                        Console.WriteLine(switch_ver_required);
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
                        string remove_ver_required = BB.Messages("remove_ver_required");
                        Console.WriteLine(remove_ver_required);
                        Environment.Exit(0);
                    }
                    BB.RemovePerl(args[1]);
                    break;

                case "exec":
                    if (args.Length == 1)
                    {
                        string exec_command_required = BB.Messages("exec_command_required");
                        Console.WriteLine(exec_command_required);
                        Environment.Exit(0);
                    }
                    args[0] = "";
                    BB.CompileExec(String.Join(" ", args).Trim());
                    break;

                case "license":
                    if (args.Length == 1)
                    {
                        BB.Print("license");
                        Environment.Exit(0);
                    }
                    break;

                default:
                    BB.Print("help");
                    break;
            }
        } 
    }
}
