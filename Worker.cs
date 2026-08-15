using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG_281_Project
{
    public abstract class Worker
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Available { get; set; }

        public Worker(int id, string name)
        {
            Id = id;
            Name = name;
            Available = true;
        }

        public abstract string GetRole();
    }


    public class Picker : Worker
    {
        public Picker(int id, string name)
            : base(id, name)
        {
        }

        public override string GetRole()
        {
            return "Picker";
        }
    }


    public class Driver : Worker
    {
        public Driver(int id, string name)
            : base(id, name)
        {
        }

        public override string GetRole()
        {
            return "Driver";
        }
    }
}
