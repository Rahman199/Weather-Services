using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;



namespace Using_Web_Services
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Weather_Data> weatherDataList = new List<Weather_Data>();
            List<float> temperatures = new List<float>();
            List<int> changes = new List<int>();
            List<int> timeStamps = new List<int>();

            string result = retrieveData().Result;

            //   ==================================
            //              CLEANING DATA
            //   ==================================
    
            // Removing pre tags
            result = Regex.Replace(result, @"<[^>]*>", "");
            string[] resultSeparated = result.Split('\n');

            Console.WriteLine("Total " + (resultSeparated.Length-2).ToString() + " Objects recieved");

            // Saving to on.csv
            Console.WriteLine("\nWriting recieved data to CSV..");
            File.WriteAllText("on.csv", result);
            Console.WriteLine("Data written to CSV\n");

            // removing header and footer elements
            resultSeparated = resultSeparated.Skip(1).ToArray();
            resultSeparated = resultSeparated.Take(resultSeparated.Length - 1).ToArray();

            var csv = new StringBuilder();
            string head = string.Format("{0};{1};{2};{3}", "Number", "Time", "Temperature", "Change");
            csv.AppendLine(head);

            //   ==================================
            //      storing into the object array
            //   ==================================
            
            foreach (string sResult in resultSeparated)
            {
                string[] dataSplitted = sResult.Split(';');
                int Nr = Convert.ToInt32( dataSplitted[0]);
                int UnixTimeStamp= Convert.ToInt32(dataSplitted[1]);
                string zeit = dataSplitted[2];
                float temperature = 0;
                float humidity = 0;
                float druck = 0;
                float PM10 = 0;
                float PM25 = 0;
                float.TryParse(dataSplitted[3],out temperature);
                float.TryParse(dataSplitted[4],out humidity);
                float.TryParse(dataSplitted[5],out druck);
                float.TryParse(dataSplitted[6],out PM10);
                float.TryParse(dataSplitted[7],out PM25);

                string str = "0";
                // Analyze the temprature using the web service
                if (weatherDataList.Count > 0)
                {
                    Console.WriteLine("\nPosting "+ Convert.ToInt32(weatherDataList[weatherDataList.Count - 1]
                        .temperature).ToString() + ","+ Convert.ToInt32(temperature).ToString()+" to Analyse web service ...");
                    str = AnalyseData(Convert.ToInt32(weatherDataList[weatherDataList.Count-1].temperature),
                        Convert.ToInt32(temperature)).Result;
                    Console.WriteLine("Recieved : " + str);
                }

                string newLine = string.Format("{0};{1};{2};{3}", Nr.ToString(), UnixTimeStamp.ToString(),
                    temperature.ToString(), str);
                csv.AppendLine(newLine);

                Weather_Data data = new Weather_Data(Nr, UnixTimeStamp, zeit, temperature, humidity,
                                                        druck, PM10, PM25, Convert.ToInt32(str));
                weatherDataList.Add(data);

                // preparing for the charts
                temperatures.Add(temperature);
                changes.Add(Convert.ToInt32(str));
                timeStamps.Add(UnixTimeStamp);
            }
            File.WriteAllText("From.csv", csv.ToString());
            Console.WriteLine("\nAnalysed Data written to csv\n");

            Console.WriteLine("Saving chart as png");
            Chart chart1 = new Chart();

            // configurations for chart area for chart
            var chartArea = new ChartArea();
            chartArea.Name = "Temperature Graph";
            chartArea.AxisX.Title = "Time Stamps (UNIX)";
            chartArea.AxisY.Title = "Temperature";
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisX.IntervalOffset = 1000;
            chartArea.AxisX.Minimum = 1611400000;
            
            // chart configurations
            chart1.Size = new Size(1024, 720);
            chart1.ChartAreas.Add(chartArea);

            // creating series for chart
            Series series1 = new Series();
            series1.Name = "";
            series1.Color = Color.Gray;
            series1.ChartType = SeriesChartType.Line;
            chart1.Series.Add(series1);

            for(int i = 0; i<temperatures.Count; i++)
            {
                // adding data points for chart
                chart1.Series[0].Points.AddXY(timeStamps[i], temperatures[i]);
                
                // Deciding the colors
                if (changes[i] == 1)
                    chart1.Series[0].Points[i].Color = Color.Red;
                else if (changes[i] == -1)
                    chart1.Series[0].Points[i].Color = Color.Blue;
            }
            chart1.SaveImage("temperature graph.png", ChartImageFormat.Png);
            
            Console.WriteLine("Chart Saved as temerature graph.png");

            Console.ReadLine();
        }
        static async Task<string> retrieveData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://cbrell.de/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                Console.WriteLine("GET call Started");

                // API request to data
                HttpResponseMessage responseMessage = await client.GetAsync("bwi403/demo/getKlimaWS.php");
                if (responseMessage.IsSuccessStatusCode)
                {
                    // saving the response 
                    Task<string> res =  responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine("Successfull cal made :: Object Recieved from the web service");
                    return res.Result;
                }
                else
                {
                    Console.WriteLine("Failed to get data");
                    return null;
                }
            }
        }
        static async Task<string> AnalyseData(int a, int b)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://cbrell.de/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                // sending request to analyzedata API
                HttpResponseMessage responseMessage = await client.GetAsync("bwi403/demo/analyseWS.php?x="+a+";"+b);
                if (responseMessage.IsSuccessStatusCode)
                {
                    //  if the call was successfull
                    Task<string> res = responseMessage.Content.ReadAsStringAsync();
                    return res.Result;
                }
                else
                {
                    // If the call was unsuccessfull
                    Console.WriteLine("Failed to get data");
                    return null;
                }
            }
        }
    }   

}
