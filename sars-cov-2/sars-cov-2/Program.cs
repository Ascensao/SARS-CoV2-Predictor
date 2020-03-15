using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sars_cov_2
{
    static class Globals
    {
        public static List<string> countries;
        public static int c_selected;       //country selected
        public static bool locMenu_running; //menu cicle keeper
        public static char spread_phases;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SARS-CoV-2: Cumulative Stats & Predictions";
            MenuLogo();

            Globals.locMenu_running = true;
            Globals.countries = Directory.GetFiles(@"Countries", "*.txt", SearchOption.TopDirectoryOnly).ToList();
            Globals.countries.Sort();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select Location: ");
            Console.ForegroundColor = ConsoleColor.White;

            //show countries
            int counter = 0;
            foreach (string _c in Globals.countries)
            {
                counter++;
                Console.WriteLine(counter.ToString() + ". " + Path.GetFileNameWithoutExtension(_c));
            }

            //user location selection
            do
            {
                Int32.TryParse(Console.ReadLine(), out Globals.c_selected);
                if (Globals.c_selected == 0 || Globals.c_selected > counter)
                    Console.WriteLine("Incorrect number!");
                else
                {
                    Globals.c_selected = Globals.c_selected - 1; //normalize index for better human compreension.
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("\n" + Path.GetFileNameWithoutExtension(Globals.countries[Globals.c_selected]) + " selected.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Globals.locMenu_running = false;
                }
            } while (Globals.locMenu_running);

            //all file records to array "converted"
            int[] covid_stats = File.ReadAllLines(Globals.countries[Globals.c_selected])
                .Select(l => Convert.ToInt32(l))
                .ToArray();

            //get percentages of each day
            List<double> covid_percentage = new List<double>();
            for (int i = 0; i < covid_stats.Length - 1; i++)
            {
                double temp = (double)(covid_stats[i + 1] * 100) / covid_stats[i] - 100;
                covid_percentage.Add(temp);
            }

            //if country txt file don't have more then 10 regists then close app
            if (covid_percentage.Count < 10)
            {
                Console.WriteLine("The Program only can work properly if the text file have 10 or more records. Please press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Show values with percentage
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n■ Location Statistics ■\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(string.Format("{0,-10}{1}", "Cases", "New cases %"));
            for (int i = 0; i < covid_stats.Length; i++)
            {
                if (i == 0) //first value don't have percentage
                    Console.WriteLine(string.Format(" {0,-10}{1}", covid_stats[i].ToString(), "-"));
                else if (covid_percentage[i - 1] < 5) //if incremental percentage is less then 5 then show 2 decimal points.
                    Console.WriteLine(string.Format(" {0,-10}+ {1}%", covid_stats[i].ToString(), covid_percentage[i - 1].ToString("0.00")));
                else
                {
                    Console.WriteLine(string.Format(" {0,-10}+ {1}%", covid_stats[i].ToString(), covid_percentage[i - 1].ToString("0")));
                }
            }

            //Average statistic
            #region Average

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n\n■ Incremental Average ■");
            Console.ForegroundColor = ConsoleColor.White;

            //get average from all percentages
            double avg_all_percentages = Math.Round(covid_percentage.Sum() / covid_percentage.Count, 1);
            Console.WriteLine("\nAVG all time:      " + avg_all_percentages.ToString() + " %");

            //get average from last 10 elements
            List<double> last_10_percentages = new List<double>();
            for (int i = covid_percentage.Count - 10; i < covid_percentage.Count; ++i)
                last_10_percentages.Add(covid_percentage[i]);

            double avg_last10_percentage = Math.Round(last_10_percentages.Sum() / last_10_percentages.Count, 1);
            Console.WriteLine("AVG last 10 days:  " + avg_last10_percentage.ToString() + " %");

            //get average from last 5 elements
            List<double> last_5_percentages = new List<double>();
            for (int i = covid_percentage.Count - 5; i < covid_percentage.Count; ++i)
                last_5_percentages.Add(covid_percentage[i]);

            double avg_last5_percentage = Math.Round(last_5_percentages.Sum() / last_5_percentages.Count, 1);
            Console.WriteLine("AVG last  5 days:  " + avg_last5_percentage.ToString() + " %");

            //get average from last 3 elements
            List<double> last_3_percentages = new List<double>();
            for (int i = covid_percentage.Count - 3; i < covid_percentage.Count; ++i)
                last_3_percentages.Add(covid_percentage[i]);

            double avg_last3_percentage = Math.Round(last_3_percentages.Sum() / last_3_percentages.Count, 1);
            Console.WriteLine("AVG last  3 days:  " + avg_last3_percentage.ToString() + " %\n");

            #endregion Average


            //Get situation of average 10days / 5days / 3days (epi curve status)
            if ((avg_last10_percentage > avg_last5_percentage) && (avg_last5_percentage > avg_last3_percentage))
            {
                Globals.spread_phases = 'g';
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The Pandemic situation is coming to an end!"); //The percentage is decreasing between (3-5-10 days)
            }
            else if ((avg_last10_percentage < avg_last5_percentage) && (avg_last5_percentage < avg_last3_percentage))
            {
                Globals.spread_phases = 'r';
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The pandemic situation is getting bigger !!!"); //The percentage is increasing between (3-5-10 days)
            }
            else
            {
                Globals.spread_phases = 'y';
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The Pandemic situation is unstable !"); //Hights and Lows between the 3 avereges percentages (3-5-10 days)
            }
            Console.ForegroundColor = ConsoleColor.White;


            //SARS-CoV-2 Predictions
            #region Predictions

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n\n■ Next Days Predictions ■\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Tomorrow: ");
            Console.ForegroundColor = ConsoleColor.White;

            /*prediction with all medians (low accurate)
            double prediction_with_day_all = covid_stats[covid_stats.Length - 1] +
                Math.Round((covid_stats[covid_stats.Length - 1] * median_all_percentages)/100,0);
            Console.WriteLine("AVG All: " + prediction_with_day_all.ToString());*/

            //prediction with last 10 days percentage AVG
            double prediction_with_day_10 = covid_stats[covid_stats.Length - 1] +
                Math.Round((covid_stats[covid_stats.Length - 1] * avg_last10_percentage) / 100, 0);
            Console.WriteLine("AVG 10: " + prediction_with_day_10.ToString() + " infected." +
                " (+" + (prediction_with_day_10 - covid_stats[covid_stats.Length - 1]).ToString()+ ")");
            //prediction with last 5 days percentage AVG
            double prediction_with_day_5 = covid_stats[covid_stats.Length - 1] +
                Math.Round((covid_stats[covid_stats.Length - 1] * avg_last5_percentage) / 100, 0);
            Console.WriteLine("AVG  5: " + prediction_with_day_5.ToString() + " infected." +
                " (+" + (prediction_with_day_5 - covid_stats[covid_stats.Length - 1]).ToString() + ")");
            //prediction with last 3 days percentage AVG
            double prediction_with_day_3 = covid_stats[covid_stats.Length - 1] +
                Math.Round((covid_stats[covid_stats.Length - 1] * avg_last3_percentage) / 100, 0);
            Console.WriteLine("AVG  3: " + prediction_with_day_3.ToString() + " infected." +
                " (+" + (prediction_with_day_3 - covid_stats[covid_stats.Length - 1]).ToString() + ")");

            int days = 0;
            Console.WriteLine("\nPlease introduce a number of days for prediction:");
            if (!Int32.TryParse(Console.ReadLine(), out days))
            {
                Console.WriteLine("Invalid Number ! The program will show the predictions for the next 3 days.");
                days = 3;
            }

            //Trend Calculation
            double calc_trend = 0;
            if(Globals.spread_phases == 'g')
                calc_trend = (double)(avg_last3_percentage - ((avg_last5_percentage - avg_last3_percentage )/ 2.5));
            else if (Globals.spread_phases == 'r')
                calc_trend = (double)(avg_last3_percentage + ((avg_last5_percentage - avg_last3_percentage) / 2.5));
            else
                calc_trend = (double)(avg_last10_percentage + avg_last5_percentage + avg_last3_percentage) / 3;

            double prediction_per = calc_trend;
            List<int> next_days_values = new List<int>();

            string ask = string.Empty;
            Console.WriteLine("\nThe next prediction will be made with the rate of + " + Math.Round(prediction_per, 1).ToString() + " %\n");
            Console.WriteLine("Do you like use another incremental percentage rate ? y/n");
            ask = Console.ReadLine();
            if (ask == "y" || ask == "yes")
            {
                Console.Write("Please introduce new percentage %: ");
                if(!double.TryParse(Console.ReadLine(), out prediction_per))
                {
                    Console.WriteLine("Number Invalid!");
                    Environment.Exit(0);
                }
            }

            for (int i = 0; i < days; i++)
            {
                if (i > 0)
                {
                    var new_value = (next_days_values.Sum() * prediction_per) / 100;
                    next_days_values.Add(Convert.ToInt32(Math.Round(next_days_values[next_days_values.Count - 1] + new_value, 0)));
                }
                else
                {
                    var new_value = ((covid_stats[covid_stats.Length - 1] * prediction_per) / 100);
                    next_days_values.Add(covid_stats[covid_stats.Length - 1] + Convert.ToInt32(Math.Round(new_value, 0))); ;
                }
            }
           
            int days_counter = 0;
            foreach (int x in next_days_values)
            {
                days_counter++;
                if (days_counter == 1)
                    Console.WriteLine("Tomorrow: " + x.ToString() + " infected.");
                else
                {
                    Console.WriteLine(" +" + days_counter.ToString() + " days: " + x.ToString() + " infected." +
                " (+" + (x - next_days_values[days_counter-2]).ToString() + " new cases)");
                }
            }
            #endregion Predictions

            Console.ReadKey();
        }

        static void MenuLogo()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\n");
            Console.WriteLine(@"   _____           _____    _____          _____    __      __     ___ ");
            Console.WriteLine(@"  / ____|   /\    |  __ \  / ____|        / ____|   \ \    / /    |__ \     Cumulative Stats & Predictions");
            Console.WriteLine(@" | (___    /  \   | |__) || (___  ______ | |      ___\ \  / /______  ) |    V.1.0");
            Console.WriteLine(@"  \___ \  / /\ \  |  _  /  \___ \|______|| |     / _ \\ \/ /|______|/ / ");
            Console.WriteLine(@"  ____) |/ ____ \ | | \ \  ____) |       | |____| (_) |\  /        / /_ ");
            Console.WriteLine(@" |_____//_/    \_\|_|  \_\|_____/         \_____|\___/  \/        |____|    By Ascensao");
            Console.WriteLine("\n\n");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}