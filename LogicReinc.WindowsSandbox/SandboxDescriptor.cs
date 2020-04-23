using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.WindowsSandbox
{
    public class SandboxDescriptor
    {
        public string ID { get; set; }
        public bool GPU { get; set; } = true;
        public List<string> Components { get; set; }
    }
}
