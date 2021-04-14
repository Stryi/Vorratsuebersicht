using System;
using Android.Content;

namespace VorratsUebersicht
{
    internal class ActionDismissListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        private readonly Action action;

        public ActionDismissListener(Action action)
        {
            this.action = action;
        }

        public void OnCancel(IDialogInterface dialog)
        {
            this.action();
        }
    }
}