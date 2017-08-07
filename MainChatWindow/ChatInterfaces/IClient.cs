using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows.Media;

namespace ChatInterfaces {
    /// <summary>
    /// Interface class that show possible methods that server can use to callback a client.
    /// </summary>
    public interface IClient
    {

        /// <summary>
        /// Method called by server on client to receive message that was sent by other user.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="userName">Name of the user.</param>
        [OperationContract]
        void GetMessage(string message, string userName);

        /// <summary>
        /// Method called by server on client to receive users list.
        /// </summary>
        /// <param name="usersList">The users list.</param>
        [OperationContract]
        void GetUsersList(Dictionary<string, string> usersList);

        /// <summary>
        /// Method called by server on client to add new user to users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
        [OperationContract]
        void GetNewUserToList(string userName, Color userColor);

        /// <summary>
        /// Method called by server on client to remove user from users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        [OperationContract]
        void GetUserRemovedFromList(string userName);

        /// <summary>
        /// Method called by server on client to inform client about being kicked from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        [OperationContract]
        void YouHaveBeenKicked(string userName);

        /// <summary>
        /// Method called by server on client to inform client about being banned from service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        [OperationContract]
        void YouHaveBeenBanned(string userName);

        /// <summary>
        /// Method called by server on client to inform client about other users color change.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
        [OperationContract]
        void GetUserColor(string userName, Color userColor);
    }
}
