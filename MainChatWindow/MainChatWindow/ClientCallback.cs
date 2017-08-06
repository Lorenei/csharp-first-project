using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using ChatInterfaces;
using System.Windows.Media;

namespace ChatClient {

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ClientCallback : IClient {
        MainWindow mainWindowReference;

        public ClientCallback()
        {
            //Create reference to MainWindow which is chat main window and store it in variable.
            mainWindowReference = ((MainWindow)Application.Current.Windows.OfType<MainWindow>().FirstOrDefault());
        }

        public void GetMessage(string message, string userName) {
            mainWindowReference.AddMessageToFlowDocument(message, userName);
        }

        public void GetUsersList(Dictionary<string, string> usersList)
        {
            mainWindowReference.RefreshUsersList(usersList);
        }

        public void GetNewUserToList(string userName, Color userColor)
        {
            mainWindowReference.AddUserToList(userName, userColor);
        }

        public void GetUserRemovedFromList(string userName)
        {
            mainWindowReference.RemoveUserFromList(userName);
        }

        public void YouHaveBeenKicked(string userName) {
            mainWindowReference.YouHaveBeenKicked(userName);
        }

        public void YouHaveBeenBanned(string userName) {
            mainWindowReference.YouHaveBeenBanned(userName);
        }

        public void GetUserColor(string userName, Color userColor)
        {
            mainWindowReference.GetUserColor(userName, userColor);
        }
    }
}
