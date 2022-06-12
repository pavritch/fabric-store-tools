using System.Collections.Generic;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class ItemColor
    {
        private readonly Dictionary<string, string> _spellingFixes = new Dictionary<string, string>
        {
            {"Abalo", "Abalone"},
            {"Amb", "Amber"},
            {"Ameth", "Amethyst"},
            {"Aqu", "Aqua"},
            {"Aubergene", "Aubergine"},
            {"Aub", "Auburn"},
            {"Autmn", "Autumn"},
            {"Avacado", "Avocado"},
            {"Avoc", "Avocado"},
            {"Awnin", "Awning"},
            {"Azu", "Azure"},
            {"Bei", "Beige"},
            {"Beig", "Beige"},
            {"Bg", "Beige"},
            {"Bge", "Beige"},
            {"Bl", "Black"},
            {"Blck", "Black"},
            {"Blacl", "Black"},
            {"Blk", "Black"},
            {"Blu", "Blue"},
            {"Brn", "Brown"},
            {"Brnz", "Bronze"},
            {"Brw", "Brown"},
            {"Brwn", "Brown"},
            {"Browwn", "Brown"},
            {"Burg", "Burgundy"},
            {"Burgndy", "Burgundy"},
            {"Burgandy", "Burgundy"},
            {"Chamimile", "Chamomile"},
            {"Champaign", "Champagne"},
            {"Charc", "Charcoal"},
            {"Choc", "Chocolate"},
            {"Choc.", "Chocolate"},
            {"Choclate", "Chocolate"},
            {"Choco", "Chocolate"},
            {"Cml", "Caramel"},
            {"Cor", "Coral"},
            {"Crm", "Cream"},
            {"Crnberry", "Cranberry"},
            {"Fuchisa", "Fuschia"},
            {"Fusch", "Fuschia"},
            {"Fuscha", "Fuschia"},
            {"Fuschias", "Fuschia"},
            {"Geen", "Green"},
            {"Gld", "Gold"},
            {"Gls", "Glass"},
            {"Gn", "Green"},
            {"Gre", "Green"},
            {"Gree", "Green"},
            {"Gren", "Green"},
            {"Grn", "Green"},
            {"Gry", "Gray"},
            {"Grey", "Gray"},
            {"Gy", "Gray"},
            {"Irn", "Iron"},
            {"Ivor", "Ivory"},
            {"Iv", "Ivory"},
            {"Ivr", "Ivory"},
            {"Ivry", "Ivory"},
            {"Khak", "Khaki"},
            {"Khki", "Khaki"},
            {"Lav", "Lavender"},
            {"Lava", "Lavender"},
            {"Lavan", "Lavender"},
            {"Lavnr", "Lavender"},
            {"Lavndr", "Lavender"},
            {"Lavand", "Lavender"},
            {"Lavander", "Lavender"},
            {"Lvd", "Lavender"},
            {"Lvndr", "Lavender"},
            {"Magen", "Magento"},
            {"Magent", "Magento"},
            {"Mik", "Milk"},
            {"Mllk", "Milk"},
            {"Mul", "Multi"},
            {"Mult", "Multi"},
            {"Nat", "Natural"},
            {"Natl", "Natural"},
            {"Nvy", "Navy"},
            {"Oli", "Olive"},
            {"O.White", "Off-White"},
            {"Org", "Orange"},
            {"Orn", "Orange"},
            {"Orng", "Orange"},
            {"Ornge", "Orange"},
            {"Pin", "Pink"},
            {"Pnk", "Pink"},
            {"Pinki", "Pink"},
            {"Pomegranat", "Pomegranate"},
            {"Prpl", "Purple"},
            {"Prple", "Purple"},
            {"Pu", "Purple"},
            {"Pur", "Purple"},
            {"Purp", "Purple"},
            {"Purpl", "Purple"},
            {"Rd", "Red"},
            {"Re", "Red"},
            {"Redish", "Reddish"},
            {"Ros", "Rose"},
            {"Slv", "Silver"},
            {"Silv", "Silver"},
            {"Silve", "Silver"},
            {"Silvr", "Silver"},
            {"Slvr", "Silver"},
            {"Tau", "Taupe"},
            {"Terracota", "Terracotta"},
            {"Tuq", "Turquoise"},
            {"Turq", "Turquoise"},
            {"Watermellon", "Watermelon"},
            {"Wht", "White"},
            {"Whte", "White"},
            {"Whi", "White"},
            {"Whit", "White"},
            {"Wt", "White"},
            {"Wyt", "White"},
            {"Yel", "Yellow"},
            {"Yello", "Yellow"},
            {"Yellw", "Yellow"},
            {"Yllw", "Yellow"},
            {"Yllow", "Yellow"},
            {"Ylw", "Yellow"},
            {"Yw", "Yellow"},
        };

        private readonly string _value;
        public ItemColor(string value)
        {
            _value = value;
        }

        public string GetFormattedColor()
        {
            var formatted = _value.Trim().TitleCase();

            //{"Drk ", "Dark "},
            formatted = formatted.ReplaceWholeWord("Drk ", "Dark ");
            formatted = formatted.ReplaceWholeWord("Dk. ", "Dark ");
            formatted = formatted.ReplaceWholeWord("Dk ", "Dark ");
            formatted = formatted.ReplaceWholeWord("D. ", "Dark ");

            formatted = formatted.ReplaceWholeWord("Drk", "Dark ");
            formatted = formatted.ReplaceWholeWord("Dk.", "Dark ");
            formatted = formatted.ReplaceWholeWord("Dk", "Dark ");
            formatted = formatted.ReplaceWholeWord("D.", "Dark ");

            formatted = formatted.ReplaceWholeWord("Lt. ", "Light ");
            formatted = formatted.ReplaceWholeWord("Lt ", "Light ");
            formatted = formatted.ReplaceWholeWord("L. ", "Light ");
            formatted = formatted.ReplaceWholeWord("L ", "Light ");

            formatted = formatted.ReplaceWholeWord("Lt.", "Light ");
            formatted = formatted.ReplaceWholeWord("Lt", "Light ");
            formatted = formatted.ReplaceWholeWord("L", "Light ");

            formatted = formatted.ReplaceWholeWord("M.", "Medium ");
            formatted = formatted.ReplaceWholeWord("Med ", "Medium ");

            formatted = formatted.Replace("è", "e");
            formatted = formatted.TitleCase();
            formatted = formatted.Trim(new[] {'.'});

            foreach (var spellError in _spellingFixes)
            {
                formatted = formatted.ReplaceWholeWord(spellError.Key, spellError.Value);
            }

            return formatted;
        }
    }
}