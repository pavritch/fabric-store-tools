using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace StoreCurator
{
    public static class HtmlHelpers
    {

        #region DisplayManufacturers
        public static MvcHtmlString DisplayManufacturers(List<ManufacturerMenuItem> list)
        {
            // <a href="#" data-manufacturerID="1" class="list-group-item active">Text-Name-Here</a>

            var sb = new StringBuilder();

            var isFirst = true;
            foreach (var item in list)
            {
                sb.AppendFormat("<a href=\"#\" data-manufacturerid=\"{0}\" class=\"list-group-item{1}\">{2}</a>", item.ManufacturerID, (isFirst ?  " active" : string.Empty), HttpUtility.HtmlEncode(item.Name));
                isFirst = false;
            }

            return new MvcHtmlString(sb.ToString());

        }

        public static MvcHtmlString DisplayManufacturers(this HtmlHelper helper, List<ManufacturerMenuItem> list)
        {
            return DisplayManufacturers(list);
        } 
        #endregion

        #region DisplayCategories
        public static MvcHtmlString DisplayCategories(List<CategoryMenuItem> list)
        {
            //<a href="#item-1" class="list-group-item active" data-toggle="collapse" >
            //    <i class="glyphicon glyphicon-chevron-right"></i>Item 1
            //</a>
            //<div class="list-group collapse" id="item-1">
            //
            //    <a href="#item-1-1" class="list-group-item" data-toggle="collapse">
            //        <i class="glyphicon glyphicon-chevron-right"></i>Item 1.1
            //    </a>
            //    <div class="list-group collapse" id="item-1-1">
            //        <a href="#" class="list-group-item">Item 1.1.1</a>
            //        <a href="#" class="list-group-item">Item 1.1.2</a>
            //        <a href="#" class="list-group-item">Item 1.1.3</a>
            //    </div>

            //    <a href="#item-1-2" class="list-group-item" data-toggle="collapse">
            //        <i class="glyphicon glyphicon-chevron-right"></i>Item 1.2
            //    </a>
            //    <div class="list-group collapse" id="item-1-2">
            //        <a href="#" class="list-group-item">Item 1.2.1</a>
            //        <a href="#" class="list-group-item">Item 1.2.2</a>
            //        <a href="#" class="list-group-item">Item 1.2.3</a>
            //    </div>

            //    <a href="#item-1-3" class="list-group-item" data-toggle="collapse">
            //        <i class="glyphicon glyphicon-chevron-right"></i>Item 1.3
            //    </a>
            //    <div class="list-group collapse" id="item-1-3">
            //        <a href="#" class="list-group-item">Item 1.3.1</a>
            //        <a href="#" class="list-group-item">Item 1.3.2</a>
            //        <a href="#" class="list-group-item">Item 1.3.3</a>
            //    </div>
            //
            //</div>


            var sb = new StringBuilder();

            var isFirst = true;

            Func<int[], string> makeIdentifier = (indexes) =>
                {
                    // depth should be 1 or greater
                    var s = "item";
                    for (var i = 0; i < indexes.Length; i++)
                        s += string.Format("-{0}", indexes[i]);

                    return s;
                };

            Func<CategoryMenuItem, int[], string> makeMenuItem = (cat, indexes2) =>
                {
                    string s;
                    // style=\"margin-left:{4}px\"
                    var leftMargin = (indexes2.Length - 1) * 20;
                    if (cat.Children.Count() > 0)
                        s = string.Format("<a href=\"#{2}\" data-categoryid=\"{3}\" class=\"list-group-item {1}\" data-toggle=\"collapse\" ><span style=\"margin-left:{4}px\"></span><i class=\"glyphicon glyphicon-chevron-right\"></i>{0}</a>", HttpUtility.HtmlEncode(cat.Name), (isFirst ? " active" : string.Empty), makeIdentifier(indexes2), cat.CategoryID, leftMargin);
                    else
                        s = string.Format("<a href=\"#{2}\" data-categoryid=\"{3}\" class=\"list-group-item {1}\" ><span style=\"margin-left:{4}px\"></span>{0}</a>", HttpUtility.HtmlEncode(cat.Name), (isFirst ? " active" : string.Empty), makeIdentifier(indexes2), cat.CategoryID, leftMargin);

                    isFirst = false;
                    return s;
                };
            

            Action<CategoryMenuItem, int[]> processNode = null;
            processNode = (item3, indexes3) =>
                {
                    sb.Append(makeMenuItem(item3, indexes3));

                    if (item3.Children.Count() == 0)
                        return;

                    // children
                    sb.AppendFormat("<div class=\"list-group collapse\" id=\"{0}\">", makeIdentifier(indexes3));
                    var childIndexes = new int[indexes3.Length + 1];
                    for (int j = 0; j < indexes3.Length; j++)
                        childIndexes[j] = indexes3[j];
                    childIndexes[indexes3.Length] = 1;
                    foreach (var child in item3.Children)
                    {
                        processNode(child, childIndexes);
                        childIndexes[indexes3.Length]++;
                    }
                    sb.Append("</div>");
                };

            var index = 1;
            foreach (var item in list)
                processNode(item, new int[] {index++});

            return new MvcHtmlString(sb.ToString());

        }

        public static MvcHtmlString DisplayCategories(this HtmlHelper helper, List<CategoryMenuItem> list)
        {
            return DisplayCategories(list);
        }
        #endregion


        #region Pager
        public static MvcHtmlString Pager(int pageNumber, int totalPages)
        {
            var pgr = new Pager();
            var html = pgr.GetHtml(pageNumber, totalPages);

            return new MvcHtmlString(html);

        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int pageNumber, int totalPages)
        {
            return Pager(pageNumber, totalPages);
        }
        #endregion

    }
}


    