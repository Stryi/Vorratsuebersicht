using System;
using System.Globalization;

using Android.App;
using Android.Text;
using Android.Widget;

namespace VorratsUebersicht
{
    class AddToShoppingListDialog
    {
        internal static void ShowDialog(Activity activity, int articleId, int? minQuantity = null, int? prefQuantity = null)
        {
            string quantityInfo = "";
            decimal toBuyQuantity = 0;

            int quantityInStorage = (int)Database.GetArticleQuantityInStorage(articleId);
            quantityInfo += string.Format("- Bestand: {0:#,0.######}\n", quantityInStorage);

            if ((minQuantity == null) || (prefQuantity == null))
            {
                ArticleData articleData = Database.GetArticleData(articleId);

                minQuantity  = articleData.MinQuantity;
                prefQuantity = articleData.PrefQuantity;
            }

            if (minQuantity  == null) minQuantity  = 0;
            if (prefQuantity == null) prefQuantity = 0;

            if (minQuantity > 0)
            {
                quantityInfo += string.Format("- Mindestmenge: {0:#,0.######}\n", minQuantity);
                toBuyQuantity = minQuantity.Value;
            }

            if (prefQuantity > 0)
            {
                quantityInfo += string.Format("- Bevorz. Menge: {0:#,0.######}\n", prefQuantity);
                toBuyQuantity = prefQuantity.Value;
            }

            if (quantityInStorage > 0)
            {
                toBuyQuantity = toBuyQuantity - quantityInStorage;
            }

            int shoppingListQuantiy = (int)Database.GetShoppingListQuantiy(articleId);
            quantityInfo += string.Format("- Auf Einkaufsliste: {0:#,0.######}\n", shoppingListQuantiy);

            // Auf Einkaufsliste ist ein höherer Betrag als ausgereichnet?
            if (shoppingListQuantiy > toBuyQuantity)
            {
                toBuyQuantity = shoppingListQuantiy;
            }

            if (toBuyQuantity == 0)
            {
                toBuyQuantity = 1;
            }

            string message = string.Format("{0}\nNeue Anzahl eingeben:", quantityInfo); 

            var quantityDialog = new AlertDialog.Builder(activity);
            quantityDialog.SetTitle("Auf Einkaufsliste setzen");
            quantityDialog.SetMessage(message);
            EditText input = new EditText(activity);
            input.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
            input.Text = toBuyQuantity.ToString();
            input.SetSelection(input.Text.Length);
            quantityDialog.SetView(input);
            quantityDialog.SetPositiveButton("OK", (dialog, whichButton) =>
                {
                    if (string.IsNullOrEmpty(input.Text))
                        input.Text = "0";

                    bool decialOk = Decimal.TryParse(input.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out toBuyQuantity);
                    if (decialOk)
                    {
                        if (toBuyQuantity == 0)
                        {
                            Database.RemoveFromShoppingList(articleId);
                        }
                        else
                        {
                            Database.SetShoppingItemQuantity(articleId, toBuyQuantity);
                        }
                    }
                });
            quantityDialog.SetNegativeButton("Cancel", (s, e) => {});
            quantityDialog.Show();
        }

    }
}