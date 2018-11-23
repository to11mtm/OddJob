using System;
using System.Linq;

namespace GlutenFree.OddJob.Serializable
{
    public class OddJobSerializedParameter
    {
        public OddJobSerializedParameter()
        {

        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string TypeName { get; set; }

    }
}
