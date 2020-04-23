using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.WindowsSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Would you like to register .sandbox files for this .exe? (y/n)");

                    if (Console.ReadLine().ToLower() == "y")
                    {
                        FileAssociations.EnsureAssociationsSet(new FileAssociation()
                        {
                            ExecutableFilePath = Assembly.GetExecutingAssembly().Location,
                            Extension = ".sandbox",
                            FileTypeDescription = "WSB Wrapper",
                            ProgId = "LR_Sandbox"
                        });
                        Console.WriteLine("Done");
                        Console.ReadLine();
                    }
                    else
                        return;
                }
                else
                {
                    SandboxDescriptor sandbox = JsonConvert.DeserializeObject<SandboxDescriptor>(File.ReadAllText(args[0]));

                    List<ComponentDescriptor> comps = ComponentDescriptor.GetDescriptors();


                    Sandbox box = new Sandbox(sandbox, comps);

                    box.Prepare();

                    box.Run();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception:" + ex.Message);
                Console.WriteLine("Stacktrace:" + ex.StackTrace);
                Console.ReadLine();
            }
        }


        public static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            // Delete the key instead of trying to change it
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
            key.DeleteSubKey("UserChoice", false);
            key.Close();

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
