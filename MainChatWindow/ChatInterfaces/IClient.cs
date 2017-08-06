using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows.Media;

namespace ChatInterfaces {
    public interface IClient {

        /// <summary>
        /// Method called by server on client to receive message that was sent by other user.
        /// </summary>
        [OperationContract]
        void GetMessage(string message, string userName);

        /// <summary>
        /// Method called by server on client to receive users list.
        /// </summary>
        [OperationContract]
        void GetUsersList(Dictionary<string, string> usersList);

        /// <summary>
        /// Method called by server on client to add new user to users list.
        /// </summary>
        [OperationContract]
        void GetNewUserToList(string userName, Color userColor);

        /// <summary>
        /// Method called by server on client to remove user from users list.
        /// </summary>
        [OperationContract]
        void GetUserRemovedFromList(string userName);

        /// <summary>
        /// Method called by server on client to inform client about being kicked from service.
        /// </summary>
        [OperationContract]
        void YouHaveBeenKicked(string userName);

        /// <summary>
        /// Method called by server on client to inform client about being banned from service.
        /// </summary>
        [OperationContract]
        void YouHaveBeenBanned(string userName);

        /// <summary>
        /// Method called by server on client to inform client about other users color change.
        /// </summary>
        [OperationContract]
        void GetUserColor(string userName, Color userColor);
    }
}
