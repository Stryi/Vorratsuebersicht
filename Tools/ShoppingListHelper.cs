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

        public static void UnitTest()
        {
            //                                               MinQuantity  PrefQuantity IsQuantity ToBuy
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           0,           4)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           0,           3)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           0,           2)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           0,           1)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           0,           0)       == 0);

            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           6)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           5)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           4)       == 1);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           3)       == 2);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           2)       == 3);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           1)       == 4);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           0,           0)       == 5);

            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           9)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           8)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           7)       == 1);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           6)       == 2);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           5)       == 3);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           4)       == 4);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           3)       == 5);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           2)       == 6);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           1)       == 7);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(0,           8,           0)       == 8);

            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           9)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           8)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           7)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           6)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           5)       == 0);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           4)       == 4);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           3)       == 5);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           2)       == 6);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           1)       == 7);
            Trace.Assert(ShoppingListHelper.GetToBuyQuantity(5,           8,           0)       == 8);
        }
    }
}