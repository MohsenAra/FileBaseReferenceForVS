using System;
using System.Collections.Generic;

namespace ValidateProjects
{
    public class IsEndProjectTag : ITag
    {
        private readonly string line;

        public IsEndProjectTag(string line)
        {
            this.line = line;
        }

        public bool IsThisTag()
        {
            return line.Trim() == "EndProject";
        }

        public KeyValuePair<string, Guid> GetProjectInfo()
        {
            throw new NotImplementedException();
        }
    }
}