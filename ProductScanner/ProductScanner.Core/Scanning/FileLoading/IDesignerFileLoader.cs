using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProductScanner.Core.Config;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public interface IDesignerFileLoader
    {
        List<string> GetDesigners();
    }

    public class DesignerFileLoader : IDesignerFileLoader
    {
        private readonly IAppSettings _appSettings;

        public DesignerFileLoader(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        // for now, just using a single file
        public List<string> GetDesigners()
        {
            var staticRoot = _appSettings.StaticRoot;
            var designersFile = Path.Combine(staticRoot, "Designers.txt");
            return File.ReadAllLines(designersFile).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
    }
}