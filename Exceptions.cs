using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public class CapacityExceededException : Exception
    {
        public CapacityExceededException(string message): base(message)
        {
        }
    }

    public class WorkforceShortageException : Exception
    {
        public WorkforceShortageException(string message): base(message)
        {
        }
    }
}
