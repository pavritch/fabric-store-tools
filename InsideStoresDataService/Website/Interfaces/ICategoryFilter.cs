using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Website.Emails;

namespace Website
{
    public interface ICategoryFilter<TProduct> where TProduct : class
    {
        int CategoryID { get; }
        string Name { get; }
        Guid CategoryGUID { get; }
        void Initialize();
        void Classify(TProduct product);
    }
}