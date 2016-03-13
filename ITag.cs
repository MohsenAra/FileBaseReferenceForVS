using System;
using System.Collections.Generic;

namespace ValidateProjects
{
    public interface ITag
    {
        bool IsThisTag();
        KeyValuePair<string, Guid> GetProjectInfo();
    }

    public class IsEndProjectSectionTag : ITag
    {
        private string line;

        public IsEndProjectSectionTag(string line)
        {
            this.line = line;
        }

        public KeyValuePair<string, Guid> GetProjectInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsThisTag()
        {
            return line.Trim() == "EndProjectSection";

        }
    }
}