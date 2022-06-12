using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CsvHelper;

namespace InsideFabric.Data
{
    public class HomewareCategoryManager
    {
        public HomewareCategoryNode LoadTree()
        {
            var treeLocation = ConfigurationManager.AppSettings["SchemaTreeCSVPath"];
            var nodeList = LoadCsvFile(treeLocation);
            var root = BuildTree(nodeList);
            return root;
        }

        public List<HomewareCategoryNode> LoadCsvFile()
        {
            var filepath = ConfigurationManager.AppSettings["SchemaTreeCSVPath"];
            return LoadCsvFile(filepath);
        }

        private List<HomewareCategoryNode> LoadCsvFile(string inputFilename)
        {
            var list = new List<HomewareCategoryNode>();
            if (!File.Exists(inputFilename))
                return list;

            using (var fs = File.Open(inputFilename, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    using (var csv = new CsvReader(sr))
                    {
                        while (csv.Read())
                        {
                            var fields = csv.CurrentRecord;
                            var node = new HomewareCategoryNode();
                            node.MenuName = fields[0];
                            node.ParentId = Convert.ToInt32(fields[1]);
                            node.Id = Convert.ToInt32(fields[2]);
                            node.Included = Convert.ToBoolean(fields[3]);

                            int googleID = 0;
                            int.TryParse(fields[4], out googleID);
                            node.GoogleTaxonomyId = googleID;

                            node.Title = fields[5];
                            node.Description = fields[6];
                            node.SeKeywords = fields[7];
                            node.SeDescription = fields[8];
                            node.SearchTerms = fields[9];
                            list.Add(node);
                        }
                    }
                }
            }
            return list;
        }

        private HomewareCategoryNode BuildTree(List<HomewareCategoryNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.ParentId != 0)
                {
                    var parentNode = nodes.SingleOrDefault(x => x.Id == node.ParentId);
                    node.Parent = parentNode;
                }
            }

            foreach (var node in nodes)
            {
                var children = nodes.Where(x => x.ParentId == node.Id);
                node.Children = children.ToList();
            }
            return nodes.Single(x => x.MenuName == "Root");
        }

        private IEnumerable<HomewareCategoryNode> GetAllNodes(HomewareCategoryNode root)
        {
            var allNodes = new List<HomewareCategoryNode> {root};
            if (root.Children == null) return allNodes;

            foreach (var child in root.Children)
                allNodes.AddRange(GetAllNodes(child));
            return allNodes;
        }

        public string GenerateCategoryEnum(HomewareCategoryNode root)
        {
            var nodes = GetAllNodes(root).Select(x => string.Format("[Category({0}, {1})] ", x.Id, x.Included.ToString().ToLower()) + x.MenuName.Replace(" ", "_").Replace(",", "")).ToList();
            nodes.Insert(0, "[Category(0, false)] Unknown");
            nodes.Insert(1, "[Category(1, false)] Ignored");
            return string.Join("," + Environment.NewLine, nodes);
        }
    }
}