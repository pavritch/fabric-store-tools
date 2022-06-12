using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace StoreCurator
{

    public class Pager
    {
        // call like this:
        // var pager = new Pager();
        // return pager.GetHtml(sBaseURL, currentPage, totalPages);

        private const string LeftArrow = "&#x25C4"; // 25C4  25C0
        private const string RightArrow = "&#x25BA"; // 25BA   25B6

        public string GetHtml(int pageNumber, int totalPages)
        {
            var sb = new StringBuilder(1000);

            var pageButtons = FigureOutPageNumbers(pageNumber, totalPages);

            // need to deal with if was a search - just detect and grab the phrase from request context

            sb.Append("<div class=\"pgrButtonBar\">");

            //<a class="pgrButton" href="#">2</a>
            //<a class="pgrButton pgrButtonActive" href="#">1</a>

            //sb.Append("<a class=\"pgrButton\" href=\"#\" style=\"padding-left:10pt;padding-right:10px;\">More</a>");

            // output buttons from right to left


            if (pageNumber < totalPages)
                sb.Append(MakeNextButton(pageNumber + 1));
            else
                sb.Append(MakeNextButton());

            int? lastPageAdded = null;
            foreach (var pgNum in pageButtons.OrderByDescending(e => e))
            {
                if (lastPageAdded.HasValue && pgNum < (lastPageAdded - 1))
                    sb.Append(MakeBullet());

                sb.Append(MakePageNumberButton(pgNum, pgNum == pageNumber));

                lastPageAdded = pgNum;
            }

            if (pageNumber > 1)
                sb.Append(MakePreviousButton(pageNumber - 1));
            else
                sb.Append(MakePreviousButton());

            sb.Append("<div style=\"clear:both;\"></div></div>");

            return sb.ToString();
        }

        private HashSet<int> FigureOutPageNumbers(int currentPage, int totalPages)
        {
            // caller will sort list as needed for final output

            const int coreButtonCount = 15;
            var list = new HashSet<int>();

            if (totalPages <= coreButtonCount)
            {
                for (int i = 1; i <= Math.Min(totalPages, coreButtonCount); i++)
                    list.Add(i);
            }
            else if (currentPage > totalPages - coreButtonCount)
            {
                list.Add(1);
                for (int i = totalPages - (coreButtonCount - 1); i <= totalPages; i++)
                    list.Add(i);
            }
            else
            {
                list.Add(1);
                list.Add(totalPages);

                var group = currentPage / coreButtonCount;

                if ((currentPage % coreButtonCount) == 0)
                    group = Math.Max(0, group - 1);

                var startingPage = (group * coreButtonCount) + 1;

                for (int i = startingPage; i < startingPage + coreButtonCount; i++)
                    list.Add(i);
            }

            return list;
        }

        private string MakeBullet()
        {
            return "<div class=\"pgrButtonBullet\">&#x25CF</div>";
        }


        private string MakePageNumberButton(int pageNumber, bool isActive = false)
        {
            return string.Format("<a class=\"pgrButton{1}\" href=\"#\" data-targetpage=\"{0}\" >{0}</a>",  pageNumber, isActive ? " pgrButtonActive" : string.Empty);
        }

        private string MakeNavButton(string icon, int? targetPage)
        {
            // if target null, then will be disabled button

            if (!targetPage.HasValue)
                return "<a class=\"pgrButton pgrButtonNav pgrButtonDisabled\"><div>" + icon + "</div></a>";

            return string.Format("<a class=\"pgrButton pgrButtonNav\" href=\"#\" data-targetpage=\"{0}\" ><div>" + icon + "</div></a>", targetPage.Value);
        }
        private string MakePreviousButton(int? targetPage=null)
        {
            return MakeNavButton(LeftArrow, targetPage);
        }

        private string MakeNextButton(int? targetPage=null)
        {
            return MakeNavButton(RightArrow, targetPage);
        }


    }
}