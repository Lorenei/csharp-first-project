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

        [OperationContract]
        int Login(string userName, string userPassword, string userRoomName);

        [OperationContract]
        void Logout(string userName, string userRoomName);

        [OperationContract]
        void SendMessageToAll(string message, string userName);

        [OperationContract]
        Dictionary<string, string> GetUsersList();

        [OperationContract]
        string ShowIpInfo(string userName, string selectedUserName);

        [OperationContract]
        bool KickUserFromService(string userName, string selectedUserName, string userRoomName);

        [OperationContract]
        bool BanUserFromService(string userName, string selectedUserName, string userRoomName);

        [OperationContract]
        void ChangeUserColor(string userName, Color userColor);

        [OperationContract]
        bool RegisterNewUserToDB(string userName, string userPassword);
    }
}
