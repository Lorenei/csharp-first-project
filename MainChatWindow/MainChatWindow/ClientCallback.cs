using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using ChatInterfaces;

namespace ChatClient {

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ClientCallback : IClient {
        ChatWindow _chatWindow;
        public ClientCallback(ChatWindow chatWindow) {
            _chatWindow = chatWindow;
        }
        public void LoadReference(ChatWindow chatWindow) {
            _chatWindow = chatWindow;
        }
        public void GetMessage(string message, string userName) {
            //((MainWindow)Application.Current.MainWindow).AddMessageToFlowDocument(message, userName);
            if(_chatWindow != null) {
                _chatWindow.AddMessageToFlowDocument(message, userName);
            }
            else {
                MessageBox.Show("Error: ClientCallback doesn't have reference to chat window at GetMessage function");
            }
        }
    }
}
