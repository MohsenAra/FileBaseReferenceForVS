using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ValidateProjects
{
    public class DependenctTag : ITag
    {
        private readonly string line;

        public DependenctTag(string line)
        {
            this.line = line;
        }

        public bool IsThisTag()
        {
            return FindDependencyTag().Groups.Count > 2;
        }

        public KeyValuePair<string, Guid> GetProjectInfo()
        {
            throw new NotImplementedException();
        }

        private Match FindDependencyTag()
        {
            string strRegex = @"\t\t{([A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}).*{([A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12})}";
            Regex myRegex = new Regex(strRegex, RegexOptions.None);
            var match = myRegex.Match(line);
            return match;
        }
    }
}