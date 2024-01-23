using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3ai
{
    internal class Hidden
    {
        public double value { get; set; } = 0;
        public double bias { get; set; } = 0.1;
        public List<double> inputW = new List<double>();
        public List<double> outputW = new List<double>();
    }
}
