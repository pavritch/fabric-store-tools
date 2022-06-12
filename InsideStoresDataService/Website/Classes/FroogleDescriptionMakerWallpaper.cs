using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{
    public class FroogleDescriptionMakerWallpaper : WallpaperDescriptionMakerBase
    {
        public FroogleDescriptionMakerWallpaper(InsideWallpaperProduct product)
            : base(product)
        {
            var list = new List<Action<PhraseManager>>();

            list.Add(ContributeIntroWallcovering);
            list.Add(ContributePostamble);

            mgr.AddContributions(list);
        }


        #region Description Contributors


        private void ContributeIntroWallcovering(PhraseManager mgr)
        {
            List<PhraseVariant> currentList = null;

            var list1 = new List<PhraseVariant>();

            Action<string, int> add = (s, w) =>
            {
                // will have null input when a merge fails to resolve; so not used
                if (!string.IsNullOrWhiteSpace(s))
                    currentList.Add(new PhraseVariant(s, w));
            };

            currentList = list1;
            if (hasStyle && hasColor)
            {
                add(Merge("{Style} {color} {kind} by {Brand}."), 20);
                add(Merge("{Color} {style} {kind} by {Brand}."), 20);
                add(Merge("{Adj} {style} {color} {kind} by {Brand}."), 100);
                add(Merge("{Adj} {color} {style} {kind} by {Brand}."), 100);
            }
            else if (hasStyle)
            {
                add(Merge("{Style} {kind} by {Brand}."), 20);
                add(Merge("{Adj} {kind} by {Brand}."), 100);
            }
            else if (hasColor)
            {
                add(Merge("{Color} {kind} by {Brand}."), 10);
                add(Merge("{Adj} {color} {kind} by {Brand}."), 100);
            }
            else
            {
                add(Merge("{Name}."), 100);
            }

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();

            if (hasDesigner)
            {
                mgr.BeginPhraseSet();
                mgr.AddPhrase(Merge("Designed by {Designer}."));
                mgr.EndPhraseSet();
            }

            if (hasPattern)
            {
                mgr.BeginPhraseSet();
                mgr.AddPhrase(Merge("Pattern {Pattern}."));
                mgr.EndPhraseSet();
            }

            if (hasWidth)
            {
                mgr.BeginPhraseSet();
                mgr.AddPhrase(Merge("Width {width}."));
                mgr.EndPhraseSet();
            }
        }


        private void ContributePostamble(PhraseManager mgr)
        {

            if (hasMPN)
            {
                mgr.BeginPhraseSet();
                mgr.AddPhrase(Merge("{Brand} item {MPN}."));
                mgr.EndPhraseSet();
            }
            else
            {
                var list2 = new List<PhraseVariant>()
                {
                    new PhraseVariant(string.Format("Item {0}.", product.p.SKU), 100),
                    new PhraseVariant(string.Format("Our item {0}.", product.p.SKU), 100),
                };

                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list2);
                mgr.EndPhraseSet();
            }

            if (hasCountry)
            {
                mgr.BeginPhraseSet();
                mgr.AddPhrase(Merge("Made in {Country}."));
                mgr.EndPhraseSet();
            }

            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant("Free shipping available.", 50),
                new PhraseVariant("Fast delivery. Free shipping.", 100),
                new PhraseVariant("Free shipping. Fast delivery.", 100),
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();



        }


        #endregion


    }
}
