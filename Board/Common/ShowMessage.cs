using Board.Controls;

namespace Board.Common
{
    public static class ShowMessage
    {
        public static void Show(string title, string message)
        {
            NotifyWindow notify = new NotifyWindow { Title = title, MessageText = message };
            notify.Show();
        }

        public static void Show(string message)
        {
            NotifyWindow notify = new NotifyWindow { MessageText = message };
            notify.Show();
        }
    }
}
