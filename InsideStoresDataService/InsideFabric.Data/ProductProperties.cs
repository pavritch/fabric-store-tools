using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace InsideFabric.Data
{
    public class ProductProperties
    {
        #region Propert List (ordered)

        // IMPORTANT: identical list in Fabric Updater!
 
        public static string[] _properties = new string[]
        {
            "Brand",
            "Item Number",
            "Product",
            "Large Cord",
            "Cord",
            "Cordette",
            "Tassel",
            "Product Name",
            "Pattern",
            "Pattern Name",
            "Pattern Number",
            "Color Name",
            "Color Number", // 3/31/2013
            "Color",
            "Color Group",
            "Minimum Order",
            "Order Info",
            "Lead Time",
            "Same As SKU",
            "Designer", // added Peter 4/25/2012
            "Book",
            "Collection",
            "Category",
            "Group",
            "Feature",
            "Width",
            "Product Use",
            "Product Type",
            "Type",
            "Material",
            "Style",
            "Upholstery Use",
            "Use",
            "Coordinates",
            "Unit",
            "Unit Of Measure",
            "Design",
            "Backing",
            "Construction",
            "Finish",
            "Finish Treatment",
            "Finishes",
            "Furniture Grade",
            "Grade",
            "Repeat",
            "Horizontal Repeat",
            "Vertical Repeat",
            "Horizontal Half Drop",
            "Direction",
            "Hide Size",
            "Match",
            "Prepasted",
            "Railroaded",
            "Soft Home Grade",
            "Strippable",
            "Thickness",
            "Treatment",
            "Weight",
            "Country of Finish",
            "Country of Origin",
            "Content",
            "Fabric Contents",
            "Average Bolt",
            "Base",
            "Face",
            "Fabric Performance",
            "Durability",
            "Fire Code",
            "Flammability",
            "Flame Retardant", // 3/31/2013
            "WYZ/MAR",
            "Cleaning",
            "Cleaning Code",
            "UV Protection Factor",
            "UV Resistance",
            "Code",
            "Other",
            "Comment",
            "Note",
            "Additional Info",
        };
        
        #endregion


        public static XElement MakePropertiesXml(Dictionary<string, string> input)
        {
            var dic = input.ToDictionary(k => k.Key, v => v.Value);

            var root = new XElement("ExtensionData");
            foreach (var label in _properties.Intersect(dic.Keys))
            {
                var el = new XElement("Property");
                el.SetAttributeValue("Name", label);
                el.SetAttributeValue("Value", dic[label]);
                root.Add(el);

                dic.Remove(label);
            }

            // pick up any remaining properties (any order) just in case
            // there were some not on the original list...resulting in 
            // all properties in the dic being accounted for.

            foreach (var item in dic)
            {
                var el = new XElement("Property");
                el.SetAttributeValue("Name", item.Key);
                el.SetAttributeValue("Value", item.Value);
                root.Add(el);
            }

            return root;
        }

    }
}
