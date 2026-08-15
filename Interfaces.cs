using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG_281_Project
{
        public interface IDispatchable
        {
            void Dispatch();
        }

        public interface IMonitorable
        {
            void Monitor();
        }

        public interface ILogger
        {
            void Log(string message);
        }
}
