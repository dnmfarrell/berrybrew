using BerryBrew;
using BerryBrew.PerlInstance;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;

namespace BerryBrew.PathOperations {
    public class PathOp {
        private Berrybrew bb = null;

        public PathOp(Berrybrew bb) { }

        internal void PathAddBerryBrew(string binPath) {
            string path = PathGet();
            List<string> newPath = new List<string>();

            if (path == null) {
                newPath.Add(binPath);
            }
            else {
                if (path[path.Length - 1] == ';') {
                    path = path.Substring(0, path.Length - 1);
                }

                newPath.Add(binPath);
                newPath.Add(path);
            }
            PathSet(newPath);
        }

        internal void PathAddPerl(StrawberryPerl perl) {
            string path = PathGet();
            List<string> newPath = perl.Paths;

            string[] entries = path.Split(new char [] {';'});

            foreach (string p in entries) {
                newPath.Add(p);
            }
            PathSet(newPath);
        }

        internal static string PathGet() {
            const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\";
            string path = null;

            if (Registry.LocalMachine != null) {
                path = (string) Registry.LocalMachine.OpenSubKey(keyName).GetValue(
                    "Path",
                    "",
                    RegistryValueOptions.DoNotExpandEnvironmentNames
                );
            }
            return path;
        }

        internal static string PathGetUsr() {
            const string keyName = @"Environment\";
            string path = (string)Registry.CurrentUser.OpenSubKey(keyName).GetValue(
                "PATH",
                "",
                RegistryValueOptions.DoNotExpandEnvironmentNames
            );
            return path;
        }

        internal void PathRemoveBerrybrew(string binPath) {
            string path = PathGet();
            List<string> paths = path.Split(new char[] {';'}).ToList();
            List<string> updatedPaths = new List<string>();

            foreach (string pathEntry in paths) {
                if (pathEntry.ToLower() != binPath.ToLower()) {
                    updatedPaths.Add(pathEntry);
                }
            }

            PathSet(updatedPaths);
        }

        internal void PathRemovePerl(OrderedDictionary _perls, bool process=true) {
            string path = PathGet();

            if (path == null) {
                return;
            }

            var paths = path.Split(new char[] {';'}).ToList();

            foreach (StrawberryPerl perl in _perls.Values) {
                for (var i = 0; i < paths.Count; i++) {
                    if (paths[i] == perl.PerlPath
                        || paths[i] == perl.CPath
                        || paths[i] == perl.PerlSitePath){
                        paths[i] = "";
                    }
                }
            }

            paths.RemoveAll(string.IsNullOrEmpty);

            if (process) {
                PathSet(paths);
            }
        }

        internal static bool PathScan(string binPath, string target) {
            var envTarget = target == "machine" ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            string paths = Environment.GetEnvironmentVariable("path", envTarget);

            foreach (string path in paths.Split(new char[]{';'})) {
                if (path == binPath) {
                    return true;
                }
            }

            return false;
        }

        internal void PathSet(List<string> path)  {
            path.RemoveAll(string.IsNullOrEmpty);

            string paths = string.Join(";", path);

            if (!paths.EndsWith(@";"))  {
                paths += @";";
            }

            try  {
                const string keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

                using (RegistryKey pathKey = Registry.LocalMachine.OpenSubKey(keyName, true)) {

                    pathKey.DeleteValue("Path");

                    pathKey.SetValue(
                        "Path",
                        paths,
                        RegistryValueKind.ExpandString
                    );
                }

                Berrybrew.SendMessageTimeout(
                    Berrybrew.HwndBroadcast,
                    Berrybrew.WmSettingchange,
                    IntPtr.Zero,
                    "Environment",
                    Berrybrew.SmtoAbortifhung,
                    100,
                    IntPtr.Zero
                );
            }
            catch (Exception err) {
                if (err is UnauthorizedAccessException || err is SecurityException) {
                    Console.Error.WriteLine("\nModifying the PATH environment variable requires Administrator privilege");
                    if (bb.Debug) {
                        Console.Error.WriteLine("DEBUG: {0}", err);
                    }

                    bb.Exit((int) Berrybrew.ErrorCodes.ADMIN_PATH_ERROR);
                }
                throw;
            }
        }
    }
}