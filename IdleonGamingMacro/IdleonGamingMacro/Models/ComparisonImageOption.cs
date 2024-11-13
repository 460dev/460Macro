using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleonGamingMacro.Models
{
    internal class ComparisonImageOption
    {
        public double Threshold { get; private set; }
        public double Scale { get; private set; }
        public bool NoiseCancellation { get; private set; }  
        public bool Edge {  get; private set; } 

        public ComparisonImageOption(
            double threshold,
            double scale = 1.0,
            bool noiseCancellation = false,
            bool edge = false            
            )
        { 
            Threshold = threshold;
            Scale = scale;
            NoiseCancellation = noiseCancellation;
            Edge = edge;
        }

    }
}
