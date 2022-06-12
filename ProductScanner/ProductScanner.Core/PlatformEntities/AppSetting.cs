namespace ProductScanner.Core.PlatformEntities
{
    public class AppSetting
    {
        public int Id { get; set; }
        public string Group { get; set; }
        public string Store { get; set; }
        public int? VendorId { get; set; }
        public string Name { get; set; }
        public string ConfigValue { get; set; }
        public string Description { get; set; }
    }
}