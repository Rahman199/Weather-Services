using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Using_Web_Services
{
    class Weather_Data
    {
        public int Nr { get; }
        public int UnixTimeStamp { get; }
        public string zeit { get; }
        public float temperature { get; }
        public float humidity { get; }
        public float druck { get; }
        public float PM10 { get; }
        public float PM25 { get; }
        public int change { get; }
        public Weather_Data(int nr, int unix, string zeit, float temp, float hum, float dr, float pm10, float pm25, int change)
        {
            this.Nr = nr;
            this.UnixTimeStamp = unix;
            this.zeit = zeit;
            this.temperature = temp;
            this.humidity = hum;
            this.druck = dr;
            this.PM10 = pm10;
            this.PM25 = pm25;
            this.change = change;
        }
    }
}
