using System;
using System.Globalization;

using Android.App;
using Android.Text;
using Android.Text.Method;
using Android.Widget;

namespace VorratsUebersicht
{
    class AddToShoppingListDialog
    {
        internal static void ShowDialog(Activity activity, int articleId, int? minQuantity = null, int? prefQuantity = null, Action refreshListAction = null)
        {
            string quantityInfo = "";
            decimal toBuyQuantity = 0;

            decimal quantityInStorage = Database.GetArticleQuantityInStorage(articleId);
            quantityInfo += string.Format(CultureInfo.CurrentUICulture, "- Bestand: {0:#,0.######}\n", quantityInStorage);

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

            decimal shoppingListQuantiy = Database.GetShoppingListQuantiy(articleId);
            quantityInfo += string.Format(CultureInfo.CurrentUICulture, "- Auf Einkaufsliste: {0:#,0.######}\n", shoppingListQuantiy);

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

            var b = new AlertDialog.Builder(activity);
            var quantityDialog = b.Create();
            quantityDialog.Window.SetSoftInputMode(Android.Views.SoftInput.StateVisible);
            quantityDialog.SetTitle("Auf Einkaufsliste setzen");
            quantityDialog.SetMessage(message);
            EditText input = new EditText(activity);
            input.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
            input.Text = toBuyQuantity.ToString(CultureInfo.InvariantCulture);

            input.RequestFocus();
            input.SetSelection(0, input.Text.Length);
            quantityDialog.SetView(input);
            quantityDialog.SetButton("OK", (dialog, whichButton) =>
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
                        refreshListAction?.Invoke();
                    }
                });
            quantityDialog.SetButton2("Cancel", (s, e) => {});
            quantityDialog.Show();
        }
    }
}