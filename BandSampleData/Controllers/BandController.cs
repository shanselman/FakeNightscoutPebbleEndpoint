using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace HanselmanBloodSugarMSBand.Controllers
{
    public class BandController : ApiController
    {
        public RootObject Get()
        {
            long now = ToEpoch(DateTime.Now);
            int sgv = MakeFakeBloodSugarValue();

            RootObject root = new RootObject();
            root.status = new List<Status>();
            root.status.Add(new Status() { now = now } );
            root.bgs = new List<Bg>();
            root.bgs.Add(new Bg() { datetime = now, sgv = sgv.ToString(), bgdelta = -1, trend = 4, direction = "Flat" });

            return root;
        }

        //Algorithm from http://stackoverflow.com/a/8597889/6380
        /* rnd = Random_Float(); // generate number, 0 <= x < 1.0
        change_percent = 2 * volatility * rnd;
        if (change_percent > volatility)
            change_percent -= (2 * volatility);
        change_amount = old_price * change_percent;
        new_price = old_price + change_amount; 
         */
        private int MakeFakeBloodSugarValue()
        {
            double volatility = 0.03;
            double old_price = 120; //Default value for mg/dl

            object old_price_obj = HttpContext.Current.Cache.Get("old_price");
            if (old_price_obj != null)
            {
                old_price = Convert.ToDouble(old_price_obj);
            }
            
            double rnd = new Random().NextDouble();
            double change_percent = 2 * volatility * rnd;
            if (change_percent > volatility)
                change_percent -= (2 * volatility);
            double change_amount = old_price * change_percent;
            double new_price = old_price + change_amount;

            HttpContext.Current.Cache.Insert("old_price", new_price);
            
            return (int)new_price;
        }

        private long ToEpoch(DateTime d)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long secondsSinceEpoch = (long)t.TotalMilliseconds;
            return secondsSinceEpoch;
        }
    }

    public class Status
    {
        public long now { get; set; }
    }

    public class Bg
    {
        public string sgv { get; set; }
        public int bgdelta { get; set; }
        public int trend { get; set; }
        public string direction { get; set; }
        public long datetime { get; set; }
    }

    public class RootObject
    {
        public List<Status> status { get; set; }
        public List<Bg> bgs { get; set; }
    }

}
