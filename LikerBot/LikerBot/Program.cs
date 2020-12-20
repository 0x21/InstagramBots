using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Net;


namespace LikerBot
{
    class Program
    {
        public static List<string> Namelist = FileReader(@".\userlist.txt");//Userların olduğu liste. Format olarak Username|password şeklinde alıyor
        static void Main(string[] args)
        {
            string link = "https://www.instagram.com/p/aaaaaaaaaa";

            for (int i = 0; i < Namelist.Count; i++)
            {
                seleniumrun(Namelist[i].Split('|')[0], Namelist[i].Split('|')[1], link, !link.Contains("/p/"));
            }
        }

        private static List<string> FileReader(string filename)
        {
            List<string> Liste = new List<string>();
            var lines = File.ReadLines(filename);
            foreach (var item in lines)
            {
                Liste.Add(item);
            }

            return Liste;
        }

        private static void seleniumrun(string username, string password, string profile, bool isfollow )
        {
            ChromeOptions cap = new ChromeOptions();
            cap.EnableMobileEmulation("Nexus 5");
            cap.AddExtension("proxy.dat");//Chromedriver'ın proxy ayarları saçmaladığı için chrome'a ufak bir proxy eklentisi kotarladım, stackoverflowdan çalıntıydı sanırım.
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(@"C:\Users\MWR\source\repos\instafactor\instafactor\bin\Debug");
            ChromeDriver drive = new ChromeDriver(service, cap);
            Random rnd1 = new Random();
            drive.Url = "https://www.instagram.com/";            

            drive.Navigate();
            sleeper(); // Bu sleepler genelde proxy'nin yavaşlığından kaynaklı olarak program patlamasın diye. Await çekip beklete de biliyorsunuz aslında o element kullanılabilir olana kadar
                       //öyle bir özelliği var Seleniumun ama ben kullanmaya/okumaya üşendiğim için sleep atıp geçmiştim 
            drive.FindElementByCssSelector("#react-root > section > main > article > div > div > div > div:nth-child(2) > button").Click();
            sleeper();
        
            try //Bu try catch ne işe yarıyordu en ufak bir fikrim yok ama bug'a sebep oluyor, eğer program burada catch'e düşerse yeniden başlıyor ard arda 
            {

                // Burası login olma aşaması
                foreach (char item in username)
                {
                    drive.FindElementByCssSelector("#loginForm > div.Igw0E.IwRSH.eGOV_._4EzTm.kEKum > div:nth-child(3) > div > label > input").SendKeys(item.ToString());
                    Thread.Sleep(rnd1.Next(300, 600));
                }

                sleeper();
                foreach (char item in password)
                {
                    drive.FindElementByCssSelector("#loginForm > div.Igw0E.IwRSH.eGOV_._4EzTm.kEKum > div:nth-child(4) > div > label > input").SendKeys(item.ToString());
                    Thread.Sleep(rnd1.Next(300, 600));
                }
                sleeper(true);
                drive.FindElementByXPath("/html/body/div[1]/section/main/article/div/div/div/form/div[1]/div[6]/button/div").Click();


            try2: //Bazen proxy'nin yavaşlığından kaynaklı patlıyordu. Bi try catch attım bağlanana kadar tekrar tekrar deniyor olayı bu
                try
                {
                    sleeper();

                    drive.Url = profile;
                    drive.Navigate();
                }
                catch (Exception)
                {

                    goto try2;
                }
            try3: //burası da try2 ile aynı sebepten
                try
                {
                    /*Burası bütün scriptin amacı zaten, takipse takip beğeniyse beğeni atıyor Bazı yerlerde Selector bazı yerlerde Xpath kullanmamın sebebi deneyip stabil olanı bulmaya çalışmak*/
                    if (isfollow)
                    {
                        sleeper();
                        drive.FindElementByCssSelector("#react-root > section > main > div > header > section > div.Y2E37 > div > div > div > button").Click();
                    }
                    else
                    {
                        drive.FindElementByXPath("/html/body/div[1]/section/main/div/div/article/div[3]/section[1]/span[1]/button").Click();
                        sleeper();
                    }
                }
                catch (Exception)
                {
                    goto try3;
                    
                }
                sleeper(true);

                

                drive.Quit();
                WebRequest request = WebRequest.Create("ProxyIP"); //Bizim kullandığımız proxyde bize verilen api linkine sadece req. atarak ip adresini değiştiriyorduk. Burası onu sağlıyor
                WebResponse response = request.GetResponse();

            }
            catch (Exception)
            {
            }
        }

        public static void sleeper(bool isdebug = false)
        {
            if (isdebug)
            {
                Thread.Sleep(1000); /*Burada "isdebug" meselesinin olayı çok anlamsız aslında, sürekli oraya buraya BP atıp bunları kaldırmakla uğraşmak yerine zaten sürekli kullandığım sleep fonksiyonunu editledim,
                                     * bu satıra BP attığınız zaman ilgili yerdeki sleep'i true yapmanız yeterli oluyor. Neden böyle bişey yaptım bilmiyorum anlamsız aslında */
            }
            else
            {
                Thread.Sleep(5000);
            }
        }
    }
}
