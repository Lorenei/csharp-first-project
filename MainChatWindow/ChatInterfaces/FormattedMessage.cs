using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatInterfaces
{
    class FormattedMessage
    {
        //this class will be used by client and server for formated messaging system functionality
        // will contain properties etc with choosen format of message and message itself
        public string Message { get; set; }
        public string Formatting { get; set; }
        public string Color { get; set; }


    }
}
