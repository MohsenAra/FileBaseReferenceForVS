using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ValidateProjects
{
    public class IsStartProjectTag : ITag
    {
        private readonly string line;

        public IsStartProjectTag(string line)
        {
            this.line = line;
        }

        public bool IsThisTag()
        {
            return FindStratProjectTag().Groups.Count == 6;
        }
        public Match FindStratProjectTag()
        {
            var namespaces=string.Join("|", Program.ValidNameSpace);
            string strRegex =
                @"Project\(\""{([A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})}\""\).=.\""((MITD|Haj).*)\""\,\s\""(.*)\.csproj.*{([A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})}\""";
                //TODO Load Form Config 

            Regex myRegex = new Regex(strRegex, RegexOptions.None);
            var match = myRegex.Match(line);
            return match;
        }

        public KeyValuePair<string, Guid> GetProjectInfo()
        {
            var match = FindStratProjectTag();
            return new KeyValuePair<string, Guid>(match.Groups[2].Value, new Guid(match.Groups[5].Value));
        }
    }
}