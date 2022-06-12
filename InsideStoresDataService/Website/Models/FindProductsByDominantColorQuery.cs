using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Image search
    /// </summary>
    public class FindProductsByDominantColorQuery : ProductQueryBase
    {
        public System.Windows.Media.Color Color { get; set; }

        public FindProductsByDominantColorQuery(System.Windows.Media.Color color, QueryRequestMethods method)
        {
            Debug.Assert(method == QueryRequestMethods.FindByTopDominantColor || method == QueryRequestMethods.FindByAnyDominantColor);

            QueryMethod = method;
            this.Color = color;
        }
    }
}



