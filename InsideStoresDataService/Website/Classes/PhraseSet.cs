using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website
{
    public class PhraseSet
    {
        private Random rand;
        private bool IsOrdered;
        private List<string> phrases;

        public PhraseSet(Random rand, bool IsOrdered)
        {
            this.rand = rand;
            this.IsOrdered = IsOrdered;
            phrases = new List<string>();
        }

        public void AddPhrase(string s)
        {
            phrases.Add(s);
        }

        public IEnumerable<string> Phrases
        {
            get
            {

                if (IsOrdered || phrases.Count <= 1)
                {
                    // if is ordered, or there is none or one in the list, 
                    // only course of action is to return the list exactly as is.
                    
                    foreach (var s in phrases)
                        yield return s;
                }
                else
                {
                    // pull in random order

                    var shadowList = new List<string>();
                    foreach (var s in phrases)
                        shadowList.Add(s);

                    while (shadowList.Count > 1)
                    {
                        var randomPick = rand.Next(0, shadowList.Count);
                        var s = shadowList[randomPick];
                        shadowList.RemoveAt(randomPick);
                        yield return s;
                    }

                    yield return shadowList[0];
                }
            }
        }

    }
}
