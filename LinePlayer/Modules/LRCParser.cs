using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinePlayer.Modules
{
    /// <summary>
    /// A LRC file format parser, perhaps a dispatch scheduler?
    /// </summary>
    public class LRCParser
    {
        /// <summary>
        /// Basic format for each line of parsed lyric.
        /// </summary>
        public struct LyricLine
        {
            TimeSpan _time;
            string _lyric;

            public TimeSpan Time { get => _time; set => _time = value; }
            public string Lyric { get => _lyric; set => _lyric = value; }

            public override string ToString() => Lyric;
            public string ToString(string timespanFormat) => Time.ToString(timespanFormat) + " " + Lyric;
        }

        /// <summary>
        /// A static LRC parser function.
        /// </summary>
        /// <param name="LRCFilePath">The path to the LRC file</param>
        /// <returns>A list of parsed LyricLine format. Empty list if error occurs.</returns>
        public static List<LyricLine> ParseLyricInstant(string LRCFilePath)
        {
            var lyrics = new List<LyricLine>();
            if (!File.Exists(LRCFilePath))
            {
                return lyrics;
            }
            var reg_timeStamp = new Regex(@"\[\d*\:\d*\.\d*\]");

            string[] fileData = File.ReadAllLines(LRCFilePath);
            foreach (var fileLine in fileData)
            {
                var matches = reg_timeStamp.Matches(fileLine);
                var nowLyric = reg_timeStamp.Replace(fileLine, "");
                foreach (Match m in matches)
                {
                    var timeTag = m.Value.Trim("[]".ToCharArray());
                    bool ThisOK = true;
                    TimeSpan nowTime = TimeSpan.Zero;
                    //Console.WriteLine($"{m.Index}: {m.Length}, {m.Value}");
                    if (TimeSpan.TryParseExact(timeTag, @"mm\:ss", null, out TimeSpan result))
                        nowTime = result;
                    else if (TimeSpan.TryParseExact(timeTag, @"mm\:ss\.f", null, out TimeSpan result1))
                        nowTime = result1;
                    else if (TimeSpan.TryParseExact(timeTag, @"mm\:ss\.ff", null, out TimeSpan result2))
                        nowTime = result2;
                    else if (TimeSpan.TryParseExact(timeTag, @"mm\:ss\.fff", null, out TimeSpan result3))
                        nowTime = result3;
                    else if (TimeSpan.TryParseExact(timeTag, @"mm\:ss\.ffff", null, out TimeSpan result4))
                        nowTime = result4;
                    else if (TimeSpan.TryParseExact(timeTag, @"mm\:ss\.fffff", null, out TimeSpan result5))
                        nowTime = result5;
                    else
                    {
                        ThisOK = false;
                        throw new Exception("Bad time tag format in LRC file.");
                    }
                    if (ThisOK)
                    {
                        lyrics.Add(new LyricLine { Time = nowTime, Lyric = nowLyric });
                    }
                }
            }
            return lyrics.OrderBy(x => x.Time).ToList();
        }


        private List<LyricLine> _lyrics;
        private string _LrcPath;
        public string Path { get => _LrcPath; set => _LrcPath = value; }

        /// <summary>
        /// Constructor that init lyric file.
        /// </summary>
        public LRCParser()
        {
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LRCFilePath">The path to the LRC file</param>
        public LRCParser(string LRCFilePath)
        {
            Init();
            Path = LRCFilePath;
            Parse();
        }

        private void Init()
        {
            _lyrics = new List<LyricLine>();
            Path = "";
        }

        private void Parse()
        {

        }
        
    }
}
