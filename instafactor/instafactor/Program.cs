using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.SqlServer.Server;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Net.Http;
using Cookie = System.Net.Cookie;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using OpenPop.Pop3;

namespace instafactor
{
    class Program
    {
        //public static List<string> biolist = FileReader(@".\biolist.txt");
        public static List<string> Namelist = FileReader(@".\Name.txt");
        public static List<string> SurNamelist = FileReader(@".\surname.txt");
        public static List<string> months = new List<string>() { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

        static void Main(string[] args)
        {
            for (int i = 9000; i < 9100; i++) //Size verilen port sayısı kadar döndürüyorsunuz, her proxy adresi bir kere hesap açabiliyor çünkü. Spagetti yazdığım için kafa karıştırır burası.
                                              //Eğer chrome eklentisiyle açtırıyorsanız hesapları açmak istediğiniz hesap sayısı kadar ayarlayın döngüyü. Aşağıdaki proxy sıfırlama linklerini de enable edin
            {
               
                Console.WriteLine(i.ToString());
                SeleniumRun(i.ToString());

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

        private static string RandomUser()
        {
            Random rnd = new Random();
            int NameID = rnd.Next(Namelist.Count);
            int SurNameID = rnd.Next(SurNamelist.Count);
            int number = rnd.Next(99);
            string username = UTF8Replacer(Namelist[NameID].ToLower()) + UTF8Replacer(SurNamelist[SurNameID]).ToLower() + number;
            string Name = Namelist[NameID];
            string SurName = SurNamelist[SurNameID];
            string returner = string.Format("{0},{1},{2}", Name.ToLower(), SurName, username);


            return returner;
        }

        private static string UTF8Replacer(string text)
        {
            //Maili oluştururken Türkçe karakterden patlamasın diye onları editleyen bir alet. Google'da buldum kendim yazmadım.
            text = text.Replace("İ", "I");
            text = text.Replace("ı", "i");
            text = text.Replace("Ğ", "G");
            text = text.Replace("ğ", "g");
            text = text.Replace("Ö", "O");
            text = text.Replace("ö", "o");
            text = text.Replace("Ü", "U");
            text = text.Replace("ü", "u");
            text = text.Replace("Ş", "S");
            text = text.Replace("ş", "s");
            text = text.Replace("Ç", "C");
            text = text.Replace("ç", "c");
            text = text.Replace(" ", "_");
            return text;
        }

        private static void SeleniumRun(string port)
        {
            int success = 1;
        again:
            Console.WriteLine("Generating User...");
            string randomized = RandomUser();
            string email = randomized.Split(',')[2] + "@domainname.com"; //Burayı kendi mailinize göre editleyin
            string fullname = string.Format("{0} {1}", randomized.Split(',')[0], randomized.Split(',')[1]);
            string username = randomized.Split(',')[2];
            string password = "agıbuk.123";// oluşacak hesaplar için Parola
            Console.WriteLine("Trying generate mail adress..."); //CyberPanele gidip yeni mail oluşturma
            if (!MailGeneratorAsync(username).Result) { goto again; } //Eğer burası loop'a takılırsa mail üreten zımbırtının cookie bilgisi yanlıştır, aşağıda uzun uzun açıkladım.
            Console.WriteLine("User Generated Successfully!");
            Console.WriteLine(string.Format("{0}", username));
            Console.WriteLine("Running Webdriver!");

            ChromeOptions cap = new ChromeOptions();
            string proxycreds = "HTTP/S Proxy için https://kullanıcıadı:şifre@ipadresi" + port; //
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = proxycreds;
            proxy.SslProxy = proxycreds;
            //var proxy = "proxy.dat"; //Burda da aynı şekil proxy eklentisini de kullandım, normal chrome'un kendi özelliği bir ara çalışmadı, saçmaladı. Sonra düzeldi ama. Kod gayet basit zaten bakınca ne yapıldığı belli yani
            cap.Proxy = proxy;


            cap.EnableMobileEmulation("iPhone 4");
            //cap.AddExtension(proxy);
            //cap.AddArgument("--user-agent=Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 Version/7.0 Mobile/11D257 Safari/9537.5");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(@"C:\Users\MWR\source\repos\instafactor\instafactor\bin\Debug");
            ChromeDriver drive = new ChromeDriver(service, cap);

            //Proxy ayarları falan filan hallolduktan sonra işleme başlıyor

            Random rnd1 = new Random();
            WebDriverWait wait = new WebDriverWait(drive, TimeSpan.FromSeconds(15));
            drive.Url = "https://instagram.com";
           
            drive.Navigate();

    
            Thread.Sleep(1000);
            wait.Until(ExpectedConditions.ElementExists(By.XPath("/html/body/div[1]/section/main/article/div/div/div/div[4]/button"))); //bunu Burda denedim bunu çalıştı mı çalışmadı mı hatırlamıyorum
            drive.FindElementByXPath("/html/body/div[1]/section/main/article/div/div/div/div[6]/button").Click();
            Thread.Sleep(1000);

            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/span[2]").Click();
            Thread.Sleep(1000);

            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[2]/div[3]/div/label/input").Click();

            foreach (char item in email)
            {
                drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[2]/div[3]/div/label/input").SendKeys(item.ToString());
                Thread.Sleep(rnd1.Next(100, 250));
            }
            Thread.Sleep(500);

            
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[3]/button").Click();

            Console.WriteLine("verifying email");

            string verify = verificode(email);
            wait.Until(ExpectedConditions.ElementExists(By.XPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input"))); 
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input").Click();
            Thread.Sleep(1000);
            // Onay kodunu girişi
            foreach (char item in verify)
            {
                drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input").SendKeys(item.ToString());
                Thread.Sleep(rnd1.Next(200, 350));
            }
            Thread.Sleep(1000);

            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[2]/button").Click();

            //isim soyisim alanı
            wait.Until(ExpectedConditions.ElementExists(By.XPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input")));
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input").Click();
            Thread.Sleep(rnd1.Next(400, 600));

            foreach (var item in fullname)
            {
                drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[3]/div/label/input").SendKeys(item.ToString());
                Thread.Sleep(rnd1.Next(100, 250));
            }
            Thread.Sleep(1500);

            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/label/input").Click();
            Thread.Sleep(rnd1.Next(400, 600));
            //parola belirleme
            foreach (var item in password)
            {
                drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/label/input").SendKeys(item.ToString());
                Thread.Sleep(rnd1.Next(100, 250));
            }

            Thread.Sleep(500);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[2]/button").Click();
            Thread.Sleep(1000);

            //Doğum Tarihi seçimi başlangıcı
            Thread.Sleep(500);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[1]/select").Click();
            Thread.Sleep(500);
            var selectmonth = new SelectElement(drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[1]/select"));
            selectmonth.SelectByValue(rnd1.Next(1, 9).ToString());
            Thread.Sleep(500);


            Thread.Sleep(500);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[2]/select").Click();
            Thread.Sleep(500);
            var selectday = new SelectElement(drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[2]/select"));
            selectday.SelectByValue(rnd1.Next(1, 9).ToString());
            Thread.Sleep(500);


            Thread.Sleep(500);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[3]/select").Click();
            Thread.Sleep(500);
            var selectYear = new SelectElement(drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div[4]/div/div/span/span[3]/select"));
            selectYear.SelectByValue(rnd1.Next(1990, 2000).ToString());
            Thread.Sleep(700);

            Thread.Sleep(1000);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[2]/button").Click();

            //Doğum tarihi seçim bitiş
            Thread.Sleep(4000);

            //username değişikliği
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/div/button").Click();
            Thread.Sleep(1000);
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/form/div/div/div/label/input").Clear();

            //kullanıcı adına uniq hale getirmek için ufak bir ekleme yapma,
            Thread.Sleep(1000);
            string userkey = rnd1.Next(1, 9).ToString();
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/form/div/div/div/label/input").Click();
            drive.FindElementByXPath("/html/body/div[1]/section/main/div[2]/div/div[1]/form/div/div/div/label/input").SendKeys(userkey);
            username = username + userkey;
            Thread.Sleep(rnd1.Next(100, 250));

            Thread.Sleep(5000);

            drive.FindElementByCssSelector("#react-root > section > main > div.smPOl > div > div._4DmyP > button").Click();
            Thread.Sleep(6000);

            //Bozuk Proxy için try/catch burdan atılacak

            try
            {
                drive.FindElementByCssSelector("#react-root > section > main > div > div > div:nth-child(1) > button").Click();
                Thread.Sleep(4000);
                drive.FindElementByCssSelector("#react-root > section > main > div > div > div:nth-child(2) > section > button").Click(); // burda patlıyo
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("#react-root > section > main > div > div > div:nth-child(3) > form > section > div > div:nth-child(5) > button").Click();
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("body > div.RnEpo.Yx5HN > div > div > div > div.mt3GC > button.aOOlW.HoLwm").Click();
                Thread.Sleep(1000);

                drive.FindElementByCssSelector("#react-root > section > nav.NXc7H.f11OC > div > div > div.KGiwt > div > div > div:nth-child(5) > a").Click();
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("#react-root > section > main > div > header > section > div.Y2E37 > div > a").Click();
                Thread.Sleep(1000);

                drive.FindElementByCssSelector("#react-root > section > main > div > article > div > div.XX1Wc > div > form > input").SendKeys("C:\\Users\\MWR\\Desktop\\deneme.jpg");
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("#react-root > section > div.Scmby > header > div > div.mXkkY.KDuQp > button").Click();
                Thread.Sleep(1000);
                try
                {
                    drive.FindElementByCssSelector("body > div:nth-child(21) > div > div > div > div.mt3GC > button.aOOlW.bIiDR").Click();

                }
                catch (Exception)
                {


                }
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("#react-root > section > nav.NXc7H.f11OC > div > div > div.KGiwt > div > div > div:nth-child(5) > a").Click();
                Thread.Sleep(1000);
                drive.FindElementByCssSelector("#react-root > section > nav.gW4DF > div > header > div > div.mXkkY.HOQT4 > button > svg").Click();
                Thread.Sleep(500);
                drive.FindElementByCssSelector("#react-root > section > nav.gW4DF > div > section > div._7XkEo > div > div:nth-child(1) > div:nth-child(5) > div > a > div.xIOKA").Click();
                Thread.Sleep(500);
                drive.FindElementByCssSelector("#accountPrivacy > label > div").Click();
                Thread.Sleep(1000);

                Thread.Sleep(1000);
                drive.Quit();
                Console.WriteLine("all succesful");
                success += 1; //Bu sanırım kaç tane user ürettiğini sayıyor
                File.AppendAllText(@".\Usernames.txt", string.Format("{0}:|:{1}:|:{2}", username, password, email));//Userı text dosyasına kaydetme
                //WebRequest request = WebRequest.Create("api adresi"); // proxy sıfırlama
                //WebResponse response = request.GetResponse();
                Thread.Sleep(5000);
            }
            catch (Exception)
            {
                
                Console.WriteLine("User üretilemedi");
                drive.Quit();
                //WebRequest request = WebRequest.Create("Api adresi");// Her ihtimale karşı proxy'yi sıfırlıyor
                //WebResponse response = request.GetResponse();
                Thread.Sleep(5000);
            }

            Console.WriteLine(success.ToString() + " Account Successfully generated");
        }

        //Normalde ürettiği hesapları web panele kolay aktarmak için buraya bi Sql bağlantısı falan ayarlamıştım ama gerek kalmadı sonra

        //private static void SqlCon(string username, string password, string email)
        //{

        //    SQLiteConnection con = new SQLiteConnection("Data Source=Users.db;Version=3;");
        //    SQLiteCommand cmd = new SQLiteCommand(con);
        //    SQLiteTransaction transaction = null;
        //    transaction = con.BeginTransaction();
        //    if (!File.Exists("users.db"))
        //    {
        //        SQLiteConnection.CreateFile("users.db");

        //        cmd.CommandText = @"Create table if not exists Users (
        //                            UserID INTEGER PRIMARY KEY AUTOINCREMENT, 
        //                            username varchar(55), 
        //                            password varchar(55),
        //                            email varchar(55))";
        //        cmd.ExecuteNonQuery();

        //    }

        //    cmd.CommandText = "insert into Users (username, password, email) Values (@username, @password, @email)";
        //    cmd.Prepare();
        //    cmd.Parameters.AddWithValue("username", username);
        //    cmd.Parameters.AddWithValue("password", password);
        //    cmd.Parameters.AddWithValue("email", email);
        //    cmd.ExecuteNonQuery();
        //    transaction.Commit();

        //    con.Close();


        //}


        private static async Task<bool> MailGeneratorAsync(string username)
        {

            /* Burada saçma sapan şekilde benim CyberPanel apim çalışmadığı için normal oturumun session bilgilerini alıp programa veriyordum, serverdan ayarladığım için o session çok uzun süre patlamıyordu
             * ama böyle çok iş yapmaz düzenlemek gerekir. Bunu oturdun okuyorsan otur düzenle her şeyi devletten bekleme ben artık kullanmıyorum diye koydum github'a amme hizmeti.
             
             
             
             
             
             */

            //Buralar CyberPanele bağlanıp emaili oluştururken atılan requestleri copy paste yaptığım yerler. Gidiyor mail oluşturuyor. Bu kadar
            List<string> sesion = SessionRead();


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => { return true; };

            var baseAddress = new Uri("https://CyberPanel/");
            CookieContainer contain = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler() { CookieContainer = contain };
            var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };



            var content = new StringContent("{\"domain\":\"maildomaini\",\"username\":\"" + username + "\",\"passwordByPass\":\"ParolaParola\"}", Encoding.UTF8, "application/json"); //Format string kullanmamışım anlamsız şekile. Normalde kullanmayanı aşağılıyorum

            contain.Add(baseAddress, new Cookie("csrftoken", sesion[0].Replace(" ", null)));
            contain.Add(baseAddress, new Cookie("django_language", "en"));
            contain.Add(baseAddress, new Cookie("sessionid", sesion[1].Replace(" ", null)));


            //httpClient.DefaultRequestHeaders.Add("X-CSRFToken", sesion[0].Replace(" ", null));
            httpClient.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.8,en-US;q=0.5,en;q=0.3");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://CyberPanel/email/createEmailAccount");
            httpClient.DefaultRequestHeaders.Add("X-CSRFToken", sesion[0].Replace(" ", null));
            httpClient.DefaultRequestHeaders.Add("Origin", "https://CyberPanel");
            httpClient.DefaultRequestHeaders.Add("Connection", "close");


            HttpResponseMessage result = await httpClient.PostAsync("/email/submitEmailCreation", content);

            string respdata = await result.Content.ReadAsStringAsync();

            

            if (respdata.Contains("\"error_message\": \"None\""))
            {
                return true;

            }
            else
            {
                Console.WriteLine("Mail Server Response : " + respdata);
                Console.WriteLine("Make sure your sessionID is correct");
                return false;
            }
        }

        public static List<string> SessionRead()
        {
            List<string> sesid = System.IO.File.ReadAllLines("./session.txt").ToList();
            return sesid;
        }


        public static string verificode(string email)
        {

            //Burası pop3 bağlantısıyla mail server'a bağlanıp oluşturduğu mailin gelen kutusunu sürekli kontrol ediyor. onay kodu gelene kadar dönüyor. Program kendi oluşturduğu maile bağlandığı için doğal olarak sıfır'dan büyük olmasına bakıyor kutudaki mesaj sayısının.
            //Hazır mail listesi verirseniz patlarsınız yani
            string password = "aaaaaaaa";
            Pop3Client client = new Pop3Client();

            Console.WriteLine("Connecting Server...");


            Console.WriteLine("authencated");

        again:

            try
            {


                while (true)
                {
                    Console.WriteLine("Getting instagram code");
                    client.Connect("mailserver", 995, true);
                    Console.WriteLine("Connected, authencating");
                    client.Authenticate(email, password);
                    Thread.Sleep(6000);
                    int messageCount = client.GetMessageCount();
                    Console.WriteLine("waiting for code, messagecount =" + messageCount.ToString());
                    Console.WriteLine("messagecount =" + messageCount.ToString());

                    if (messageCount > 0)
                    {
                        for (int i = messageCount; i > 0; i--)
                        {

                            string msdId = client.GetMessage(i).Headers.MessageId;
                            OpenPop.Mime.Message msg = client.GetMessage(i);
                            //OpenPop.Mime.MessagePart plainTextPart = msg.FindFirstPlainTextVersion();
                            string message = Encoding.UTF8.GetString(msg.MessagePart.Body, 0, msg.MessagePart.Body.Length);
                            string onaykodu = getBetween(message, "text-align:center;padding-bottom:25px;", "</td></td></tr></table></td>");
                            Console.WriteLine("We get verify code successful");
                            return onaykodu;

                        }
                    }

                    client.Disconnect();
                }


            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("null ref ex");
                System.Threading.Thread.Sleep(1000);
                goto again;

            }
            catch (Exception)
            {
                Console.WriteLine("exception, waiting for code");
                System.Threading.Thread.Sleep(1000);
                goto again;
            }


        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length + 2;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

    }
}