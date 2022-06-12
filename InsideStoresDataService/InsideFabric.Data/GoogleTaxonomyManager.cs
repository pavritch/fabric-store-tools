using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace InsideFabric.Data
{
    public class GoogleTaxonomyManager
    {
        private readonly Dictionary<int, List<string>> _productCategories;

        public GoogleTaxonomyManager()
        {
            _productCategories = new Dictionary<int, List<string>>();
            LoadTaxonomyFile();
        }

        private void LoadTaxonomyFile()
        {
            var fileLocation = ConfigurationManager.AppSettings["GoogleTaxonomyPath"];
            // the first line is a comment
            var lines = File.ReadAllLines(fileLocation).Skip(1);
            foreach (var line in lines)
            {
                var id = Convert.ToInt32(line.Split(new[] {'-'}).First());
                var categories = line.Split(new[] {'-'}).Last().Split(new []{'>'}).Select(x => x.Trim()).ToList();
                _productCategories.Add(id, categories);
            }
        }

        public List<string> GetTaxonomy(int id)
        {
            return _productCategories.ContainsKey(id) ? _productCategories[id] : new List<string>();
        }
    }
}