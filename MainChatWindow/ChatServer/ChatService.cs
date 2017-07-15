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

        public Dictionary<string, int> GetUsersList() {
            Dictionary<string, int> usersList = new Dictionary<string, int>();

            foreach(var client in _connectedClients) {
                usersList.Add(client.Value.UserName, client.Value.UserColor);
            }

            return usersList;
        }

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

            if(_connectedClients.TryAdd(userName, newClient)) {
                Console.WriteLine("New user added to dictionary");
            }
            Console.WriteLine("User connection print: " + newClient.connection.ToString());

            Console.WriteLine("New user connected to server: " + userName);

            return 0;
        }

        public void SendMessageToAll(string message, string userName) {
            Console.WriteLine("Message received from: " + userName + " that contains: " + message);
            foreach(var client in _connectedClients) {
                //IMPORTANT NOTE TO MYSELF!!
                //IF I TRY TO REMOVE BELOW LINE, TO MAKE SERVER SEND MESSAGE TO CLIENT THAT SENT IT
                //APP CRASHES/FREEZES DUE TO UNKNOWN REASON
                //IT'S PROBABLY BECAUSE PORT IS USED TO SEND MESSAGE TO SERVER AND IS WAITING FOR RESPONSE
                //BUT SERVER IS TRYING TO SEND CALLBACK TO THE SAME PORT BEFORE SENDING RESPONSE
                //WHICH FREEZES CLIENT AND MAKES IT TIMEOUT
                //NEED TO FIND SOLUTION
                if(client.Key.ToLower() != userName.ToLower()) { //Uncomment this to make server skip person that sent this message.
                    Console.WriteLine("Entered foreach loop, trying to send message to: " + client.Value.UserName);
                    client.Value.connection.GetMessage(message, userName);
                    Console.WriteLine("Message sent to: " + client.Value.UserName);
                }
            }
        }
    }
}
