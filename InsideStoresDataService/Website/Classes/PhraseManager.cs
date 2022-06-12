using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Website
{
    public class PhraseManager
    {
        private Random rand;
        private readonly List<PhraseSet> phraseSets;
        private PhraseSet currentPhraseSet;

        /// <summary>
        /// This is the topmost level of phrase management.
        /// </summary>
        /// <remarks>
        /// The text seed can be anything. Used to allow deterministic repeatable
        /// sequences by passing in a same value - like a string of the productID.
        /// </remarks>
        /// <param name="textSeed"></param>
        public PhraseManager(string textSeed = null)
        {
            if (!string.IsNullOrEmpty(textSeed))
                rand = new Random(GetSeed(textSeed));
            else
                rand = new Random();

            phraseSets = new List<PhraseSet>();
            currentPhraseSet = null;
        }

        public PhraseManager(Random rand)
        {
            if (rand == null)
                throw new ArgumentNullException();

            this.rand = rand;

            phraseSets = new List<PhraseSet>();
            currentPhraseSet = null;
        }

        public int GetRandomIndex(int min, int max)
        {
            return rand.Next(min, max+1);
        }

        public void BeginPhraseSet(bool IsOrdered=false)
        {
            // the current phrase set is always the last element in the collection

            var newPhraseSet = new PhraseSet(rand, IsOrdered);
            phraseSets.Add(newPhraseSet);
            currentPhraseSet = newPhraseSet;
        }

        public void EndPhraseSet()
        {
            currentPhraseSet = null;
        }

        public void AddPhrase(string s)
        {
            if (currentPhraseSet == null)
                BeginPhraseSet();

            currentPhraseSet.AddPhrase(s);
        }

        public override string ToString()
        {
            EndPhraseSet();

            var sb = new StringBuilder(1024);

            int countPhrases = 0;

            foreach(var phraseSet in phraseSets)
            {
                foreach (var phrase in phraseSet.Phrases)
                {
                    // need space between phrases

                    if (countPhrases > 0)
                        sb.Append(" ");

                    sb.Append(phrase);
                    countPhrases++;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Call a series of methods which can contribute to the output.
        /// </summary>
        /// <remarks>
        /// Each method takes a reference to the phrase manager so it can
        /// selectively choose to contribute zero or more phrases in any
        /// combination of phrase sets.
        /// </remarks>
        /// <param name="methods"></param>
        public void AddContributions(List<Action<PhraseManager>> methods)
        {
            foreach (var method in methods)
            {
                method(this);
            }
        }

        /// <summary>
        /// Pick a variant from the candidates - randomly based on weight.
        /// </summary>
        /// <param name="variants"></param>
        /// <returns></returns>
        public string PickPhraseVariant(IEnumerable<PhraseVariant> variants)
        {
            if (variants.Count() == 1)
                return variants.First().Phrase;

            var totalWeight = variants.Sum(e => e.Weight);

            var pickIndex = rand.Next(1, totalWeight+1);

            // spin through and figure out which one gets picked

            int currentWeight = 0;
            foreach (var variant in variants)
            {
                if (pickIndex > currentWeight + variant.Weight)
                {
                    currentWeight += variant.Weight;
                    continue;
                }

                return variant.Phrase;
            }

            throw new Exception("Unexpected fall through loop in PickPhraseVariant().");
        }


        /// <summary>
        /// Pick a variant from the candidates - randomly based on weight.
        /// </summary>
        /// <param name="variants"></param>
        /// <returns></returns>
        public void PickAndAddPhraseVariant(IEnumerable<PhraseVariant> variants)
        {
            if (variants.Count() == 1)
            {
                AddPhrase(variants.First().Phrase);
                return;
            }

            var totalWeight = variants.Sum(e => e.Weight);

            var pickIndex = rand.Next(1, totalWeight+1);

            // spin through and figure out which one gets picked

            int currentWeight = 0;
            foreach (var variant in variants)
            {
                if (pickIndex > currentWeight + variant.Weight)
                {
                    currentWeight += variant.Weight;
                    continue;
                }

                AddPhrase(variant.Phrase);
                return;
            }

            throw new Exception("Unexpected fall through loop in PickPhraseVariant().");
        }

        public static int GetSeed(string textSeed)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(textSeed);

            var hashString = new SHA1Managed();
            int result = 0;

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                result += x;

            return result;
        }
    }
}
