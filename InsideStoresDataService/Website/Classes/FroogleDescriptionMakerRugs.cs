using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{
    public class FroogleDescriptionMakerRugs : RugsDescriptionMakerBase
    {
        public FroogleDescriptionMakerRugs(InsideRugsProduct product)
            : base(product)
        {

            var list = new List<Action<PhraseManager>>()
            {
                ContributeIntro,
                ContributePostamble,
            };

            mgr.AddContributions(list);
        }


        #region Description Contributors

        private void ContributeIntro(PhraseManager mgr)
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
                add(Merge("{Style} {color} area rug by {Brand}."), 100);
                add(Merge("{Color} {style} area rug by {Brand}."), 100);
                add(Merge("{Adj} {style} {color} area rug by {Brand}."), 100);
                add(Merge("{Adj} {color} {style} area rug by {Brand}."), 100);
            }
            else if (hasStyle)
            {
                add(Merge("{Style} area rug by {Brand}."), 100);
                add(Merge("{Adj} {style} area rug by {Brand}."), 100);
            }
            else if (hasColor)
            {
                add(Merge("{Color} rug by {Brand}."), 100);
                add(Merge("{Adj} {color} rug by {Brand}."), 100);
            }
            else
            {
                add(Merge("{Name}."), 100);
            }
            
            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);

            if (hasOutdoor)
                mgr.AddPhrase("Indoor/outdoor.");

            mgr.EndPhraseSet();

            var list2 = new List<PhraseVariant>();
            currentList = list2;

            if (hasWeave && countSizes > 1)
            {
                add(Merge("{Weave}, available in {CountSizes}."), 100);
                add(Merge("Available in {CountSizes}. {Weave}."), 100);
            }
            else if (hasWeave && hasCountry)
            {
                add(Merge("{Weave}, made in {Country}."), 100);
                add(Merge("{Weave} in {Country}."), 100);
                add(Merge("This {adj2} rug is {weave} in {Country}."), 100);
                add(Merge("{Weave}."), 100);

            }
            else if (countSizes > 1 && hasCountry)
            {
                add(Merge("Available in {CountSizes}."), 100);
                add(Merge("Available in {CountSizes}. Made in {Country}."), 100);
                add(Merge("This {adj2} rug is available in {CountSizes}. Made in {Country}."), 50);
            }
            else if (countSizes > 1)
            {
                add(Merge("Available in {CountSizes}."), 100);

            } else if (hasCountry)
            {
                add(Merge("Made in {Country}."), 100);
            }

            if (list2.Count() > 0)
            {
                mgr.BeginPhraseSet();
                mgr.PickAndAddPhraseVariant(list2);
                mgr.EndPhraseSet();
            }
        }


        private void ContributePostamble(PhraseManager mgr)
        {
            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant("Free shipping available.", 50),
                new PhraseVariant("Fast delivery. Free shipping.", 100),
                new PhraseVariant("Free shipping. Fast delivery.", 100),
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();

            var list2 = new List<PhraseVariant>()
            {
                new PhraseVariant(string.Format("Item {0}.", product.p.SKU), 100),
                new PhraseVariant(string.Format("Our item {0}.", product.p.SKU), 100),
            };
            
            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list2);
            mgr.EndPhraseSet();

        }


        #endregion


    }
}
