using System.Collections.Generic;

namespace InsideFabric.Data
{
    /// <summary>
    /// Holds fields which have been updated by human review which should survive scanner updates.
    /// </summary>
    public class HumanHomewareProductFeatures : HomewareProductFeatures
    {
        public string Name { get; set; }
        public string SEName { get; set; }
        public string SETitle { get; set; }
        public string SEDescription { get; set; }

        // the base category property is not nullable, so replace
        public new int? Category { get; set; }

        public int? PrimarySqlCategory { get; set; }
    }
}

