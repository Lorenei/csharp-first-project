using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows.Media;

namespace ChatInterfaces {
    public interface IClient {

        [OperationContract]
        void GetMessage(string message, string userName);

        [OperationContract]
        void GetUsersList(Dictionary<string, string> usersList);

        [OperationContract]
        void GetNewUserToList(string userName, Color userColor);

        [OperationContract]
        void GetUserRemovedFromList(string userName);

        [OperationContract]
        void YouHaveBeenKicked(string userName);

        [OperationContract]
        void YouHaveBeenBanned(string userName);

        [OperationContract]
        void GetUserColor(string userName, Color userColor);
    }
}
