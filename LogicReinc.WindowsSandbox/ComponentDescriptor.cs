using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.WindowsSandbox
{
    public class ComponentDescriptor
    {
        public string ID { get; set; }
        public Dictionary<string, string> References { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public List<string> Startup { get; set; } = new List<string>();
        public List<string> Preserve { get; set; } = new List<string>();

        public List<string> Shortcuts { get; set; } = new List<string>();

        public string SourcePath { get; set; }
        public string SourceDirectory { get; set; }

        public static List<ComponentDescriptor> GetDescriptors()
        {
            string compPath = Global.Relative("Components");
            if (Directory.Exists(compPath))
            {
                List<ComponentDescriptor> descs = new List<ComponentDescriptor>();

                foreach(FileInfo file in new DirectoryInfo(compPath).GetFiles().Where(x=>x.Extension == ".json"))
                {
                    try
                    {
                        ComponentDescriptor desc = JsonConvert.DeserializeObject<ComponentDescriptor>(File.ReadAllText(file.FullName));
                        desc.SourcePath = file.FullName;
                        desc.SourceDirectory = file.DirectoryName;
                        descs.Add(desc);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Failed to load component '{file.Name}' due to  {ex.Message}");
                    }
                }
                foreach(DirectoryInfo dir in new DirectoryInfo(compPath).GetDirectories())
                {
                    string configPath = Path.Combine(dir.FullName, "config.json");
                    if(File.Exists(configPath))
                    {
                        try
                        {
                            ComponentDescriptor desc = JsonConvert.DeserializeObject<ComponentDescriptor>(File.ReadAllText(configPath));
                            desc.SourcePath = configPath;
                            desc.SourceDirectory = dir.FullName;
                            descs.Add(desc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to load component '{dir.Name}' due to  {ex.Message}");
                        }

                    }
                }

                return descs;
            }
            else
                return new List<ComponentDescriptor>();
        }
    }
}
