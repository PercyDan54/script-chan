﻿using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteWebhookDialogViewModel : Screen
    {
        #region Constructor
        public DeleteWebhookDialogViewModel(Webhook webhook)
        {
            this.webhook = webhook;
        }
        #endregion

        #region Properties
        private Webhook webhook;

        public string Name
        {
            get { return webhook.Name; }
        }

        public string Label
        {
            get { return string.Format(Properties.Resources.DeleteWebhookDialogView_LabelText, Name); }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
