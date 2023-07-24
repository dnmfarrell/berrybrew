using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BerryBrew.PerlInstance {
    public struct StrawberryPerl {
        public readonly string Name;
        public readonly string File;
        public readonly string Url;
        public readonly string Version;
        public readonly string Sha1Checksum;
        public readonly bool Newest;
        public readonly bool Custom;
        public readonly bool Virtual;
        public readonly string archivePath;
        public readonly string installPath;
        public readonly string CPath;
        public readonly string PerlPath;
        public readonly string PerlSitePath;
        public readonly List<string> Paths;

        public StrawberryPerl(
            Berrybrew bb,
            object name,
            object file,
            object url,
            object version,
            object csum,
            bool newest = false,
            bool custom = false,
            bool virtual_install = false,
            string perl_path = "",
            string lib_path = "",
            string aux_path = ""
        )
        {
            if (!virtual_install)
            {
                if (string.IsNullOrEmpty(perl_path))
                {
                    perl_path = bb.rootPath + name + @"\perl\bin";
                }

                if (string.IsNullOrEmpty(lib_path))
                {
                    lib_path = bb.rootPath + name + @"\perl\site\bin";
                }

                if (string.IsNullOrEmpty(aux_path))
                {
                    aux_path = bb.rootPath + name + @"\c\bin";
                }
            }

            Name = name.ToString();
            Custom = custom;
            Newest = newest;
            Virtual = virtual_install;
            File = file.ToString();
            Url = url.ToString();
            Sha1Checksum = csum.ToString();
            Version = version.ToString();

            archivePath = bb.archivePath;
            installPath = bb.rootPath + name;

            CPath = aux_path;
            PerlPath = perl_path;
            PerlSitePath = lib_path;

            Paths = new List<string>
            {
                CPath, PerlPath, PerlSitePath
            };
        }
    }
}