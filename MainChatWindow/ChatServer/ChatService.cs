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
using System.IO;

namespace ChatServer {

    /// <summary>
    /// Main class of Chat Service that holds all logic of server.
    /// </summary>
    /// <seealso cref="ChatInterfaces.IChatService" />
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService {

        /// <summary>
        /// This dictionary holds all of our currently connected users, together with some of their personal settings.
        /// </summary>
        public ConcurrentDictionary<string, ConnectedClient> _connectedClients = new ConcurrentDictionary<string, ConnectedClient>();

        /// <summary>
        /// This dictionary holds all currently banned users.
        /// </summary>
        private ConcurrentDictionary<string, BanSettings> BanList = new ConcurrentDictionary<string, BanSettings>();

        /// <summary>
        /// This dictionary holds all users currently registered from database.
        /// </summary>
        private ConcurrentDictionary<string, string> _usersDatabaseDictionary = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// The user database file path
        /// </summary>
        private string _userDBFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatService"/> class.
        /// </summary>
        /// <param name="usersDatabaseDictionary">The users database dictionary.</param>
        /// <param name="userDBFilePath">The user database file path.</param>
        public ChatService(ConcurrentDictionary<string,string> usersDatabaseDictionary, string userDBFilePath)
        {
            _usersDatabaseDictionary = usersDatabaseDictionary;
            _userDBFilePath = userDBFilePath;
        }

        /// <summary>
        /// This method returns dictionary of users with their personal name colors. Used by new conneted users since only they need whole list.
        /// </summary>
        /// <returns>
        /// Users list as dictionary item.
        /// </returns>
        public Dictionary<string, string> GetUsersList() {
            Dictionary<string, string> usersList = new Dictionary<string, string>();

            foreach(var client in _connectedClients) {
                usersList.Add(client.Value.UserName, client.Value.UserColor.ToString());
            }

            return usersList;
        }

        /// <summary>
        /// Method used by clients to login into service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>
        /// 0 for succesful login, other for failed attempt.
        /// </returns>
        public int Login(string userName, string userPassword, string userRoomName) {

            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            var establishedUserConnection = context.GetCallbackChannel<IClient>();

            ConnectedClient newClient = new ConnectedClient();
            newClient.connection = establishedUserConnection;
            newClient.UserIPAddress = endpoint.Address;

            Console.WriteLine("New user trying to log in with login: " + userName + " and password: " + userPassword);

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

            //Check if server already has someone logged in with new users name.
            foreach (var client in _connectedClients) {
                if(client.Key.ToLower() == userName.ToLower()) {
                    return 1;
                }
            }

            //Check if provided login and password are in database and are correct.
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

            //Create new user.
            newClient.UserName = userName;
            newClient.UserPassword = userPassword;
            newClient.UserRoom = userRoomName;
            newClient.UserColor = Colors.Black;

            //Add new user to dictionary
            if (_connectedClients.TryAdd(userName, newClient)) {
                Console.WriteLine("New user added to dictionary");
            }
            Console.WriteLine("User connection print: " + newClient.connection.ToString());

            Console.WriteLine("New user connected to server: " + userName);
            Console.WriteLine("New users IP IS: " + newClient.UserIPAddress);

            //Send information about new user connected to everyone.
            UpdateUsersListForAll(newClient.UserName, newClient.UserColor);

            return 0;
        }
        /// <summary>
        /// Method used by clients to logout from service. Server will remove user from it's dictionary.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        public void Logout(string userName, string userRoomName)
        {
            ConnectedClient clientToRemove = new ConnectedClient();
            if(_connectedClients.TryRemove(userName, out clientToRemove)) {
                //Console.WriteLine("Logout(string,string): Server removed client from dictionary: " + userName);
                Console.WriteLine("Server removed client from dictionary: " + userName);
            }
            UpdateUsersListForAll(userName, Colors.Black, true);
            //Console.WriteLine("Logout(string,string): Sending request to clients to remove user from users list: " + userName);
            Console.WriteLine("Sending request to clients to remove user from users list: " + userName);
        }
        /// <summary>
        /// Logouts the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <param name="dontInformThisUserAboutLogout">The dont inform this user about logout.</param>
        private void Logout(string userName, string userRoomName, string dontInformThisUserAboutLogout) {
            ConnectedClient clientToRemove = new ConnectedClient();
            if (_connectedClients.TryRemove(userName, out clientToRemove)) {
                Console.WriteLine("Logout(string,string,string): Server removed client from dictionary: " + userName);
            }

            UpdateUsersListForAll(userName, clientToRemove.UserColor, true, dontInformThisUserAboutLogout);
            Console.WriteLine("Logout(string,string,string): Sending request to clients to remove user from users list: " + userName + " dontinformthisuseraboutlogout = " + dontInformThisUserAboutLogout);
        }

        /// <summary>
        /// Updates the users list for all.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
        /// <param name="isLoggingOut">if set to <c>true</c> [is logging out].</param>
        /// <param name="dontSendRequestToThisUser">The dont send request to this user.</param>
        private void UpdateUsersListForAll(string userName, Color userColor, bool isLoggingOut = false, string dontSendRequestToThisUser = null)
        {
            foreach(var client in _connectedClients)
            {
                if(client.Key.ToLower() != userName.ToLower())
                {
                    if (dontSendRequestToThisUser == null || dontSendRequestToThisUser == "" || (dontSendRequestToThisUser != null && dontSendRequestToThisUser != "" && dontSendRequestToThisUser.ToLower() != client.Key.ToLower()))
                    {
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
                        }
                    }
                    else
                    {
                        Console.WriteLine("Didnt send request to log out kicked user to: " + dontSendRequestToThisUser);
                    }
                }
            }
        }
        /// <summary>
        /// Updates the users list for all.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
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

        /// <summary>
        /// Method used to inform all users except for the one doing the sending, about new message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="userName">Name of the user.</param>
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

        /// <summary>
        /// Method returns ip address of selected client from users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <returns>
        /// IP address as string.
        /// </returns>
        public string ShowIpInfo(string userName, string selectedUserName)
        {
            //if userThatWantsToSee has admin privileges return ip if not return error
            return _connectedClients[selectedUserName].UserIPAddress;
        }

        /// <summary>
        /// Method used to kick selected user from users list from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>
        /// True if kicked, false if not.
        /// </returns>
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

        /// <summary>
        /// Method used to ban selected user from users list from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>
        /// True if banned, false if not.
        /// </returns>
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

        /// <summary>
        /// Adds to ban list.
        /// </summary>
        /// <param name="bannedUserName">Name of the banned user.</param>
        /// <param name="roomName">Name of the room.</param>
        private void AddToBanList(string bannedUserName, string roomName) {
            //BanList.TryAdd(_connectedClients[bannedUserName].UserIPAddress, new BanSettings());
            BanList.TryAdd(_connectedClients[bannedUserName].UserName, new BanSettings());
            Console.WriteLine("AddToBanList(string,string): Added new user to ban list: " + bannedUserName);
        }

        /// <summary>
        /// Method used to change visible color on users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
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

        /// <summary>
        /// Informs the users about color change.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
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

        /// <summary>
        /// Method used to register new user to service database file.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns>
        /// True if registered, false if not.
        /// </returns>
        public bool RegisterNewUserToDB(string userName, string userPassword)
        {
            if(_usersDatabaseDictionary.ContainsKey(userName))
            {
                return false;
            }
            try
            {
                File.AppendAllText(_userDBFilePath, userName + " " + userPassword + Environment.NewLine);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch(Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }
            _usersDatabaseDictionary.TryAdd(userName, userPassword);
            return true;
        }
    }
}
