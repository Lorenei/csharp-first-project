using ChatInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Text;
using System.ServiceModel.Channels;
using System.Windows.Media;

namespace ChatServer {

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService {

        //This dictionary holds all of our currently connected users, together with some of their personal settings.
        public ConcurrentDictionary<string, ConnectedClient> _connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        private ConcurrentDictionary<string, BanSettings> BanList = new ConcurrentDictionary<string, BanSettings>();

        private ConcurrentDictionary<string, string> _usersDatabaseDictionary = new ConcurrentDictionary<string, string>();

        public ChatService(ConcurrentDictionary<string,string> usersDatabaseDictionary)
        {
            _usersDatabaseDictionary = usersDatabaseDictionary;
        }

        //This method returns dictionary of users with their personal name colors. Used by new conneted users since only they need whole list.
        public Dictionary<string, string> GetUsersList() {
            Dictionary<string, string> usersList = new Dictionary<string, string>();

            foreach(var client in _connectedClients) {
                usersList.Add(client.Value.UserName, client.Value.UserColor.ToString());
            }

            return usersList;
        }

        public int Login(string userName, string userPassword, string userRoomName) {

            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            var establishedUserConnection = context.GetCallbackChannel<IClient>();

            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserIPAddress = endpoint.Address;

            Console.WriteLine("new user trying to log in with login: " + userName + " and password: " + userPassword);

            //Commented out things that make ban for IP address. It makes impossible to localtest it.
            //Bans for username instead.

            //if(BanList.ContainsKey(newClient.UserIPAddress)) {
            if(BanList.ContainsKey(userName)) { 
                //if(!BanList[newClient.UserIPAddress].CheckIfStillValid()) {
                if(!BanList[userName].CheckIfStillValid()) { 
                    BanSettings tempBanSettings = new BanSettings();
                    //BanList.TryRemove(newClient.UserIPAddress, out tempBanSettings);
                    BanList.TryRemove(userName, out tempBanSettings);
                }
                else {
                    //context.InstanceContext.Abort();
                    Console.WriteLine("User is banned. Login failed.");
                    return 1;
                }
            }

            foreach (var client in _connectedClients) {
                if(client.Key.ToLower() == userName.ToLower()) {
                    return 1;
                }
            }

            if(_usersDatabaseDictionary.ContainsKey(userName))
            {
                if(_usersDatabaseDictionary[userName] != userPassword)
                {
                    return 1;
                } 
            }
            else
            {
                return 1;
            }

            newClient.UserName = userName;
            newClient.UserPassword = userPassword;
            newClient.UserRoom = userRoomName;
            newClient.UserColor = Colors.Black;

            //debug data. This ip adres need verification since its saying localhost and I have no idea whether its servers adres,endpoints adress or simple client adress
            //newClient.debugConnectionInfo = OperationContext.Current.IncomingMessageProperties
            //^^^ temporary need to review this at later date, but it works

            if (_connectedClients.TryAdd(userName, newClient)) {
                Console.WriteLine("New user added to dictionary");
            }
            Console.WriteLine("User connection print: " + newClient.connection.ToString());

            Console.WriteLine("New user connected to server: " + userName);
            Console.WriteLine("New users IP IS: " + newClient.UserIPAddress);

            //UpdateUsersListForAll(userName);
            UpdateUsersListForAll(newClient.UserName, newClient.UserColor);

            return 0;
        }
        public void Logout(string userName, string userRoomName)
        {
            ConnectedClient clientToRemove = new ConnectedClient();
            if(_connectedClients.TryRemove(userName, out clientToRemove)) {
                Console.WriteLine("Logout(string,string): Server removed client from dictionary: " + userName);
            }
            UpdateUsersListForAll(userName, Colors.Black, true);
            Console.WriteLine("Logout(string,string): Sending request to clients to remove user from users list: " + userName);
            //clientToRemove.connection.
        }
        private void Logout(string userName, string userRoomName, string dontInformThisUserAboutLogout) {
            ConnectedClient clientToRemove = new ConnectedClient();
            if (_connectedClients.TryRemove(userName, out clientToRemove)) {
                Console.WriteLine("Logout(string,string,string): Server removed client from dictionary: " + userName);
            }

            UpdateUsersListForAll(userName, clientToRemove.UserColor, true, dontInformThisUserAboutLogout);
            Console.WriteLine("Logout(string,string,string): Sending request to clients to remove user from users list: " + userName + " dontinformthisuseraboutlogout = " + dontInformThisUserAboutLogout);
        }

        private void UpdateUsersListForAll(string userName, Color userColor, bool isLoggingOut = false, string dontSendRequestToThisUser = null)
        {
            foreach(var client in _connectedClients)
            {
                if(client.Key.ToLower() != userName.ToLower())
                {
                    if (dontSendRequestToThisUser == null || dontSendRequestToThisUser == "" || (dontSendRequestToThisUser != null && dontSendRequestToThisUser != "" && dontSendRequestToThisUser.ToLower() != client.Key.ToLower()))
                    {
                        //continue;
                        //}
                        if (!isLoggingOut)
                        {
                            Console.WriteLine("Sending new user to join users list to : " + client.Key);
                            try
                            {
                                client.Value.connection.GetNewUserToList(userName, userColor);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to send new joined user to : " + client.Key + ". Error e: " + e.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sending update of user logging out to: " + client.Key);
                            try
                            {
                                client.Value.connection.GetUserRemovedFromList(userName);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to send user logged out to: " + client.Key + ". Error e: " + e.ToString());
                            }
                            //Console.WriteLine("dontsendrequesttothisuser = " + dontSendRequestToThisUser);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Didnt send request to log out kicked user to: " + dontSendRequestToThisUser);
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
                    try
                    {
                        client.Value.connection.GetUsersList(this.GetUsersList());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Failed to send users list to : " + client.Key + ". Error e: " + e.ToString());
                    }
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
                    try
                    {
                        client.Value.connection.GetMessage(message, userName);
                        Console.WriteLine("Message sent to: " + client.Value.UserName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to send message to: " + client.Key + ". Error e: " + e.ToString());
                    }
                }
            }
        }

        public string ShowIpInfo(string userName, string selectedUserName)
        {
            //if userThatWantsToSee has admin privileges return ip if not return error
            return _connectedClients[selectedUserName].UserIPAddress;
        }

        public bool KickUserFromService(string userName, string selectedUserName, string userRoomName) {
            //if user has admin rights allow him to kick user
            Console.WriteLine("Request to kick user named: " + selectedUserName + " received from: " + userName);
            try {
                _connectedClients[selectedUserName].connection.YouHaveBeenKicked(userName);
            }
            catch(Exception e) {
                Console.WriteLine("KickUserFromService(string,string,string): Error: " + e.ToString());
            }
            Logout(selectedUserName, userRoomName, userName);
            return true;
        }

        public bool BanUserFromService(string userName, string selectedUserName, string userRoomName) {

            Console.WriteLine("Request to ban user named: " + selectedUserName + " received from: " + userName);
            try {
                _connectedClients[selectedUserName].connection.YouHaveBeenBanned(userName);
            }
            catch(Exception e) {
                Console.WriteLine("BanUserFromService(string, string, string): Error: " + e.ToString());
            }
            AddToBanList(selectedUserName, userRoomName);
            Logout(selectedUserName, userRoomName, userName);

            return true;
        }

        private void AddToBanList(string bannedUserName, string roomName) {
            //BanList.TryAdd(_connectedClients[bannedUserName].UserIPAddress, new BanSettings());
            BanList.TryAdd(_connectedClients[bannedUserName].UserName, new BanSettings());
            Console.WriteLine("AddToBanList(string,string): Added new user to ban list: " + bannedUserName);
        }

        public void ChangeUserColor(string userName, Color userColor)
        {
            if (_connectedClients.ContainsKey(userName))
            {
                _connectedClients[userName].UserColor = userColor;
                Console.WriteLine("ChangeUserColor(string,Color): Changed user ( " + userName + " ) color to: " + userColor.ToString());
                InformUsersAboutColorChange(userName, userColor);
                Console.WriteLine("ChangeUserColor(string,Color): Started method to inform everyone about color change");
            }
        }

        private void InformUsersAboutColorChange(string userName, Color userColor)
        {
            foreach(var client in _connectedClients)
            {
                if(client.Key.ToLower() != userName.ToLower())
                {
                    try
                    {
                        client.Value.connection.GetUserColor(userName, userColor);
                        Console.WriteLine("InformUsersAboutColorChange(string,Color): Sending request to change users color on list to: " + client.Value.UserName);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Failed to send informationa bout color change to: " + client.Key + ". Error e: " + e.ToString());
                    }
                }
            }
        }
    }
}
