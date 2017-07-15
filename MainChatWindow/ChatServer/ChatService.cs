using ChatInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Text;

namespace ChatServer {
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ChatService" in both code and config file together.
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService {

        public ConcurrentDictionary<string, ConnectedClient> _connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        public int Login(string userName, string userPassword, string userRommName) {

            foreach(var client in _connectedClients) {
                if(client.Key.ToLower() == userName.ToLower()) {
                    return 1;
                }
            }
            var establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();

            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserName = userName;
            newClient.UserPassword = userPassword;
            newClient.UserRoom = userRommName;
            newClient.UserColor = 0;

            _connectedClients.TryAdd(userName, newClient);

            Console.WriteLine("New user connected to server: " + userName);

            return 0;
        }

        public void SendMessageToAll(string message, string userName) {
            Console.WriteLine("Message received from: " + userName + " that contains: " + message);
            foreach(var client in _connectedClients) {
                //if(client.Key.ToLower() != userName.ToLower()) { //Uncomment this to make server skip person that sent this message.
                    client.Value.connection.GetMessage(message, userName);
                //}
            }
        }
    }
}
