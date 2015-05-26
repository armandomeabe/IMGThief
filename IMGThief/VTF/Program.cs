using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMGThiefCore;

namespace VTF
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = "http://www.ventafe.com.ar"; // /rafaela-santa-fe/autos/passat/388232";

            var visitar = new List<string>();
            var visitado = new List<string>();
            var escanear = new List<string>();
            var rand = new Random(DateTime.Now.Millisecond);
            var db = new EmailSeekerEntities();
            var core = new Core();

            visitar.Add(source);

            while (visitar.Any())
            {
                Console.WriteLine("Visitar: " + visitar.Count);
                Console.WriteLine("Visitado: " + visitado.Count);
                //Console.WriteLine("Mails encontrados: " + mailconunter);

                var actual = visitar.First();
                visitar.Remove(visitar.First());
                visitado.Add(actual);
                actual = actual == source ? "/" : actual;

                Console.WriteLine("Visitando: " + actual);
                Console.WriteLine("##############################################");

                var emails = core.FetchEmailsFromText(source + "/web/" + (actual == "/" ? actual : "/"));
                foreach (var email in emails)
                {
                    Console.WriteLine("EMAIL: " + email);
                    if (!db.Emails.Any(x => x.email1.Equals(email)))
                    {
                        var newEmail = new Email()
                        {
                            email1 = email,
                            source = source + "/web/" + (actual == "/" ? actual : "/"),
                            sitioId = 2
                        };
                        db.Emails.Add(newEmail);
                        db.SaveChanges();
                    }
                }
                // Acá debería sacar los emails de cada pagina

                var nuevasUrls = core.FetchUrls(source + "/web/" + (actual == "/" ? actual : "/")).Where(url => url.EndsWith("html"));//.Where(url => url.StartsWith("/") && url != "/").Distinct();
                foreach (var url in nuevasUrls)
                {
                    if (!visitado.Contains(url) && !visitar.Contains(url))
                    {
                        visitar.Add(url);
                    }
                }
            }
        }
    }
}
