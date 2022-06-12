using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Website
{
    public class InsideFabricAnchorTextSpinner
    {
        #region PhraseManagerVariant

        public class PhraseManagerVariant
        {
            public Func<Random, PhraseManager> Function { get; set; }
            public int Weight { get; set; }

            public PhraseManagerVariant(Func<Random, PhraseManager> Function, int Weight = 1)
            {
                if (Weight == 0)
                    throw new Exception("Weight cannot be 0.");

                this.Function = Function;
                this.Weight = Weight;
            }
        }

        #endregion

        private Random rand;

        readonly InsideFabricProduct product;

        public InsideFabricAnchorTextSpinner(InsideFabricProduct product)
        {
            this.product = product;
        }

        /// <summary>
        /// Spin up N anchor text links.
        /// </summary>
        /// <remarks>
        /// The prior known (but not guaranteed) count is provided in case it helps with the generation strategy.
        /// </remarks>
        /// <param name="count">Must return this many spun anchors.</param>
        /// <param name="priorCount">Hint of how many likely previously spun up.</param>
        /// <returns></returns>
        public IEnumerable<string> SpinAnchorText(int count, int priorCount)
        {
            rand = new Random(PhraseManager.GetSeed(product.SKU + ":" + priorCount.ToString() + ":" + DateTime.Now.Ticks.ToString()));

            for(int i=0; i < count; i++)
                yield return GetRandomPhraseManager().ToString();
        }

        /// <summary>
        /// As we make each pick, the concept for the various choices vary widely, so we
        /// randomly pick a function and let it do all the work. Each function is free to use whatever simple or complex
        /// methodology it chooses to come up with a primed phrase manager.
        /// </summary>
        /// <returns></returns>
        private PhraseManager GetRandomPhraseManager()
        {
            var list = new PhraseManagerVariant[]
            {
                new PhraseManagerVariant(ProductNamePhraseMgr, 100),
                new PhraseManagerVariant(SimplePhrases, 100),
            };

            var function = PickPhraseGenerator(list);

            if (function == null)
                throw new NullReferenceException("GetRandomPhraseManager");

            return function(rand);
        }

        private PhraseManager ProductNamePhraseMgr(Random r)
        {
            var mgr = new PhraseManager(r);
            mgr.AddPhrase("My Name");
            return mgr;
        }

        private PhraseManager SimplePhrases(Random r)
        {
            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant("one", 100),
                new PhraseVariant("two", 100),
                new PhraseVariant("three", 100),
            };

            var mgr = new PhraseManager(r);
            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();

            return mgr;
        }


        /// <summary>
        /// Given a list of functions that know how to yield a phrase manager and respective weights - pick one.
        /// </summary>
        /// <remarks>
        /// The function will be invoked with the global PRNG and it is expected to yield a phrase manager ready
        /// to go - which can then have ToString() called to return some resulting phrase.
        /// </remarks>
        /// <param name="variants"></param>
        /// <returns></returns>
        private Func<Random, PhraseManager> PickPhraseGenerator(IEnumerable<PhraseManagerVariant> variants)
        {
            var totalWeight = variants.Sum(e => e.Weight);

            var pickIndex = rand.Next(1, totalWeight + 1);

            // spin through and figure out which one gets picked

            int currentWeight = 0;
            foreach (var variant in variants)
            {
                if (pickIndex > currentWeight + variant.Weight)
                {
                    currentWeight += variant.Weight;
                    continue;
                }

                return variant.Function;
            }

            throw new Exception("Unexpected fall through loop in PickPhraseGenerator().");
        }



    }
}
