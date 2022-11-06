using System;
using System.Globalization;

using Android.App;
using Android.Text;
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
            quantityInfo += string.Format(CultureInfo.CurrentUICulture, "{0} {1:#,0.######}\n",
                activity.Resources.GetString(Resource.String.ToShoppingList_Inventory),
                quantityInStorage);

            if ((minQuantity == null) || (prefQuantity == null))
            {
                Article articleData = Database.GetArticleData(articleId);

                minQuantity  = articleData.MinQuantity;
                prefQuantity = articleData.PrefQuantity;
            }

            if (minQuantity  == null) minQuantity  = 0;
            if (prefQuantity == null) prefQuantity = 0;

            if (minQuantity > 0)
            {
                quantityInfo += string.Format("{0} {1:#,0.######}\n", activity.Resources.GetString(Resource.String.ToShoppingList_MinQuantity), minQuantity);
                toBuyQuantity = minQuantity.Value;
            }

            if (prefQuantity > 0)
            {
                quantityInfo += string.Format("{0} {1:#,0.######}\n", activity.Resources.GetString(Resource.String.ToShoppingList_PrefQuantity), prefQuantity);
                toBuyQuantity = prefQuantity.Value;
            }

            if (quantityInStorage > 0)
            {
                toBuyQuantity = toBuyQuantity - quantityInStorage;
            }

            decimal shoppingListQuantiy = Database.GetShoppingListQuantiy(articleId);
            quantityInfo += string.Format(CultureInfo.CurrentUICulture, "{0} {1:#,0.######}\n", activity.Resources.GetString(Resource.String.ToShoppingList_ToShoppingList), shoppingListQuantiy);

            // Auf Einkaufsliste ist ein höherer Betrag als ausgereichnet?
            if (shoppingListQuantiy > toBuyQuantity)
            {
                toBuyQuantity = shoppingListQuantiy;
            }

            if (toBuyQuantity == 0)
            {
                toBuyQuantity = 1;
            }

            string message = string.Format("{0}\n{1}", quantityInfo, activity.Resources.GetString(Resource.String.ToShoppingList_EnterNewQuantity)); 

            var b = new AlertDialog.Builder(activity);
            var quantityDialog = b.Create();
            quantityDialog.Window.SetSoftInputMode(Android.Views.SoftInput.StateVisible);
            quantityDialog.SetTitle(activity.Resources.GetString(Resource.String.ToShoppingList_Title));
            quantityDialog.SetMessage(message);
            EditText input = new EditText(activity);
            input.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
            input.Text = toBuyQuantity.ToString(CultureInfo.InvariantCulture);

            input.RequestFocus();
            input.SetSelection(0, input.Text.Length);
            quantityDialog.SetView(input);
            quantityDialog.SetButton(activity.Resources.GetString(Resource.String.App_Ok), (dialog, whichButton) =>
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
            quantityDialog.SetButton2(activity.Resources.GetString(Resource.String.App_Cancel), (s, e) => {});
            quantityDialog.Show();
        }
    }
}