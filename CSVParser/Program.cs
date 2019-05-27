using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using System.Threading;

namespace CSVParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //Constants
            string pinName = "input";
            string clkName = "clk";
            string irName = "input";

            List<string>[] readCSV(string fileLocation)
            {
                bool isPS2 = false;

                StreamReader reader = new StreamReader(File.OpenRead(fileLocation));

                List<string> TIME1 = new List<String>();
                List<string> TIME2 = new List<String>();
                List<string> VALUE1 = new List<String>();
                List<string> VALUE2 = new List<String>();

                bool trigger = false;

                string line = reader.ReadLine();
                string[] previous = line.Split(',');
                line = reader.ReadLine();
                string[] values = line.Split(',');
                double counter = 0;

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        previous = values;
                        values = line.Split(',');

                        if (trigger && values[0] != "" && values[1] != "" && (previous[0] != "TIME"))
                        {
                            string compare1 = "";
                            string compare2 = "";
                            string compare3 = "";
                            string compare4 = "";

                            if (!isPS2)
                            {
                                compare1 = (Convert.ToDouble(previous[1]) < 3) ? "1" : "0";
                                compare2 = (Convert.ToDouble(values[1]) < 3) ? "1" : "0";
                            }
                            else
                            {
                                compare1 = (Convert.ToDouble(previous[1]) < 3.5) ? "0" : "1";
                                compare2 = (Convert.ToDouble(values[1]) < 3.5) ? "0" : "1";

                                compare3 = (Convert.ToDouble(previous[2]) < 3.5) ? "0" : "1";
                                compare4 = (Convert.ToDouble(values[2]) < 3.5) ? "0" : "1";
                            }
                            

                            if (compare1 != compare2 && !isPS2)
                            {
                                if (Convert.ToDouble(values[1]) < 3)
                                {
                                    TIME1.Add(counter.ToString());
                                    VALUE1.Add("1");
                                }
                                else
                                {
                                    TIME1.Add(counter.ToString());
                                    VALUE1.Add("0");
                                }
                            }
                            if (compare1 != compare2 && isPS2)
                            {
                                if (Convert.ToDouble(values[1]) < 3.5)
                                {
                                    TIME1.Add(counter.ToString());
                                    VALUE1.Add("0");
                                }
                                else
                                {
                                    TIME1.Add(counter.ToString());
                                    VALUE1.Add("1");
                                }
                            }
                            if (compare3 != compare4 && isPS2)
                            {
                                if (Convert.ToDouble(values[2]) < 3.5)
                                {
                                    TIME2.Add(counter.ToString());
                                    VALUE2.Add("0");
                                }
                                else
                                {
                                    TIME2.Add(counter.ToString());
                                    VALUE2.Add("1");
                                }
                            }

                            counter += 0.2;
                        }

                        if(values[0] == "TIME")
                        {
                            trigger = true;
                            isPS2 = (values[1] == "CH1") ? true : false;
                        }
                    }
                }

                List<string>[] data = new List<string>[4];
                data[0] = TIME1.ToList();
                data[1] = VALUE1.ToList();
                data[2] = TIME2.ToList();
                data[3] = VALUE2.ToList();

                return data;
            }

            void createDO(List<string>[] data, string fileName, string pinname, string clkname, string irname)
            {
                List<string> output = new List<string>();

                if(data[2].Count() == 0)
                {
                    output.Add("force " + irname + " 1 @ {100 ps}");

                    for (int i = 0; i < data[0].Count(); i++)
                    {
                        output.Add("force " + irname + " " + data[1][i] + " @ {" + Math.Round(Convert.ToDouble(data[0][i]), 1) + " us}");
                    }
                }
                else
                {
                    output.Add("force " + pinname + " 1 @ {100 ps}");

                    for (int i = 0; i < data[2].Count(); i++)
                    {
                        output.Add("force " + pinname + " " + data[3][i] + " @ {" + Math.Round(Convert.ToDouble(data[2][i]), 1) + " us}");
                    }

                    output.Add("");

                    for (int i = 0; i < data[0].Count(); i++)
                    {
                        if (i == 0)
                        {
                            output.Add("force " + clkname + " 1 @ {100 ps}");
                        }
                        output.Add("force " + clkname + " " + data[1][i] + " @ {" + Math.Round(Convert.ToDouble(data[0][i]), 1) + " us}");
                    }
                }

                output.Add("");
                output.Add("run " + Math.Round(Convert.ToDouble(data[0].Last()) + 0.2, 1) + " us");

                string outputFile = Directory.GetCurrentDirectory() + "\\" + fileName + ".do";
                File.WriteAllLines(outputFile, output, System.Text.Encoding.UTF8);
            }

            void asynchronousFileParser(string fileLocation, string pinname, string clkname, string irname)
            {
                List<string>[] data = readCSV(fileLocation);
                string fileName = fileLocation.Remove(0, Directory.GetCurrentDirectory().Length + 1);
                fileName = fileName.Substring(0, fileName.Length - 4);
                createDO(data, fileName, pinname, clkName, irname);

                Console.WriteLine("Created " + fileName + ".do successfully.");
            }

            //For Testing (Replace with directory of your file below!)
            //string test = "D:\\Programs\\CSVParser\\CSVParser\\bin\\Debug\\a.csv";
            //asynchronousFileParser(test, "irpin", "ps2pin", "ps2clk");

            Console.WriteLine("Enter paramenters for the pins names and clk names for either project...");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("Enter the name of your IR input pin:");
            irName = Console.ReadLine();
            Console.WriteLine("Enter the name of your PS/2 input pin:");
            pinName = Console.ReadLine();
            Console.WriteLine("Enter the name of your PS/2 clk pin:");
            clkName = Console.ReadLine();
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine();

            

            //If files were dragged onto me setup process
            if (args.Count() != 0)
            {
                Console.WriteLine("The files that were dropped into me are shown below");
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();

                foreach (string fileLocation in args)
                {
                    string fileName = fileLocation.Remove(0, Directory.GetCurrentDirectory().Length + 1);
                    Console.WriteLine(fileName);
                }

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();

                var tasks = new List<Task>();
                foreach (string fileLocation in args)
                {
                    Task task = Task.Factory.StartNew(() => asynchronousFileParser(fileLocation, pinName, clkName, irName));
                    tasks.Add(task);

                    //asynchronousFileParser(fileLocation, pinName);
                }
                Task.WaitAll(tasks.ToArray());
                
                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();

                Console.WriteLine("All do files were created successfully.");
                Console.WriteLine("Press enter to close this program...");
                Console.ReadLine();
            }
            else
            {

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();

                string path = Directory.GetCurrentDirectory();
                string supportedExtensions = "*.csv";

                var tasks = new List<Task>();
                foreach (string fileLocation in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())))
                {
                    Task task = Task.Factory.StartNew(() => asynchronousFileParser(fileLocation, pinName, clkName, irName));
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();

                Console.WriteLine("All do files were created successfully.");
                Console.WriteLine("Press enter to close this program...");
                Console.ReadLine();
            }
        }
    }
}
