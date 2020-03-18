using DatabaseInterpreter.Model;
using DatabaseMigration.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseMigration.Demo
{
    public class ConvertorDemoRuner
    {
        public static async Task Run(ConvertorDemo demo)
        {
            await demo.Convert();
        }
    }
}
