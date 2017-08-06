using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Media;

namespace ChatInterfaces {

    [ServiceContract(CallbackContract = typeof(IClient))]

    public interface IChatService {

        /// <summary>
        /// Method used by clients to login into service.
        /// </summary>
        /// <returns>0 for succesful login, other for failed attempt.</returns>
        [OperationContract]
        int Login(string userName, string userPassword, string userRoomName);

        /// <summary>
        /// Method used by clients to logout from service. Server will remove user from it's dictionary.
        /// </summary>
        [OperationContract]
        void Logout(string userName, string userRoomName);

        /// <summary>
        /// Method used to inform all users except for the one doing the sending, about new message.
        /// </summary>
        [OperationContract]
        void SendMessageToAll(string message, string userName);

        /// <summary>
        /// Method that returns whole list of logged in users into service.
        /// </summary>
        [OperationContract]
        Dictionary<string, string> GetUsersList();

        /// <summary>
        /// Method returns ip address of selected client from users list.
        /// </summary>
        [OperationContract]
        string ShowIpInfo(string userName, string selectedUserName);

        /// <summary>
        /// Method used to kick selected user from users list from service.
        /// </summary>
        [OperationContract]
        bool KickUserFromService(string userName, string selectedUserName, string userRoomName);

        /// <summary>
        /// Method used to ban selected user from users list from service.
        /// </summary>
        [OperationContract]
        bool BanUserFromService(string userName, string selectedUserName, string userRoomName);

        /// <summary>
        /// Method used to change visible color on users list.
        /// </summary>
        [OperationContract]
        void ChangeUserColor(string userName, Color userColor);

        /// <summary>
        /// Method used to register new user to service database file.
        /// </summary>
        [OperationContract]
        bool RegisterNewUserToDB(string userName, string userPassword);
    }
}
