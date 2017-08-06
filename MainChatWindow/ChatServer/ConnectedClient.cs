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
        public IClient connection; //Holds callback channel to make it possible to call this client methods.
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserRoom { get; set; } //Holds name of room where user is logged in
        public Color UserColor { get; set; } //Holds color of users name that will be visible in logged in users list.
        public string UserIPAddress; //Holds users IP address
    }
}
