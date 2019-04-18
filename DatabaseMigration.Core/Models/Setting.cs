using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class Setting
    {
        public int CommandTimeout { get; set; } = 600;
        public int DataBatchSize { get; set; } = 10000;
    }
}
