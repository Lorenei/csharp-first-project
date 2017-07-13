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
        public void GetMessage(string message, string userName) {
            ((MainWindow)Application.Current.MainWindow).AddMessageToFlowDocument(message, userName);
        }
    }
}
