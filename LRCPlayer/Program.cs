using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinePlayer.Modules;
using LinePlayer;
using System.IO;
using System.Text.RegularExpressions;

namespace LRCPlayer
{
    class Program
    {
        public static PlayerModule pm;
        static void Main(string[] args)
        {
            pm = new PlayerModule();
            if (args.Length <= 0)
            {
                Console.WriteLine("Usage: LRCPlayer music");
                Console.WriteLine("Fuck");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Music File not found. Fuck you.");
                return;
            }
            Regex rgx = new Regex(@"\.[a-zA-Z0-9_]+$");
            var lrcPath = rgx.Replace(args[0], ".lrc");
            if (!File.Exists(lrcPath))
            {
                Console.WriteLine("LRC not found");
            }
            else
            {
                pm.Open(args[0]);
                var _lyrics = LRCParser.ParseLyricInstant(lrcPath);
                int pos = 0;
                pm.Play();
                while (pos < _lyrics.Count)
                {
                    if(pm.Position > _lyrics[pos].Time)
                    {
                        Console.WriteLine(_lyrics[pos].Time.ToString(@"mm\:ss\,ff") + " " + _lyrics[pos].ToString());
                        pos++;
                    }
                }
            }

        }
    }
}
