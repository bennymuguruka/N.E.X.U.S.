using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG_281_Project
{
    internal class Events
    {
        public delegate void WarehouseEventHandler(string message);

        public class WarehouseEvents
        {
            public event WarehouseEventHandler Alert;

            public void RaiseAlert(string message)
            {
                Alert?.Invoke(message);
            }
        }

    }
}
