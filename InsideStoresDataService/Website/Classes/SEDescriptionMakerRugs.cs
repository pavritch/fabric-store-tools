using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{
    public class SEDescriptionMakerRugs : RugsDescriptionMakerBase
    {
        public SEDescriptionMakerRugs(InsideRugsProduct product) : base(product)
        {

            var list = new List<Action<PhraseManager>>()
            {
                ContributeIntro,
                ContributeOther,
                ContributePostamble,
            };

            mgr.AddContributions(list);
        }


        #region Description Contributors

        private void ContributeIntro(PhraseManager mgr)
        {
            var list1 = new List<PhraseVariant>();

            Action<string, int> add = (s, w) =>
            {
                // will have null input when a merge fails to resolve; so not used
                if (!string.IsNullOrWhiteSpace(s))
                    list1.Add(new PhraseVariant(s, w));
            };

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
        }


        private void ContributeOther(PhraseManager mgr)
        {


            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant("Search thousands of styles, sizes and colors.", 100),
                new PhraseVariant("Search thousands of area rugs in every imaginable style, shape and color.", 100),
                new PhraseVariant("Find thousands of rugs in every imaginable style and color.", 100),
                new PhraseVariant("Over 50,000 styles and colors.", 100),
                new PhraseVariant("Over 50,000 styles, sizes and colors.", 100),

                new PhraseVariant("Search thousands of designer area rugs.", 100),

                new PhraseVariant("Search thousands of luxury area rugs.", 100),
                new PhraseVariant("Find thousands of designer area rugs.", 100),
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();
        }


        private void ContributePostamble(PhraseManager mgr)
        {
            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant("Free shipping available.", 50),
                new PhraseVariant("Fast delivery. Free shipping.", 100),
                new PhraseVariant("Free shipping. Fast delivery.", 100),
            };

            var list2 = new List<PhraseVariant>()
            {
                new PhraseVariant(string.Format("Item {0}.", product.p.SKU), 100),
                new PhraseVariant(string.Format("SKU {0}.", product.p.SKU), 100),
            };
            
            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.PickAndAddPhraseVariant(list2);
            mgr.EndPhraseSet();
        }


        #endregion


    }
}
