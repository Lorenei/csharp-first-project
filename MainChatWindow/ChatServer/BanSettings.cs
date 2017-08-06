using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    class BanSettings {

        private int ForHowLong; //Time for ban in minutes
        private DateTime StartOfBan;
        public BanSettings(int _ForHowLong = 30) {
            ForHowLong = _ForHowLong;
            StartOfBan = DateTime.Now;
        }

        public bool CheckIfStillValid() {

            if((DateTime.Now - StartOfBan).TotalMinutes > ForHowLong) {
                return false;
            }
            return true;
        }
    }
}
