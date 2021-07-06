using System;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace StockQuoteAlert
{
    class Program
    {
        private static string strFrom = "";
        private static string strPassword = "";
        private static string strTo = "";
        private static string strServerSMTP = "smtp.gmail.com";
        private static int nServerPort = 587;
        private static string strConfigFileName = "Configuration.txt";
        private static string strSymbol = "PETR4.SA";
        private static double sSalePrice = 0;
        private static double sBuyPrice = 0;
        private static long nLastTimestamp = 0;
        private static bool bCanSendEmailToSale = true;
        private static bool bCanSendEmailToBuy = true;
        private static Timer aTimer;
        private static string QUERY_URL;

        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                strSymbol = args[0];
                sSalePrice = Convert.ToDouble(args[1]);
                sBuyPrice = Convert.ToDouble(args[2]);
                ReadConfigurationFromFile(AppDomain.CurrentDomain.BaseDirectory);
                SetTimer();

                Console.WriteLine("\nPress the Enter key to exit the application...\n");
                Console.ReadLine();
                aTimer.Stop();
                aTimer.Dispose();

                Console.WriteLine("Terminating the application...");
            }
            else
            {
                Console.WriteLine("Invalid arguments!");
                ReadConfigurationFromFile(System.AppDomain.CurrentDomain.BaseDirectory.ToString());
                SetTimer();

                Console.WriteLine("\nPress the Enter key to exit the application...\n");
                Console.ReadLine();
                aTimer.Stop();
                aTimer.Dispose();

                Console.WriteLine("Terminating the application...");
            }
        }
        static bool SendEmail(string strTitle, string strBody)
        {
            bool result = true;
            try
            {
                MailMessage msg = new MailMessage(strFrom, strTo);

                msg.Subject = strTitle;
                msg.Body = strBody;

                msg.BodyEncoding = Encoding.UTF8;
                msg.SubjectEncoding = Encoding.UTF8;

                SmtpClient client = new SmtpClient(strServerSMTP, nServerPort);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(strFrom, strPassword);
                client.EnableSsl = true;

                client.Send(msg);
            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine("SendEmail : Error : " + e.Message);
            }
            return result;
        }
        static bool ReadConfigurationFromFile(string path)
        {
            QUERY_URL = "https://query1.finance.yahoo.com/v8/finance/chart/" + strSymbol + "?region=BR&lang=pt-BR&includePrePost=false&interval=1m&useYfid=true&range=1d&corsDomain=finance.yahoo.com&.tsrc=finance";
            string strTarget = path + @"\config\";
            if (!Directory.Exists(strTarget))
                Directory.CreateDirectory(strTarget);

            if (File.Exists(strTarget + strConfigFileName))
            {
                string[] lines = File.ReadAllLines(strTarget + strConfigFileName);

                foreach (string line in lines)
                {
                    string[] arrConfig = line.Split("=");
                    try
                    {
                        switch (arrConfig[0].Trim())
                        {
                            case "from":
                                strFrom = arrConfig[1].Trim();
                                break;
                            case "password":
                                strPassword = arrConfig[1].Trim();
                                break;
                            case "to":
                                strTo = arrConfig[1].Trim();
                                break;
                            case "ServerSMTP":
                                strServerSMTP = arrConfig[1].Trim();
                                break;
                            case "Port":
                                nServerPort = Convert.ToInt32(arrConfig[1].Trim());
                                break;
                            default:
                                Console.WriteLine("ReadConfigurationFromFile : Unknown field");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ReadConfigurationFromFile : Error processing field : " + e.Message);
                    }
                }
                return true;
            }
            else
            {
                Console.WriteLine("Configuration file " + strTarget + strConfigFileName + " does not exist!");
                return false;
            }
        }
        static void GetStockQuote()
        {
            Console.WriteLine("GetStockQuote");
            try
            {
                Uri queryUri = new Uri(QUERY_URL);

                using (WebClient client = new WebClient())
                {
                    var response = client.DownloadString(queryUri);
                    try
                    {
                        dynamic json_data = JsonConvert.DeserializeObject(response);
                        var sClosePrice = json_data.chart.result[0].meta.regularMarketPrice;
                        var nTimestamp = (long)json_data.chart.result[0].meta.regularMarketTime;
                        Console.WriteLine("Close price : " + Convert.ToString(sClosePrice));
                        if (nLastTimestamp != nTimestamp)
                        {
                            nLastTimestamp = nTimestamp;
                            if (sClosePrice >= sSalePrice)
                            {
                                bCanSendEmailToBuy = true;
                                if (bCanSendEmailToSale)
                                {
                                    if (SendEmail("Stock quote alert", "Alert for sale of stock. The stock price is " + Convert.ToString(sClosePrice) + "."))
                                        bCanSendEmailToSale = false;
                                }
                            }
                            else if (sClosePrice <= sBuyPrice)
                            {
                                bCanSendEmailToSale = true;
                                if (bCanSendEmailToBuy)
                                {
                                    if (SendEmail("Stock quote alert", "Alert for buy of stock. The stock price is " + Convert.ToString(sClosePrice) + "."))
                                        bCanSendEmailToBuy = false;
                                }
                            }
                            else
                            {
                                bCanSendEmailToSale = true;
                                bCanSendEmailToBuy = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("GetStockQuote : Error1 :  " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetStockQuote : Error2 : " + e.Message);
            }
        }

        private static void SetTimer()
        {
            aTimer = new Timer(10000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            GetStockQuote();
        }
    }
}