using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JFFabrics.Metadata
{
    public class JFFabricsDetailsFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("BOOK NUMBER", ScanField.ItemNumber),
            new FileProperty("PATTERN", ScanField.Pattern),
            new FileProperty("COLOR_NO", ScanField.ColorNumber),
            new FileProperty("CONTENT", ScanField.Ignore),
            new FileProperty("FRCONTENT", ScanField.Ignore),
            new FileProperty("CONTRACT", ScanField.Ignore),
            new FileProperty("MULTI-PURPOSE", ScanField.Ignore),
            new FileProperty("UPHOLSTERY", ScanField.Ignore),
            new FileProperty("DRAPERY", ScanField.Ignore),
            new FileProperty("Tapes & Trim", ScanField.Ignore),
            new FileProperty("Wallcoverings", ScanField.Ignore),
            new FileProperty("Rugs", ScanField.Ignore),
            new FileProperty("ECO FRIENDLY", ScanField.Ignore),
            new FileProperty("HS Code", ScanField.Ignore),
            new FileProperty("Desc #2", ScanField.Ignore),
            new FileProperty("Weight", ScanField.Ignore),
            new FileProperty("Avg Pce Size", ScanField.Ignore),
            new FileProperty("Description", ScanField.Ignore),
            new FileProperty("NOOFCOLORS", ScanField.Ignore),
            new FileProperty("COUNTRY_OF_ORIGIN", ScanField.Ignore),
            new FileProperty("WIDTH", ScanField.Ignore),
            new FileProperty("EMBROIDERED WIDTH", ScanField.Ignore),
            new FileProperty("HORIZONTAL", ScanField.Ignore),
            new FileProperty("VERTICAL", ScanField.Ignore),
            new FileProperty("FINISH", ScanField.Ignore),
            new FileProperty("RAILROADED", ScanField.Ignore),
            new FileProperty("CARECODE", ScanField.Ignore),
            new FileProperty("FLAMMABILITY", ScanField.Ignore),
            new FileProperty("SURFACEABRASION", ScanField.Ignore),
            new FileProperty("SEAMSLIPPAGEWARP", ScanField.Ignore),
            new FileProperty("SEAMSLIPPAGEFILLING", ScanField.Ignore),
            new FileProperty("BREAKINGSTRENGTHWARP", ScanField.Ignore),
            new FileProperty("BREAKINGSTRENGTHFILLING", ScanField.Ignore),
            new FileProperty("TEARSTRENGTHWARP", ScanField.Ignore),
            new FileProperty("TEARSTRENGTHFILLING", ScanField.Ignore),
            new FileProperty("COLORFASTNESSTOWATER", ScanField.Ignore),
            new FileProperty("COLORFASTNESSTOSOLVENT", ScanField.Ignore),
            new FileProperty("COLORFASTNESSTOLIGHT", ScanField.Ignore),
            new FileProperty("CROCKINGWET", ScanField.Ignore),
            new FileProperty("CROCKINGDRY", ScanField.Ignore),
            new FileProperty("PILLING", ScanField.Ignore),
            new FileProperty("FR / NFPA 701", ScanField.Ignore),
            new FileProperty("ABRASION (GENERAL - 15,000-30,000, DOUBLE RUBS)", ScanField.Ignore),
            new FileProperty("ABRASION (HEAVY DUTY- 30,000+ DOUBLE RUBS)", ScanField.Ignore),
            new FileProperty("WASHABLE", ScanField.Ignore),
            new FileProperty("Publish", ScanField.Ignore),
            new FileProperty("languageid", ScanField.Ignore),
            new FileProperty("typeid", ScanField.Ignore),
            new FileProperty("Book NO", ScanField.Ignore),
            new FileProperty("Book Description", ScanField.Ignore),
        };

        private readonly IStorageProvider<JFFabricsVendor> _storageProvider;
        public JFFabricsDetailsFileLoader(IStorageProvider<JFFabricsVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadData()
        {
            var staticFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFilesFolder, "JFFabrics_Details.xlsx"), Properties, ScanField.ItemNumber, 1, 2);
        }
    }
}