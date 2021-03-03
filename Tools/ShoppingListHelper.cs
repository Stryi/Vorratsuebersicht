using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    public class ShoppingListHelper
    {
        public static int GetToBuyQuantity(int minQuantity, int prefQuantity, int isQuantity)
        {
            int toBuy = 0;

            if (minQuantity > 0)
            {
                if (isQuantity < minQuantity)
                {
                    if (prefQuantity > 0)
                    {
                        toBuy = prefQuantity - isQuantity;
                    }
                    else
                    {
                        toBuy = minQuantity - isQuantity;
                    }
                }
                return toBuy;
            }

            if (prefQuantity > 0)
            {
                if (isQuantity < prefQuantity)
                {
                    toBuy = prefQuantity - isQuantity;
                }
                return toBuy;
            }

            return toBuy;
        }
    }
}