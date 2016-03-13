using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ValidateProjects
{
    public class Program
    {
        public static List<string> ExcludeList = ConfigurationManager.AppSettings["ExcludeSearchPattern"].Split(',').ToList();
        public static List<string> ValidNameSpace = ConfigurationManager.AppSettings["ValidNameSpace"].Split(',').ToList();

        public static bool Write;
        private static void Main(string[] args)
        {
            //

            if (args.Length > 0)
                Write = true;
            var checker = new Checker();
            checker.Validate();

            WriteColor("Press any key...", ConsoleColor.Red);
            Console.ReadKey();
        }


        public static void WriteColor(string message, ConsoleColor color = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;

            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Black;

        }

    }

    public class SolutionFileUpdater
    {
        private IEnumerable<string> lines;
        private Dictionary<string, List<string>> solutionDependencies;
        public Dictionary<string, Guid> projectIds = new Dictionary<string, Guid>();

        public SolutionFileUpdater(IEnumerable<string> lines, Dictionary<string, List<string>> solutionDependencies)
        {
            this.lines = lines;
            this.solutionDependencies = solutionDependencies;
        }

        public void Load()
        {
            foreach (var line in lines)
            {
                if (line.Trim().Length == 0) continue;
                var startTag = new IsStartProjectTag(line);
                if (startTag.IsThisTag())
                {
                    var data = startTag.GetProjectInfo();
                    if (data.Value.ToString().Contains("0000"))
                    {
                    }
                    projectIds.Add(data.Key, data.Value);
                }

            }
        }

        public void ReWrite()
        {
            var startTagInfo = new KeyValuePair<string, Guid>();
            bool starting = false;
            foreach (var line in lines)
            {
               
                var result = IsKnowTag(line);

                if (result == null)
                {
                    WriteLine(line);
                    continue;
                }

                switch (result.GetType().Name)
                {
                    case "IsStartProjectTag":
                        startTagInfo = result.GetProjectInfo();
                        WriteLine(line);
                        starting = true;
                        break;
                    case "IsStartProjectSectionTag":
                        WriteDependencies(startTagInfo);
                        break;
                    case "DependenctTag":
                        if (starting == false)
                            WriteLine(line);
                        break;
                    case "IsEndProjectTag":
                        starting = false;
                        WriteLine(line);

                        break;
                    default:
                        WriteLine(line);
                        break;
                }

            }
            //  File.AppendAllText("file.txt","mohsen \n\r");

            if (Program.Write)
                File.WriteAllText(@"sl2.sln", stringBuilder.ToString());
            //  TextWriter tw = new StreamWriter("date.txt", true);
            // tw.Write(stringBuilder.ToString());
        }
        StringBuilder stringBuilder = new StringBuilder();
        private void WriteDependencies(KeyValuePair<string, Guid> startTagInfo)
        {
            WriteLine("	ProjectSection(ProjectDependencies) = postProject");
            
            var projectDependencies = solutionDependencies.FirstOrDefault(c => c.Key == startTagInfo.Key);
            if (projectDependencies.Key == null)
                return;
            foreach (var dependency in projectDependencies.Value)
            {
                var guid = projectIds.FirstOrDefault(c => c.Key == dependency);
                if (guid.Value.ToString().Contains("00000"))
                {
                    Program.WriteColor(string.Format("Invalid Refrence in {0} - {1}", startTagInfo.Key, dependency), ConsoleColor.Yellow);
                    continue;
                }
                WriteLine(string.Format(@"		{{{0}}} = {{{0}}}", guid.Value.ToString().ToUpper()));

            }
        }



        private void WriteLine(string line)
        {
            stringBuilder.Append(line);
            stringBuilder.Append(Environment.NewLine);
        }

        public ITag IsKnowTag(string line)
        {
            var types = Assembly.GetAssembly(typeof(DependenctTag))
                                .GetTypes()
                                .Where(c => c.GetInterfaces().Any(d => d.Name == typeof(ITag).Name));
            foreach (var type in types)
            {
                var tag = (ITag)Activator.CreateInstance(type, line);
                if (tag.IsThisTag())
                    return tag;
            }
            return null;
        }
    }


}
