using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK.Events;
using Smart4All.Core;

namespace Smart4All
{
    class Program
    {
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            ShowDisplayMessage();

            var handle = Activator.CreateInstance(null, "Smart4All.Plugin.Champion." + Player.Instance.ChampionName);
            var pluginModel = (Brain)handle.Unwrap();
        }

        private static void ShowDisplayMessage()
        {
            var r = new Random();
            try
            {
                var sr = new System.IO.StreamReader(System.Net.WebRequest.Create(string.Format("https://raw.githubusercontent.com/mrarticuno/silver-fiesta/master/Smart4All/Smart4All/Gfx/Index.name")).GetResponse().GetResponseStream());
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
            catch { }
        }
    }
}
