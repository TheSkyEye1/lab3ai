using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3ai
{
    public class DataSet
    {
        public int number;
        public byte[] image_data;

        public DataSet(List<string> values)
        {
            number = int.Parse(values[0]);
            image_data = new byte[28 * 28];

            for(int i = 1; i<values.Count; i++)
            {
                image_data[i-1] = byte.Parse(values[i]);
            }
        }
    }
}
