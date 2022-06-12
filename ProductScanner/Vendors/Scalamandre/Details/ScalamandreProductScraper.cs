using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Scalamandre.Details
{
    /*
    public class ScalamandreProductScraper : ProductScraper<ScalamandreVendor>
    {
        public ScalamandreProductScraper(IPageFetcher<ScalamandreVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var url = context.ScanData.GetDetailUrl();
            var mpn = context.DetailUrl.GetDocumentName().Replace(".html", "");
            var page = await PageFetcher.FetchAsync(url.Replace("skuhtml", "skuhtmlp"), CacheFolder.Details, mpn);

            if (page.InnerText.ContainsIgnoreCase("WebWizard Diagnostic Display"))
            {
                var pageTwo = await PageFetcher.FetchAsync("http://visualaccess.scalamandre.com/wwoob.htm", CacheFolder.Details, mpn + "-2");
                page = await PageFetcher.FetchAsync("http://visualaccess.scalamandre.com/wwizshow.asp", CacheFolder.Details, mpn + "-3");
            }

            if (mpn.StartsWith("WP"))
            {
                // correct some mis-classifications in the discoverer
                context.ScanData[ScanField.ProductGroup] = "Wallcovering";
            }

            var product = new ScanData(context.ScanData);
            product[ScanField.Bullet1] = page.GetFieldHtml("#desc");
            product[ScanField.PatternName] = page.GetFieldValue("#firstline");
            product[ScanField.ManufacturerPartNumber] = mpn;

            GetProperties(product, mpn);

            var imageElement = page.QuerySelector("#imagetd img");
            if (imageElement != null)
            {
                var imageUrl = imageElement.Attributes["src"].Value.Replace("wid=320", "wid=600").Replace("hei=291", "hei=600");
                product.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));
            }

            return new List<ScanData> {product};
        }

        private void GetProperties(ScanData p, string mpn)
        {
            // intended to help when a multi-line property on the website might actually contain
            // data that to us should be split between our own properties. Given a list of possible
            // values, see if any of the words are find in one of the lines, and if so, associate 
            // the entire line with the target property - skip any other lines.
            Action<List<string>, ScanField, string[]> addWhenFound = (lst, prop, words) =>
            {
                int ndx = 0;
                var toBeRemoved = new List<int>();

                foreach (var line in lst)
                {

                    foreach (var word in words)
                    {
                        if (line.ContainsIgnoreCase(word))
                        {
                            if (p.ContainsKey(prop))
                            {
                                var existing = p[prop];
                                p[prop] = existing + ", " + line;
                            }
                            else
                            {
                                p[prop] = line;
                            }
                            toBeRemoved.Add(ndx);
                            break;
                        }
                    }

                    ndx++;
                }

                // remove in reverse
                foreach (var i in toBeRemoved.OrderByDescending(e => e))
                    lst.RemoveAt(i);
            };

            if (mpn.ContainsIgnoreCase("-"))
            {
                var arySkuParts = mpn.Split('-');
                p[ScanField.PatternNumber] = arySkuParts[0];
                p[ScanField.ColorNumber] = arySkuParts[1];
            }

            // the temp1 data is a long span which has a bunch of br lines, but also commonly includes
            // the table with stock information. This is presented as html.

            var temp1 = p[ScanField.Bullet1];
            if (temp1 == null) return;

            // remove any embedded tables
            while (temp1.ContainsIgnoreCase("<table"))
            {
                var tableStart = temp1.IndexOf("<table");
                var tableEnd = temp1.IndexOf("</table>");

                temp1 = temp1.Substring(0, tableStart) + temp1.Substring(tableEnd + 8);
            }

            var temp1Lines = temp1.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList();
            addWhenFound(temp1Lines, ScanField.Content, new[] {"%", "Suede", "Leather", "Horsehair"});
            addWhenFound(temp1Lines, ScanField.ColorName, new[] {"COLOR:"});
            addWhenFound(temp1Lines, ScanField.Cleaning, new[]
            {
                "DRY CLEAN", "Cold Wash", "no water solvents", "bleach", "Do not tumble dry", "do not wash", "do not iron",
                "May use water or mild solvent cleaner to the fabrics surface", "Washable", "Warm Iron", "Medium Iron", "Use Iron on",
            });
            addWhenFound(temp1Lines, ScanField.Collection, new[] {"COLLECTION"});
            addWhenFound(temp1Lines, ScanField.OrderInfo, new[]
            {
                "Sold in double rolls only", "Specify Length", "SOLD BY THE PANEL", "SOLD BY THE HIDE",
                "Sold by full hide only", "Sold By The Repeat.", "Yards to A Roll", "Packed & Sold", 
                "One Set Contains", "Special Order Only", "increments only", "bolts=", "Sold In"
            });

            addWhenFound(temp1Lines, ScanField.MinimumOrder, new[] {"Yard Minimum", "Bolt Minimum Order",});

            addWhenFound(temp1Lines, ScanField.Width, new[] {"WIDTH:"});
            addWhenFound(temp1Lines, ScanField.Repeat, new[] {"RPT:", "REPEAT"});
            addWhenFound(temp1Lines, ScanField.Cost, new[] {"PRICE:", "PRICE "});

            addWhenFound(temp1Lines, ScanField.Durability, new[] {"DOUBLE RUBS", "WYZENBEEK", "MARTINDALE", "D.R."});
            addWhenFound(temp1Lines, ScanField.FlameRetardant, new[] {"UFAC", "FLAME", "NFPA", "CAL 117", "ASTM ", "Fire Retardant", "#117-Class"});

            addWhenFound(temp1Lines, ScanField.ProductUse, new[] {"CAN USE", "INDOOR", "OUTDOOR", "DRAPERY", "UPHOLSTERY", "LIGHT DUTY", "WINDOWS", "Recommended Use", "Bedcovers", "Pillows", "upholstered walls", "Designed for", "Wallcovering use only"});

            addWhenFound(temp1Lines, ScanField.StockCount, new[] {"Stock Quantity", "stock check", "quick ship", "prompt delivery", "please call", "out of stock"});

            addWhenFound(temp1Lines, ScanField.Finish, new[] {"Finish", "Scotchgarded", "Stain Resistant", "Sanforized", "Water Resistant", "Repellant"});

            addWhenFound(temp1Lines, ScanField.Prepasted, new[] {"Prepasted"});
            addWhenFound(temp1Lines, ScanField.Backing, new[] {"Acrylic Backed", "Acrylic Backing", "acrylic back", "Can be backed"});
            addWhenFound(temp1Lines, ScanField.Length, new[] {"Length:"});
            addWhenFound(temp1Lines, ScanField.Coordinates, new[] {"Matching and Companion:"});
            addWhenFound(temp1Lines, ScanField.Screens, new[] {"No. Screens:"});
            addWhenFound(temp1Lines, ScanField.Layout, new[] {"LAYOUT:"});
            addWhenFound(temp1Lines, ScanField.CordSpread, new[] {"Cord Spread:"});
            addWhenFound(temp1Lines, ScanField.Style, Descriptors);
            addWhenFound(temp1Lines, ScanField.AdditionalInfo, AdditionalInformation);
            addWhenFound(temp1Lines, ScanField.Category, TrimDescriptions);
            addWhenFound(temp1Lines, ScanField.Ignore, IgnoreList);

            foreach (var row in temp1Lines)
            {
                if (row.ContainsIgnoreCase("Available Quantity"))
                {
                    p[ScanField.StockCount] = row;
                }
            }

            var price = p[ScanField.Cost];

            var indexDollar = price.IndexOf("$");
            if (indexDollar != -1)
            {

                // could include both the original and the clearance price

                //Original Price $313.50
                //Price $76.95

                if (price.ContainsIgnoreCase("Original Price"))
                {
                    var firstPrice = price.Substring(indexDollar + 1).Replace(",", "").TakeOnlyFirstDecimalToken(2);
                    var secondPrice = price.Substring(price.LastIndexOf("$") + 1).Replace(",", "").TakeOnlyFirstDecimalToken(2);

                    p[ScanField.Cost] = Math.Min(firstPrice, secondPrice).ToString();
                }
                else
                {
                    p[ScanField.Cost] = price.Substring(indexDollar + 1).Replace(",", "").TakeOnlyFirstDecimalToken(2).ToString();
                }

            }
            else
                p[ScanField.Cost] = "0.00";
        }

        #region Data Keys
        private static readonly string[] IgnoreList =
        {
            "for authenticity.",
            "THE MEDICI ARCHIVE PROJECT",
            "Quick Ship Item: Most Quick Ship Items are Available for Prompt Delivery",
            "Shipping Charge subject to Destination and UPS Rates for non-sample purchases",
            "Up to 5 memo samples: only $9.50/shipped",
            "CAUTION-CAUTION-CAUTION",
            "Due to unique characteristics of this fabric",
            "are inherent in the overall look.",
            "yd Client Mn",
            "Stock is on Order",
            "OEKO-TEX Standard 100",
            "Certified By International",
            "Scalamandre CONTRACT",
            "Repp",
            "Interpretation",
            "Immediate Delivery"

        };

        private static readonly string[] TrimDescriptions =
        {
            "Cord",
            "Metallic Cord",
            "Cord On Tape",
            "3 Ply Plain Cord",
            "Looped And Blocked Braid",
            "Braid With Chenille Block",
            "Double Scallop Braid",
            "Double Edge Cut Gimp",
            "Striped Ribbon With Bow",
            "Open Scroll Braid",
            "Braid (Heading Of Fx1350)",
            "Double Open Scroll Braid",
            "Braid (Heading Of Fx1320)",
            "Plated Cord",
            "Plaited Cord",
            "Plaited Cord On Tape",
            "Plaited & Sinker Cord",
            "Plaited & Sinker Cord On Tape",
            "Cord To Match T2000 Chairtie",
            "Crepe Cord",
            "Bullion Fringe",
            "Key Tassel",
            "Beaded Fringe",
            "Mold Fringe",
            "Braid",
            "Rosette With Tassel",
            "Single Tassel Tieback",
            "Double Tassel Tieback",
            "Medallion Design Braid",
            "Green Jade Chicken With Mold",
            "Open Scroll Braid With Tulips",
            "Glass Bead Tassel Fringe",
            "Bullion Fringe With Tassels",
            "Chairtie",
            "Rosette With Mold Hangers",
            "Greek Key Border",
            "White Looped Braid",
            "Corded Gimp",
            "Bourette Gimp",
            "Striped Ribbon",
            "Striped Ribbons",
            "Ribbon Tuft",
            "Uncut Tuft",
            "Gros Grain Ribbon",
            "Embroidered Braid",
            "Double Sided Loop Gimp",
            "Jacquard Empire Galloon",
            "Checkered Braid",
            "Braid With Design",
            "Flat Braid",
            "Cord With Lip",
            "6-Ply Crepe Cord",
            "3-Ply Plain Cord",
            "Chenille Cord On Tape",
            "Gimp",
            "Push Up Tassel Chair Tie",
            "Multi Cut Moss Fringe",
            "Alternating Fringe",
            "Basketweave Braid",
            "Basketweave Gimp",
            "Plain Band Chair Tie",
            "Picture Hanger",
            "Tieback",
            "Bow (Set Of Twelve)",
            "Cut Tuft",
            "Tape",
            "Satin Tape",
            "Empire Gimp",
            "Scalloped Braid",
            "Open Work Braid",
            "Loop Braid With Satin Center",
            "Looped Gimp",
            "Satin Fringed Edges",
            "Ribbed Braid",
            "Jacquard Braid",
            "Scalloped Gimp",
            "Blocked Gimp",
            "Double Scroll Braid",
            "Alternating Bullion Fringe",
            "Cut Fringe",
            "Scalloped Top Cut Fringe",
            "Cut Fringe With Hangers",
            "Loop Fringe",
            "Scallop Loop Fringe",
            "Cut Moss Fringe",
            "Moss Fringe",
            "Tassel Fringe",
            "Scalloped Tassel Fringe",
            "Base Fringe",
            "Tuft Fringe",
            "Balloon Tassel Fringe",
            "Cluster Tassel Fringe",
            "Large Stained Mold Fringe",
            "Small Stained Mold Fringe",
            "Chair Tie",
            "Tassel Tieback",
            "Looped Tuft (Set Of Twelve)",
            "Tape With Design",
            "French Gimp",
            "Corded Double French Gimp",
            "Corded French Gimp",
            "Open Scroll Tapered Tieback",
            "Yellow Jade Medallion,"
        };

        private static readonly string[] Descriptors =
        {
            "Damask",
            "Modified Pattern",
            "Hand Printed",
            "Printed",
            "Printed Union Cloth",
            "Printed Chintz",
            "Printed Glazed Sateen",
            "White House",
            "Hand Printed Toile",
            "Hand Printed Satin",
            "Antique Velvet",
            "Battiste",
            "Adaptation",
            "Hand Printed Union Cloth",
            "Hand Printed Velvet",
            "Hand Printed Velvet Paisley",
            "Hand Printed Slub Duck",
            "Printed & Lightly Glazed",
            "Warp Print",
            "Printed & Glazed Twill",
            "Printed & Glazed Union Cloth",
            "Floral Print",
            "Print",
            "Crush Linen",
            "Printed Damask",
            "Digital Warp Print",
            "Fabric Print",
            "Matelasse",
            "Cut Velvet",
            "Madras",
            "Gros Point",
            "Fancy Jacquard",
            "Embroidery",
            "Novelty",
            "Plaid",
            "Chenille Texture",
            "Small Scale Jacquard",
            "Sheer",
            "Herringbone Sheer",
            "Moired Damask",
            "Yellow Jade Medallion",
            "Distressed Velvet",
            "Striped Velvet",
            "Novelty Lampas",
            "Eagle Design",
            "Strie Lampas",
            "Memories of a Voyage to India",
            "Jacquard Texture",
            "Striped Sheer",
            "Embroidered Sheer",
            "Woven",
            "Velvet Jacquard",
            "Armure",
            "Heavy Texture Jacquard",
            "Textured",
            "Leno Weave",
            "Taffeta",
            "Taffeta Plaid",
            "Moired Plaid",
            "Reproduction Circa",
            "Historic Representation Circa:  18th Century",
            "Catherine Palace, Blue Sitting Room Wallcovering",
            "Villa Louis Master Bedroom Suite Curtains & Portier. Prairie du Chien, Wisconsin. State Historical Society Of Wisconsin",
            "warp stripe",
            "Interpretation Circa:  1775-1825",
            "Adapted From ",
            "Interpretation Circa:  Early 20th Century",
            "Recreated From ",
            "Interpretation Circa:  18th Century",
            "Stripe",
            "Paisley",
            "Strie Stripe",
            "Reproduction French Empire",
            "Contemporary Jacquard",
            "Lampas",
            "Brocatelle",
            "Replica Of ",
            "Chair Covering Fragment From Cappon House, Holland, Mi.",
            "Historic Representation Circa:  15th Century Arts & Crafts",
            "Lisere",
            "Striped Jacquard",
            "Horizontal Striped Jacquard",
            "Moired Lisere",
            "Horsehair, Stripe",
            "Striped Lampas",
            "Figured Velvet",
            "Chenille Jacquard",
            "Lisere Velvet",
            "Striped Cut Loop Velvet",
            "Texture",
            "Epingle",
            "Check",
            "Gaufre Stripe",
            "Crewel",
            "Floral Striped Lampas",
            "Jacquard Velvet",
            "Lampass",
            "Moired Stripe",
            "Strie Satin",
            "Striped Satin",
            "Antique Satin",
            "Striae Velvet",
            "Panama Canvas",
            "Horizontal Stripe",
            "Embossed Velvet",
            "Satin",
            "Moire",
            "Striae Repp",
            "Solid",
            "Cut & Loop Velvet",
            "Velvet",
            "Dobby",
            "Antique Striae Velvet",
            "Solid Sateen",
            "Solid Horizontal Rib",
            "Embossed Satin Texture",
            "Plain Satin",
            "Cut Pile",
            "Mohair",
            "Adaptation Circa",
            "Ombre Stripe",
            "Strie",
            "Empire Lampas Sofa Panel",
            "Red Room, White House. Dolly Madison Sofa.",
            "Empire Lampas",
            "Baize Cloth",
            "Jacquard",
            "Lampas Chairseat",
            "Satin Stripe",
            "Broad Ribbed Repp",
            "Frieze",
            "Chenille Tapestry",
            "Crepe Stripe",
            "Gobelin",
            "Brocade",
            "Velvet Stripe",
            "Broken Twill",
            "Lampas Chairback & Chairseat",
            "Historic ",
            "Jacquard Stripe",
            "Striped Satin Faille",
            "Sateen",
            "Reproduction",
            "Striped Casement",
            "Solid Polished Canvas",
            "Faille",
            "Horsehair, Sateen",
            "Glazed Strie",
            "Chenille Striped Jacquard",
            "Horsehair, Jaspe Repp",
            "Metallic Barre",
            "Satin & Moire Stripe",
            "Architecture & Design Series",
            "Metallic Sisal",
            "Hand Mottled",
            "Panel Cloth",
            "Paperweave",
            "Tapestry",
            "Sisal Paper",
            "Historic Representation Circa:  1820-1835",
            "Historic Representation Of An Early 19Th Century French Hand Blocked Wallpaper Found At Prestwould Plantation, Clarksville, Va.",
            "Historic Representation Circa:  1850-1875",
            "Metropolitan Opera, N. Y. C.",
            "Speaker Cloth",
            "Plush",
            "Slub Faille",
            "Far Eastern Straw Grasscloth",
            "Sisal Metallic",
            "White House, Washington, D. C. The Red Room Nixon Administration Clinton Administration",
            "Flannel",
            "Chenille",
            "Contemporary",
            "Historic Representation Circa:  Mid 19th Century",
            "Solid Striped Chevron",
            "Flower Rosette",
            "Herringbone",
            "Glazed Strie Satin",
            "Epingle Velvet",
            "Moired Jacquard",
            "Jacquard Chenille",
            "U. S. Treasury Andrew Johnson Suite",
            "Tightweave Jute",
            "Heavy Tightweave Jute",
            "Casement",
            "Chevron Design",
            "Prestwould Plantation, Lelia's Second Floor Bedroom",
            "English Hand Blocked Wallpaper Found At Prestwould Plantation , Clarksville, Va.",
            "Rosettes",
            "Pompon",
            "Piping",
            "Hand Crafted",
            "Greek Key",
            "Green Jade Turtle On Rosette",
            "Rosette Hanging Yellow Jade",
            "Strie Repp",
            "Striped Repp",
            "Venetian Carpet",
        };

        private static readonly string[] AdditionalInformation =
        {
            "Puckering characteristic",
            "Water removes glaze",
            "Resists Damage from Mildew",
            "Use Appropriate Outdoor Cushion Core",
            "Subtle variations exist from repeat to repeat",
            "Shading Characteristic",
            "Horizontal bands on face are characteristic",
            "Reversible",
            "Irregularities inherent",
            "Specks are characteristic",
            "Cut & Uncut Velvet",
            "Acrylic Back for theater seating installations",
            "If reverse side of fabric is desired, order #26413R",
            "Horizontal Barre Characteristic",
            "Crush Marks & Creasing are Characteristic",
            "Distressed Velvet",
            "Can be paperbacked for wallcovering",
            "Specks & shading inherent",
            "Figured Cut Velvet",
            "Cut & Voided Jacquard Velvet",
            "Cut & Uncut Velvet Stripe",
            "Backing is recommended for upholstery",
            "To minimize the variations, press with a hot iron",
            "Hand Woven Cut & Voided Velvet",
            "If reverse side of fabric is desired, order #36119R",
            "Handwoven: Irregularities Inherent",
            "Crush Resistant",
            "Pile direction is downward",
            "Permanant Press",
            "Untrimmed",
            "Avoid prolonged exposure to direct sunlight",
            "Not for direct wall application",
            "Ticket Image is Shown Railroaded",
            "Mirrored Border",
            "Railroad for upholstery",
            "Not recommended for uph",
            "Reproduction includes original irregularities & uneven dyeing",
            "Pretrimmed",
            "Shrink Wrapped",
            "Liner Paper Recommended",
            "Wheat Paste Recommended",
            "Dry Trim",
            "MUST ORDER ONE ROLL OF A-IN EVEN INC.WITH B",
            "Reverse rolled",
            "Walls should be primed or use WG554 Liner Paper for best results",
            "Custom woven with set-up charge-5 yard minimum",
            "required for",
            "Original irregularities and imperfections have been duplicated",
            "Colorfastness To ",
            "Color Variation is Characteristic",
            "Delicate fabric",
            "Double Cloth",
            "Dye lots can vary",
            "Scotch Ingrain",
            "Centered within one yard",
            "Panel Effect is",
            "Recommend Stain-Off process before hanging this wallpaper",
            "A and B Rolls NUST be ordered in even increments",
            "Dyed from",
            "Artisan Hand-Crafted. Dots will vary",
            "BK Vertical",
            "BK Profile Series II",
            "See 30021MM",
            "Irregularly stagger the horizontal banding from panel to panel.",
            "Two motifs across the width.",
            "OLD WORLD WEAVERS / STARK FABRICS",
            "Do not paperback", 
            "Dry in The Shade",
            "direct application walls",
            "Printed on "
        };
        #endregion
    }
    */
}