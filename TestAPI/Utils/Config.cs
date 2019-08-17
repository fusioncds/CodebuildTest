using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAPI.Utils
{
    public class Config
    {


        public Config()
        {

        }
        public string ApplicationName { get; set; }
        public int Version { get; set; }

        public string AssemblyPath { get; set; }


    }
}