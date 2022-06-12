using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsideFabric.Data;

namespace EnumBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new HomewareCategoryManager();
            var enumText = manager.GenerateCategoryEnum(manager.LoadTree());
        }
    }
}
