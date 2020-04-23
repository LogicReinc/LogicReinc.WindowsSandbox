using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.WindowsSandbox
{
    public static class Global
    {
        public static string ApplicationPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string Relative(string path)
        {
            if (Path.IsPathRooted(path))
                return path;
            return Path.Combine(ApplicationPath, path);
        }
    }
}
