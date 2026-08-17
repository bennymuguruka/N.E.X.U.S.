using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
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
