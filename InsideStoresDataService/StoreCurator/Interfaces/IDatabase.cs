using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public interface IDatabase
    {
        bool SetPretty(int productID, bool isPretty);
        bool SetPublished(int productID, bool isPublished);
        bool RemoveCategory(int productID, int categoryID);
        bool AddCategory(int productID, int categoryID);
    }
}