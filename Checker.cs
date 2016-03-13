using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ValidateProjects
{
    public class Checker
    {
        
        public void Validate()
        {
            var solutionPath = ConfigurationManager.AppSettings["SolutionPath"];
            var directoryInfo = new DirectoryInfo(solutionPath);
            var list = GetFiles(directoryInfo);
            Program.WriteColor(string.Format("Total Project Files {0}", list.Count), ConsoleColor.Blue);
            foreach (var fileInfo in list)
            {
                //  Console.WriteLine("Process {0}", fileInfo.Name);
                UpdateFile(fileInfo);

            }
            var solutionFile = ConfigurationManager.AppSettings["SolutionFile"];
            var lines = File.ReadLines(solutionFile);
            var x = new SolutionFileUpdater(lines, dependencies);
            x.Load();
            x.ReWrite();


        }
        public void UpdateFile(FileInfo fileInfo)
        {
            try
            {
                if (inExcludeList(fileInfo.Name))
                    return;
                var doc = XDocument.Load(fileInfo.FullName);
                XNamespace rootNamespace = doc.Root.Name.NamespaceName;
                if (UpdateCopyLocalReference(doc, rootNamespace) || (!UpdateProjectReference(doc, rootNamespace) ||
                                                                     UpdateReference(doc, rootNamespace))
                    )
                {
                    doc.Save(fileInfo.FullName);
                    Console.WriteLine();
                    Program.WriteColor(string.Format("Update {0}", fileInfo.Name), ConsoleColor.Yellow);
                }
                UpdateProjectDependenceis(doc, fileInfo.Name.Remove(fileInfo.Name.LastIndexOf('.')));
            }
            catch (Exception ex)
            {
                Program.WriteColor(string.Format("Error {0}", fileInfo.Name), ConsoleColor.Yellow);
            }

        }

        private bool inExcludeList(string name)
        {
            foreach (var item in Program.ExcludeList)
            {
                if (name.ToLower().Contains(item.ToLower())) return true;
            }
            return false;
        }


        private void UpdateProjectDependenceis(XDocument doc, string fullName)
        {
            var list = new List<string>();
            dependencies.Add(fullName, list);

            var x = GetReferenceElements(doc);
            foreach (var item in x)
            {
                if (inExcludeList(fullName))
                    continue;

                if (!Program.ValidNameSpace.Any(c => item.Value.Contains(c)))
                    continue;
                if (item.Attributes("Include").Any())
                {
                    if (item.Attribute("Include").Value.IndexOf(',') == -1)
                        list.Add(item.Attribute("Include").Value);
                    else
                        list.Add(item.Attribute("Include").Value.Remove(item.Attribute("Include").Value.IndexOf(',')));
                }
            }

        }

        public Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        private bool UpdateCopyLocalReference(XDocument doc, XNamespace rootNamespace)
        {
            var update = false;
            var x =
                GetReferenceElements(doc).Where(c => !inExcludeList(c.Value));


            foreach (var item in x.Where(element => Program.ValidNameSpace.Any(c => element.Value.Contains(c))))
            {
                var doUpdate = CheckCopyLocal(item, rootNamespace);
                update = update || doUpdate;
                doUpdate = CheckHinthPath(item, rootNamespace);
                update = update || doUpdate;
            }
            return update;
        }

        private static List<XElement> GetReferenceElements(XDocument doc)
        {
            return doc.Root.Descendants()
                .Where(c => c.Name.LocalName == "Reference" && c.Parent.Name.LocalName == "ItemGroup")
                .ToList();
        }

        private bool CheckCopyLocal(XElement element, XNamespace rootNamespace)
        {
            XElement privateNode;
            if (!Program.ValidNameSpace.Any(c => element.Value.Contains(c)))
                return false;
            var privateNodes = element.Elements().Where(c => c.Name.LocalName == "Private").ToList();

            if (privateNodes.Count == 1)
            {

                privateNode = privateNodes.First();
                if (privateNode.Value.ToLower() == "true" || (privateNode.Value == "false"))
                {
                    privateNode.Value = "False";
                    return true;
                }
                return false;
            }
            if (privateNodes.Count > 1)
                privateNodes.Remove();
            privateNode = new XElement(rootNamespace + "Private");
            privateNode.SetValue("False");
            Program.WriteColor("Add CopyLocal Elements", ConsoleColor.Gray);
            element.Add(privateNode);
            return true;
        }

        private bool CheckHinthPath(XElement element, XNamespace rootNamespace)
        {

            XElement hintPath;
            var hintPaths = element.Elements().Where(c => c.Name.LocalName == "HintPath").ToList();


            if (hintPaths.Count == 1)
            {

                hintPath = hintPaths.First();
                if (!hintPath.Value.StartsWith("..\\bin\\"))
                {
                    var oldVal = hintPath.Value;
                    var newVal = "..\\bin" + oldVal.Remove(0, oldVal.LastIndexOf("\\"));
                    hintPath.Value = newVal;
                    Program.WriteColor(string.Format("Changed  Reference Path {0} => {1}", oldVal, newVal), ConsoleColor.Gray);

                    return true;
                }
                return false;
            }
            else
            {
                Program.WriteColor(string.Format("Please Check By Hand {0} Hint Count {1}", element.ToString(), hintPaths.Count), ConsoleColor.Red);

            }
            return true;
        }
        private bool UpdateProjectReference(XDocument doc, XNamespace rootNamespace)
        {
            var x =
                doc.Root.Descendants()
                    .Where(c => c.Name.LocalName == "ProjectReference" && c.Parent.Name.LocalName == "ItemGroup")
                    .ToList();
            if (!x.Any())
                return true;

            var x2 =
                doc.Root.Descendants()
                    .FirstOrDefault(c => c.Name.LocalName == "Reference" && c.Parent.Name.LocalName == "ItemGroup");
            XElement item2 = null;
            foreach (var item in x)
            {
                x2.Parent.Add(Create(item, rootNamespace));
                item2 = item.Parent;
            }
            if (item2 != null)
                item2.Remove();

            return false;
        }
        private bool UpdateReference(XDocument doc, XNamespace rootNamespace)
        {

            var itemGroups =
                doc.Root.Descendants()
                    .Where(c => c.Name.LocalName == "Reference" && c.Parent.Name.LocalName == "ItemGroup" && c.Parent.Parent.Name.LocalName == "Project").Select(c => c.Parent).Distinct().ToList();

            if (itemGroups.Count == 1) return false;

            for (int i = 1; i < itemGroups.Count; i++)
            {
                itemGroups[0].Add(itemGroups[i].Elements());
                itemGroups[i].Remove();
            }
            return true;

            //if (!x.Any())
            //    return true;

            //var x2 =
            //    doc.Root.Descendants()
            //       .FirstOrDefault(c => c.Name.LocalName == "Reference" && c.Parent.Name.LocalName == "ItemGroup");
            //XElement item2 = null;
            //foreach (var item in x)
            //{
            //    x2.Parent.Add(Create(item, rootNamespace));
            //    item2 = item.Parent;
            //}
            //if (item2 != null)
            //    item2.Remove(); 
            return false;
        }
        public XElement Create(XElement element, XNamespace rootNamespace)
        {
            var name = element.Elements().Single(c => c.Name.LocalName == "Name").Value;
            var e = new XElement(rootNamespace + "Reference");
            e.SetAttributeValue("Include", name + ", Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL");
            var specificVersion = new XElement(rootNamespace + "SpecificVersion");
            specificVersion.SetValue("False");
            e.Add(specificVersion);

            var hintPath = new XElement(rootNamespace + "HintPath");
            hintPath.SetValue(@"..\bin\" + name + ".dll");
            e.Add(hintPath);
            var @private = new XElement(rootNamespace + "Private");
            @private.SetValue("False");
            e.Add(@private);
            return e;
        }
        private List<FileInfo> GetFiles(DirectoryInfo directoryInfo)
        {
            var list = new List<FileInfo>();
            Program.ValidNameSpace.ForEach(c =>
            {
                list.AddRange(directoryInfo.GetFiles(c+".*.csproj", SearchOption.AllDirectories));    

            });
            return list.Distinct().ToList();
        }
    }
}