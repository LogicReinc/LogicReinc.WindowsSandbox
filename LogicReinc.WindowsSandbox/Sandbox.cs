using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.WindowsSandbox
{
    public class Sandbox
    {
        public const string SANDBOX_USER = "C:/users/WDAGUtilityAccount/";
        public const string SANDBOX_DESKTOP = "C:/users/WDAGUtilityAccount/Desktop/";
        public const string SANDBOX_DIRECTORY = "Sandboxes";

        public SandboxDescriptor Descriptor { get; set; }

        public bool Prepared { get; set; }

        Dictionary<string, ComponentDescriptor> comps = new Dictionary<string, ComponentDescriptor>();

        public Sandbox(SandboxDescriptor desc, List<ComponentDescriptor> comps)
        {
            Descriptor = desc;
            if (comps != null)
                this.comps = comps.ToDictionary(x => x.ID, y => y);
        }

        public void Prepare()
        {
            if (!Prepared)
            {
                string[] missing = Descriptor.Components.Where(x => !comps.ContainsKey(x)).ToArray();
                if (missing.Length > 0)
                    throw new ArgumentException("Missing Components: " + string.Join(",", missing));

                List<ComponentDescriptor> descs = Descriptor.Components.Select(x => comps[x]).ToList();

                List<string> commands = new List<string>();
                WSB wsb = new WSB()
                {
                    VGPU = (Descriptor.GPU) ? "Enable" : "Disable"
                };

                string sandboxID = Descriptor.ID;
                string sandboxPath = Global.Relative(Path.Combine(SANDBOX_DIRECTORY, sandboxID));
                string sandboxRepository = Path.Combine(sandboxPath, "Repository");
                string sandboxData = Path.Combine(sandboxPath, "Data");

                Directory.CreateDirectory(sandboxRepository);
                Directory.CreateDirectory(sandboxData);

                wsb.MappedFolders.Add(new WSBFolder()
                {
                    HostFolder = sandboxRepository,
                    ReadOnly = true
                });
                wsb.MappedFolders.Add(new WSBFolder()
                {
                    HostFolder = sandboxData,
                    ReadOnly = false
                });
                Dictionary<string, string> defaultPaths = GetDefaultPaths();

                foreach (ComponentDescriptor descriptor in descs)
                {
                    Dictionary<string, string> paths = GetDefaultPaths();

                    //Setup read-only references
                    foreach (var r in descriptor.References)
                    {
                        string refID = r.Key;
                        string refPath = Relative(descriptor.SourceDirectory, r.Value);

                        if (Directory.Exists(refPath))
                        {
                            //Reference directory directly as readonly
                            wsb.MappedFolders.Add(new WSBFolder()
                            {
                                HostFolder = refPath,
                                ReadOnly = true
                            });
                            //Add internal path
                            paths.Add(refID, Path.Combine(SANDBOX_DESKTOP, new DirectoryInfo(refPath).Name));
                        }
                        else if (File.Exists(refPath))
                        {
                            //Copy file to shared Repository directory
                            string repoRefPath = Path.Combine(sandboxRepository, Path.GetFileName(refPath));
                            if (!File.Exists(repoRefPath))
                                File.Copy(refPath, repoRefPath);
                            //Add internal path
                            paths.Add(refID, Path.Combine(SANDBOX_DESKTOP, "Repository", Path.GetFileName(refPath)));
                        }
                        else
                            throw new ArgumentException($"Component '{descriptor.ID}' contains unknown path '{refPath}'");
                    }

                    //Setup write-able data (copy)
                    foreach(var data in descriptor.Data)
                    {
                        string dataID = data.Key;
                        string dataPath = Relative(descriptor.SourceDirectory, data.Value);

                        if (Directory.Exists(dataPath))
                        {
                            DirectoryInfo sourceDir = new DirectoryInfo(dataPath);
                            if (!Directory.Exists(Path.Combine(sandboxData, sourceDir.Name)))
                            {
                                Directory.CreateDirectory(new DirectoryInfo(Path.Combine(sandboxData, sourceDir.Name)).FullName);
                                CopyDirectory(sourceDir, new DirectoryInfo(Path.Combine(sandboxData, sourceDir.Name)));
                            }

                            //Add internal path
                            paths.Add(dataID, Path.Combine(SANDBOX_DESKTOP, "Data", sourceDir.Name));
                        }
                        else if (File.Exists(dataPath))
                        {
                            //Copy file to shared Repository directory
                            string repoRefPath = Path.Combine(sandboxData, Path.GetFileName(dataPath));
                            if (!File.Exists(repoRefPath))
                                File.Copy(dataPath, repoRefPath);
                            //Add internal path
                            paths.Add(dataID, Path.Combine(SANDBOX_DESKTOP, "Data", Path.GetFileName(dataPath)));
                        }
                        else
                            throw new ArgumentException($"Component '{descriptor.ID}' contains unknown path '{dataPath}'");
                    }

                    foreach (string preserve in descriptor.Preserve)
                    {
                        string fromPath = ReplacePaths(preserve, paths);
                        string toPath = ReplacePaths("{Data}/_" + new DirectoryInfo(preserve).Name + "_", paths);

                        commands.Add(mkdir(Path.GetDirectoryName(fromPath)));
                        commands.Add(mkdir(toPath));
                        commands.Add(Symlink(fromPath, toPath));
                    }
                    
                    foreach(string command in descriptor.Startup)
                        commands.Add(ReplacePaths(command, paths));

                    foreach(string shortcut in descriptor.Shortcuts)
                    {
                        commands.Add(mklink(ReplacePaths("{Desktop}/" + Path.GetFileName(shortcut), paths), ReplacePaths(shortcut, paths)));
                    }
                }

                File.WriteAllLines(Relative(sandboxRepository, "startup.bat"), commands);
                wsb.LogonCommand = new WSBLogon()
                {
                    Command = ReplacePaths("{Repository}/startup.bat", defaultPaths)
                };
                File.WriteAllText(Relative(sandboxPath, "sandbox.wsb"), wsb.Serialize());

                Prepared = true;
            }
        }

        public void Run()
        {
            string sandboxID = Descriptor.ID;
            string sandboxPath = Global.Relative(Path.Combine(SANDBOX_DIRECTORY, sandboxID));
            string sandboxWSBPath = Relative(sandboxPath, "sandbox.wsb");

            string filePath = new FileInfo(sandboxWSBPath).FullName;
            Console.WriteLine(filePath);
            Process.Start("explorer.exe", filePath);
        }

        private string Symlink(string source, string target)
        {
            return $"mklink /J \"{source}\" \"{target}\"";
        }
        private string mkdir(string path)
        {
            return $"mkdir \"{path}\"";
        }
        private string mklink(string source, string to)
        {
            return $"mklink \"{source}\" \"{to}\"";
        }

        private Dictionary<string,string> GetDefaultPaths()
        {
            return new Dictionary<string, string>()
            {
                { "AppData",  $"{SANDBOX_USER}/AppData/" },
                { "Desktop", $"{SANDBOX_DESKTOP}" },
                { "Data", $"{SANDBOX_DESKTOP}/Data" },
                { "Repository", $"{SANDBOX_DESKTOP}/Repository" }
            };
        }

        private void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyDirectory(dir, destination.CreateSubdirectory(dir.Name));
            if (source.Name == "Application")
                source = source;
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(destination.FullName, file.Name));
        }
        private string ReplacePaths(string input, Dictionary<string,string> paths)
        {
            foreach (var path in paths)
                input = input.Replace("{" + path.Key + "}", path.Value);
            return input;
        }

        private string Relative(string parent, string rela)
        {
            if (Path.IsPathRooted(rela))
                return rela;
            if (parent == null)
                return Global.Relative(rela);
            return Path.Combine(parent, rela);
        }
    }
}
