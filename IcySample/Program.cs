using Icy.Container;
using Icy.Database;
using Icy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IcyApp = Icy.Foundation.Application;

namespace IcySample
{

    interface IRoarrer
    {
        void roar();
    }
    interface ILion
    {
        void scarePeople();
    }

    class Roarrer: IRoarrer
    {
        public void roar()
        {
            Console.WriteLine("ROAR!!!");
        }
    }
    class Lion : ILion
    {
        IRoarrer roarrer;
        int volume;
        string name;

        public Lion(int volume = 50, IRoarrer test = null, string name = "Alex") {
            this.volume = volume;
            this.name = name;
            this.roarrer = test; // HERE's THE MAGIC! IRoarrer is an passed automatically as instance of Roarrer
        }

        public void scarePeople()
        {
            Console.WriteLine(this.name + " the Lion is about to roar at " + this.volume.ToString() + "dB!");
            this.roarrer.roar();
        }
    }

    class Program
    {
        static IcyApp app;

        static void Main(string[] args)
        {
            // initialize the app
            app = new IcyApp();

            app.bind<IRoarrer, Roarrer>();
            app.bind<ILion, Lion>();

            var i = app.make<ILion>(new Dictionary<string, object>() {
                {"0", 100 } // change the volume to 100
            });
            i.scarePeople(); // OUTPUT: ROAR!!!
        }
    }
}
