using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace script_chan2.GUI
{
    public class NewChatDialogViewModel : Screen
    {
        private string newChatChannel;
        public string NewChatChannel
        {
            get { return newChatChannel; }
            set
            {
                if (value != newChatChannel)
                {
                    newChatChannel = value;
                    NotifyOfPropertyChange(() => NewChatChannel);
                    NotifyOfPropertyChange(() => OpenEnabled);
                }
            }
        }

        public bool OpenEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(NewChatChannel))
                    return false;
                return true;
            }
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        public void NewChatChannelEnter()
        {
            if (OpenEnabled)
                DialogHost.CloseDialogCommand.Execute(true, null);
        }
    }
}
