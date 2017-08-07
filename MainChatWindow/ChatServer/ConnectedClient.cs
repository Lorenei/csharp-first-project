using ChatInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChatServer {
    /// <summary>
    /// Class stored informations about connected user to service.
    /// </summary>
    public class ConnectedClient {
        /// <summary>
        /// Holds callback channel to make it possible to call this client methods.
        /// </summary>
        public IClient connection;
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        /// <value>
        /// The user password.
        /// </value>
        public string UserPassword { get; set; }
        /// <summary>
        /// Holds name of room where user is logged in
        /// </summary>
        /// <value>
        /// The user room.
        /// </value>
        public string UserRoom { get; set; }
        /// <summary>
        /// Holds color of users name that will be visible in logged in users list.
        /// </summary>
        /// <value>
        /// The color of the user.
        /// </value>
        public Color UserColor { get; set; }
        /// <summary>
        /// The user ip address
        /// </summary>
        public string UserIPAddress { get; set; }
    }
}
