using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHashConverter.Test_Data
{
    public class Parent
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; } 
        public List<ChildModel> Children { get; set; }

        public class ChildModel
        {
            public int id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }

    
}
