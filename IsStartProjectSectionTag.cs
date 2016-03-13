using System;
using System.Collections.Generic;

namespace ValidateProjects
{
    public class IsStartProjectSectionTag : ITag
    {
        private string line;

        public bool IsThisTag()
        {
            return line.Trim() == "ProjectSection(ProjectDependencies) = postProject";
        }
        public IsStartProjectSectionTag(string line)
        {
            this.line = line;
        }

        public KeyValuePair<string, Guid> GetProjectInfo()
        {
            throw new NotImplementedException();
        }
    }
}