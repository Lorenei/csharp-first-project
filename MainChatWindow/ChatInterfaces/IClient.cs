﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ChatInterfaces {
    public interface IClient {
        [OperationContract]
        void GetMessage(string message, string userName);
    }
}
