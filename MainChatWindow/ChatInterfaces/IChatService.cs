using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Media;

namespace ChatInterfaces {

    /// <summary>
    /// Interface class that show possible methods that client can use to call a server.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IClient))]

    public interface IChatService
    {

        /// <summary>
        /// Method used by clients to login into service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>
        /// 0 for succesful login, other for failed attempt.
        /// </returns>
        [OperationContract]
        int Login(string userName, string userPassword, string userRoomName);

        /// <summary>
        /// Method used by clients to logout from service. Server will remove user from it's dictionary.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        [OperationContract]
        void Logout(string userName, string userRoomName);

        /// <summary>
        /// Method used to inform all users except for the one doing the sending, about new message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="userName">Name of the user.</param>
        [OperationContract]
        void SendMessageToAll(string message, string userName);

        /// <summary>
        /// Method that returns whole list of logged in users into service.
        /// </summary>
        /// <returns>Users list as dictionary item.</returns>
        [OperationContract]
        Dictionary<string, string> GetUsersList();

        /// <summary>
        /// Method returns ip address of selected client from users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <returns>IP address as string.</returns>
        [OperationContract]
        string ShowIpInfo(string userName, string selectedUserName);

        /// <summary>
        /// Method used to kick selected user from users list from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>True if kicked, false if not.</returns>
        [OperationContract]
        bool KickUserFromService(string userName, string selectedUserName, string userRoomName);

        /// <summary>
        /// Method used to ban selected user from users list from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="selectedUserName">Name of the selected user.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        /// <returns>True if banned, false if not.</returns>
        [OperationContract]
        bool BanUserFromService(string userName, string selectedUserName, string userRoomName);

        /// <summary>
        /// Method used to change visible color on users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
        [OperationContract]
        void ChangeUserColor(string userName, Color userColor);

        /// <summary>
        /// Method used to register new user to service database file.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns>True if registered, false if not.</returns>
        [OperationContract]
        bool RegisterNewUserToDB(string userName, string userPassword);
    }
}
