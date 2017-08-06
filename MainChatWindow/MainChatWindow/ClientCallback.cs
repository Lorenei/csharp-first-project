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
        //ChatWindow _chatWindow;

        //public void LoadReference(ChatWindow chatWindow) {
            //_chatWindow = chatWindow;
        //}
        public ClientCallback()
        {
            mainWindowReference = ((MainWindow)Application.Current.Windows.OfType<MainWindow>().FirstOrDefault());
        }
        public void GetMessage(string message, string userName) {
            //((MainWindow)Application.Current.MainWindow).AddMessageToFlowDocument(message, userName);
            //((MainWindow)Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()).AddMessageToFlowDocument(message, userName);
            //if(mainWindowReference != null) { 
                mainWindowReference.AddMessageToFlowDocument(message, userName);
            //}
            //else {
                //MessageBox.Show("Error: ClientCallback doesn't have reference to chat window at GetMessage function.");
            //}
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
