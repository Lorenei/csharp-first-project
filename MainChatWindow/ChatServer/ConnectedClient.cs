using ChatInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    public class ConnectedClient {
        public IClient connection;
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserRoom { get; set; } //Holds name of room where user is logged in
        public int UserColor { get; set; } //Holds color of users name that will be visible in logged in users list.
        public string debugConnectionInfo;
    }
}
