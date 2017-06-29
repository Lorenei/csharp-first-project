using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ChatInterfaces;

namespace ChatClient {

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ClientCallback : IClient {
        public void PlaceHolder() {
            throw new NotImplementedException();
        }
    }
}
