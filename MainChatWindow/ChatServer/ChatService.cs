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

        public int Login(string userName, string userPassword, string userRoomName) {

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
            newClient.UserRoom = userRoomName;
            newClient.UserColor = 0;

            //debug data. This ip adres need verification since its saying localhost and I have no idea whether its servers adres,endpoints adress or simple client adress
            newClient.debugConnectionInfo = OperationContext.Current.Channel.LocalAddress.Uri.DnsSafeHost.ToString();

            if(_connectedClients.TryAdd(userName, newClient)) {
                Console.WriteLine("New user added to dictionary");
            }
            Console.WriteLine("User connection print: " + newClient.connection.ToString());

            Console.WriteLine("New user connected to server: " + userName);
            Console.WriteLine("New users IP IS: " + newClient.debugConnectionInfo);

            //UpdateUsersListForAll(userName);
            UpdateUsersListForAll(userName, newClient.UserColor);

            return 0;
        }
        public void Logout(string userName, string userRoomName)
        {
            ConnectedClient clientToRemove = new ConnectedClient();
            if(_connectedClients.TryRemove(userName, out clientToRemove)) {
                Console.WriteLine("Server removed client from dictionary: " + userName);
            }
            UpdateUsersListForAll(userName, 0, true);
            Console.WriteLine("Sending request to clients to remove logged out user: " + userName);
        }

        private void UpdateUsersListForAll(string userName, int userColor = 0, bool isLoggingOut = false)
        {
            foreach(var client in _connectedClients)
            {
                if(client.Key.ToLower() != userName.ToLower())
                {
                    if (!isLoggingOut)
                    {
                        Console.WriteLine("Sending new user to join users list to : " + client.Key);
                        client.Value.connection.GetNewUserToList(userName, userColor);
                    }
                    else
                    {
                        Console.WriteLine("Sending update of user logging out to: " + client.Key);
                        client.Value.connection.GetUserRemovedFromList(userName);
                    }
                }
            }
        }
        private void UpdateUsersListForAll(string userName)
        {
            foreach(var client in _connectedClients)
            {
                if(client.Key.ToLower() != userName.ToLower())
                {
                    Console.WriteLine("Sending users list to : " + client.Key);
                    client.Value.connection.GetUsersList(this.GetUsersList());
                }
            }
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

        public string ShowIpInfo(string userName, string roomName)
        {
            //if userThatWantsToSee is OP return ip if not return error
            string ipAddress = _connectedClients[userName].debugConnectionInfo;
            return ipAddress;
        }
    }
}
