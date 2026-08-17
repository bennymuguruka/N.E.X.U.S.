using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
        public interface IDispatchable
        {
            void Dispatch();
        }

        public interface IMonitorable
        {
            void CheckStatus();
        }

        public interface ILogger
        {
            void Log(string message);
        }
}
