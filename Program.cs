using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Translation.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace SynonymsTranslate
{
    class Program
    {


        static string host = "https://api.dictionaryapi.dev";
        static string path = "/api/v2/entries/tr/";



        //static string key = "ENTER YOUR KEY HERE";
        List<string> synonyms = new List<string>();
        //static string query = "siyah";

        static void Main(string[] args)
        {
            List<string> liste = new List<string>();
            Console.WriteLine("Dosya dizinini giriniz.");
            string fileloc = Console.ReadLine();
            List<string> syns;
            //fileloc = @"Z:\BELGELERIM\bqc1sgb\My Documents\Desktop\Dosya2.csv";

            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = false
            };

            var streamReader = File.OpenText(fileloc);
            var csvReader = new CsvReader(streamReader, csvConfig);
            
            string value;
            string[] satir = new string[6];

            TranslationClient client = TranslationClient.Create();
            TranslationResult translated;

            while (csvReader.Read())
            {
                for (int i = 0; csvReader.TryGetField<string>(i, out value); i++)
                {
                    satir[i] = value;
                    Console.Write($"{value} ");
                }

                Console.WriteLine();
                syns = GetResponseFromURI(satir[0]);
                if (syns.Count > 0) 
                { 
                    for (int j = 1; j < 5; j++)
                    {
                        if (syns.Exists(x => x == satir[j]))
                        {
                            satir[j] = null;
                        };
                    }
                }


                translated = client.TranslateText(satir[0], LanguageCodes.English);
                satir[5] = translated.TranslatedText;
                liste.Add(String.Join(";", satir.Select(p => p)));
                //
            }

            streamReader.Close();

            using (var writer = new StreamWriter(fileloc))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var item in liste)
                {
                    csv.WriteField(item);
                    csv.NextRecord();
                }
                writer.Flush();
            }
        }

        private static List<string> GetResponseFromURI(string word)
        {
            HttpClient client = new HttpClient();
            string uri = host + path + System.Net.WebUtility.UrlEncode(word);
            HttpResponseMessage response = client.GetAsync(uri).Result;
            List<string> synonyms = new List<string>();
           
            if (response.IsSuccessStatusCode)
            {
                string contentString = response.Content.ReadAsStringAsync().Result.ToString();
                dynamic parsedJson = JsonConvert.DeserializeObject(contentString);
                var defs = parsedJson[0].meanings[0].definitions;
                foreach (var syn in defs)
                {
                    if (syn.synonyms.Count != 0)
                        synonyms.Add((string)syn.synonyms[0]);
                }

                Console.WriteLine(parsedJson);

            }
                return synonyms;
        }
    }
}
