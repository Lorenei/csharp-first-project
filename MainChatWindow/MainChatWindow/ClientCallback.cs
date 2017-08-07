using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows;
using ChatInterfaces;
using System.Windows.Media;

namespace ChatClient
{

    /// <summary>
    /// Setting used to describe class as WCF callback client class.
    /// </summary>
    /// <seealso cref="ChatInterfaces.IClient" />
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ClientCallback : IClient
    {
        /// <summary>
        /// The main window reference.
        /// Stored here so there won't be abuse of getting reference each time server callbacks client.
        /// </summary>
        MainWindow mainWindowReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCallback"/> class.
        /// Finds and stores reference to Main Window at creation.
        /// </summary>
        public ClientCallback()
        {
            //Create reference to MainWindow which is chat main window and store it in variable.
            mainWindowReference = ((MainWindow)Application.Current.Windows.OfType<MainWindow>().FirstOrDefault());
        }

        /// <summary>
        /// Method called by server on client to receive message that was sent by other user.
        /// </summary>
        /// <param name="message">Message received from server.</param>
        /// <param name="userName">Name of user that sent the message.</param>
        public void GetMessage(string message, string userName)
        {
            mainWindowReference.AddMessageToFlowDocument(message, userName);
        }

        /// <summary>
        /// Method called by server on client to receive users list.
        /// </summary>
        /// <param name="usersList">Dictionary that holds users list connected to server. Holding users name, and users name color.</param>
        public void GetUsersList(Dictionary<string, string> usersList)
        {
            mainWindowReference.RefreshUsersList(usersList);
        }

        /// <summary>
        /// Method called by server on client to add new user to users list.
        /// </summary>
        /// <param name="userName">Name of user to add to list.</param>
        /// <param name="userColor">Color of users name that will be added to list.</param>
        public void GetNewUserToList(string userName, Color userColor)
        {
            mainWindowReference.AddUserToList(userName, userColor);
        }

        /// <summary>
        /// Method called by server on client to remove user from users list.
        /// </summary>
        /// <param name="userName">Name of user that will be removed from list.</param>
        public void GetUserRemovedFromList(string userName)
        {
            mainWindowReference.RemoveUserFromList(userName);
        }

        /// <summary>
        /// Method called by server on client to inform client about being kicked from service.
        /// </summary>
        /// <param name="userName">Name of user that kicked you.</param>
        public void YouHaveBeenKicked(string userName)
        {
            mainWindowReference.YouHaveBeenKicked(userName);
        }

        /// <summary>
        /// Method called by server on client to inform client about being banned from service.
        /// </summary>
        /// <param name="userName">Name of user that banned you.</param>
        public void YouHaveBeenBanned(string userName)
        {
            mainWindowReference.YouHaveBeenBanned(userName);
        }

        /// <summary>
        /// Method called by server on client to inform client about other users color change.
        /// </summary>
        /// <param name="userName">Name of user that has color changed.</param>
        /// <param name="userColor">New color selected by userName.</param>
        public void GetUserColor(string userName, Color userColor)
        {
            mainWindowReference.GetUserColor(userName, userColor);
        }
    }
}
