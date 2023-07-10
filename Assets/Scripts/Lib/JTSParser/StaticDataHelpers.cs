namespace YYZ.JTS
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Text.RegularExpressions;

    public class ParameterData
    {
        public Dictionary<string, Dictionary<string, string>> Data;

        public static ParameterData Parse(string parameterText)
        {
            var rd = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<string, string> current = null; // new Dictionary<string, string>();

            foreach(var line in parameterText.Split("\n"))
            {
                if(line.Length > 0)
                {
                    var h = line[0];
                    if(('a' <= h && h <= 'z') || ('A' <= h && h <= 'Z'))
                    {
                        rd[line.Trim()] = current = new Dictionary<string, string>();
                    }
                    else
                    {
                        // '\t' or '  ' breaks the word
                        foreach(var word in line.Split(new string[]{"\t", "  "}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var pair = word.Split(": ");
                            if(pair.Length == 2) // skip "\r"
                                current[pair[0]] = pair[1];
                        }
                    }
                }
            }
            
            return new ParameterData(){Data=rd};
        }

        public override string ToString()
        {
            var s = string.Join(", ", Data.Select(KV => $"{KV.Key}: [{KV.Value.Count}]"));
            return $"ParameterData({s})";
        }
    }

    public static class StaticDataHelper
    {
    }
}