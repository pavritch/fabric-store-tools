using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Website
{
    public class SEDescriptionMakerWallpaper : WallpaperDescriptionMakerBase
    {
        public SEDescriptionMakerWallpaper(InsideWallpaperProduct product)
            : base(product)
        {

            List<Action<PhraseManager>> list = null;
            list = WallpaperMethods();

            mgr.AddContributions(list);
        }

        #region Action Method Lists

        private List<Action<PhraseManager>> WallpaperMethods()
        {
            var list = new List<Action<PhraseManager>>()
            {
                ContributeWallpaperIntro,
                ContributeWallpaperDesigner,
                ContributeWallpaperOther,
                ContributeWallpaperPostamble,
            };

            return list;
        }
        #endregion

        #region Description Contributors


        private void ContributeWallpaperIntro(PhraseManager mgr)
        {
            List<PhraseVariant> currentList = null;

            Action<string, int> add = (s, w) =>
            {
                // will have null input when a merge fails to resolve; so not used
                if (!string.IsNullOrWhiteSpace(s))
                    currentList.Add(new PhraseVariant(s, w));
            };

            var list1 = new List<PhraseVariant>();

            currentList = list1;

            if (hasStyle && hasColor)
            {
                add(Merge("{Style} {color} {kind} by {Brand}."), 20);
                add(Merge("{Color} {style} {kind} by {Brand}."), 20);
                add(Merge("{Adj2} {style} {color} {kind} by {Brand}."), 100);
                add(Merge("{Adj2} {color} {style} {kind} by {Brand}."), 100);
            }
            else if (hasStyle)
            {
                add(Merge("{Style} {kind} by {Brand}."), 20);
                add(Merge("{Adj2} {kind} by {Brand}."), 100);
            }
            else if (hasColor)
            {
                add(Merge("{Color} {kind} by {Brand}."), 10);
                add(Merge("{Adj2} {color} {kind} by {Brand}."), 100);
            }


            var list2 = new List<PhraseVariant>();

            currentList = list2;

            if (hasMPN && (hasColor | hasStyle))
            {
                add(Merge("Item {MPN}. Free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Free shipping on {Brand} products."), 100);
                add(Merge("Item {MPN}. Free shipping on {Brand} luxury wallpaper."), 100);
                add(Merge("Item {MPN}. Free shipping on {Brand} designer wallpaper."), 100);
                add(Merge("Item {MPN}. Fast, free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Fast, free shipping on {Brand} fabric."), 100);
                add(Merge("Item {MPN}. Best prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Discount pricing and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Low prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Low prices and free shipping on {Brand} products."), 100);
                add(Merge("Item {MPN}. Low prices and free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Low prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Low prices and fast free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Best prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Best prices and free shipping on {Brand} products."), 100);
                add(Merge("Item {MPN}. Best prices and free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Best prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Best prices and fast free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Lowest prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Lowest prices and free shipping on {Brand} products."), 100);
                add(Merge("Item {MPN}. Lowest prices and free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Lowest prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Item {MPN}. Lowest prices and fast free shipping on {Brand}."), 100);
                add(Merge("Item {MPN}. Save on {Brand} wallpaper. Free shipping!"), 100);
                add(Merge("Item {MPN}. Save on {Brand} products. Free shipping!"), 100);
                add(Merge("Item {MPN}. Save on {Brand} luxury wallpaper. Free shipping!"), 100);
                add(Merge("Item {MPN}. Save big on {Brand} wallpaper. Free shipping!"), 100);
                add(Merge("Item {MPN}. Save big on {Brand}. Free shipping!"), 100);
                add(Merge("Item {MPN}. Save on {Brand}. Big discounts and free shipping!"), 100);
            }
            else
            {
                add(Merge("Free shipping on {Brand}."), 100);
                add(Merge("Free shipping on {Brand} wallpaper."), 100);
                add(Merge("Free shipping on {Brand} products."), 100);
                add(Merge("Free shipping on {Brand} luxury wallpaper."), 100);
                add(Merge("Free shipping on {Brand} designer wallpaper."), 100);
                add(Merge("Fast, free shipping on {Brand}."), 100);
                add(Merge("Fast, free shipping on {Brand} fabric."), 100);
                add(Merge("Best prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Discount pricing and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Low prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Low prices and free shipping on {Brand} products."), 100);
                add(Merge("Low prices and free shipping on {Brand}."), 100);
                add(Merge("Low prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Low prices and fast free shipping on {Brand}."), 100);
                add(Merge("Best prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Best prices and free shipping on {Brand} products."), 100);
                add(Merge("Best prices and free shipping on {Brand}."), 100);
                add(Merge("Best prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Best prices and fast free shipping on {Brand}."), 100);
                add(Merge("Lowest prices and free shipping on {Brand} wallpaper."), 100);
                add(Merge("Lowest prices and free shipping on {Brand} products."), 100);
                add(Merge("Lowest prices and free shipping on {Brand}."), 100);
                add(Merge("Lowest prices and fast free shipping on {Brand} wallpaper."), 100);
                add(Merge("Lowest prices and fast free shipping on {Brand}."), 100);
                add(Merge("Save on {Brand} wallpaper. Free shipping!"), 100);
                add(Merge("Save on {Brand} products. Free shipping!"), 100);
                add(Merge("Save on {Brand} luxury wallpaper. Free shipping!"), 100);
                add(Merge("Save big on {Brand} wallpaper. Free shipping!"), 100);
                add(Merge("Save big on {Brand}. Free shipping!"), 100);
                add(Merge("Save on {Brand}. Big discounts and free shipping!"), 100);
            }


            if (list1.Count() > 0)
            {
                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list1);
                mgr.EndPhraseSet();
            }

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list2);
            mgr.EndPhraseSet();
        }


        private void ContributeWallpaperOther(PhraseManager mgr)
        {
            var list2 = new List<PhraseVariant>()
            {
                new PhraseVariant("Search thousands of patterns.", 100),
                new PhraseVariant("Search thousands of wallpaper patterns.", 100),
                new PhraseVariant("Find thousands of patterns.", 100),

                new PhraseVariant("Search thousands of designer walllpapers.", 100),
                new PhraseVariant("Find thousands of designer patterns.", 100),

                new PhraseVariant("Search thousands of luxury wallpapers.", 100),
                new PhraseVariant("Find thousands of luxury patterns.", 100),
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list2);
            mgr.EndPhraseSet();
        }


        private void ContributeWallpaperPostamble(PhraseManager mgr)
        {
            List<PhraseVariant> currentList = null;

            Action<string, int> add = (s, w) =>
            {
                // will have null input when a merge fails to resolve; so not used
                if (!string.IsNullOrWhiteSpace(s))
                    currentList.Add(new PhraseVariant(s, w));
            };

            var list1 = new List<PhraseVariant>();
            currentList = list1;

            add(Merge("Sold by the roll."), 50);
            add(Merge("Swatches available."), 100);

            var list2 = new List<PhraseVariant>();
            currentList = list2;

            if (hasMPN)
            {
                if (hasWidth)
                    add(Merge("Width {width}."), 100);
            }
            else
                add(Merge("SKU {SKU}."), 100);

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            if (list2.Count() > 0)
                mgr.PickAndAddPhraseVariant(list2);
            mgr.EndPhraseSet();
        }


        private void ContributeWallpaperDesigner(PhraseManager mgr)
        {
            // contribute single sentence about designer.

            var designer = product.Designer;
            if (designer == null)
                return;

            Func<string, string> phrase = (s) =>
            {
                return string.Format("Featuring {0}.", designer);
            };

            var list = new List<PhraseVariant>()
            {
                new PhraseVariant(phrase("{0} collection."), 100),
                new PhraseVariant(phrase("Featuring {0}."), 100),
                new PhraseVariant(phrase("Featuring {0} and many other top designers."), 100),
                new PhraseVariant(phrase("Featuring {0} and many other designers."), 100),
                new PhraseVariant(phrase("{0} and many other top designers."), 100),
                new PhraseVariant(phrase("{0} and many other designers."), 100),
                new PhraseVariant(phrase("Featuring the {0} collection."), 100),
                new PhraseVariant(phrase("Search the entire {0} collection."), 100),
                new PhraseVariant(phrase("See the entire {0} collection."), 100),
                new PhraseVariant(phrase("Find the entire {0} collection."), 100),
                new PhraseVariant(phrase("Featuring the {0} collection and many other designers."), 20),
                new PhraseVariant(phrase("Featuring the {0} collection and many other top designers."), 20),
                new PhraseVariant(phrase("Featuring the {0} collection and many other popular designers."), 20),
                new PhraseVariant(phrase("Search the entire {0} collection and thousands of popular patterns."), 20),
                new PhraseVariant(phrase("Search the entire {0} collection and thousands of other popular wallpapers."), 20),
                new PhraseVariant(phrase("See the entire {0} collection and thousands of designer patterns."), 20),
                new PhraseVariant(phrase("See the entire {0} collection and thousands of designer fabrics."), 20),
                new PhraseVariant(phrase("Find the entire {0} collection along with thousands of top designer wallpapers"), 20),
                new PhraseVariant(phrase("Featuring {0} and many other designers."), 20),
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list);
            mgr.EndPhraseSet();
        }


        #endregion


    }
}
