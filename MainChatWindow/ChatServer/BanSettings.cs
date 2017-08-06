using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    /// <summary>
    /// Class used to store ban time and time when it was assigned to user.
    /// </summary>
    class BanSettings {

        private int ForHowLong; //Time for ban in minutes
        private DateTime StartOfBan; //When ban was received.

        public BanSettings(int _ForHowLong = 30) {
            ForHowLong = _ForHowLong;
            StartOfBan = DateTime.Now;
        }

        /// <summary>
        /// Method returns true if ban should be still active, false if ban should be removed.
        /// </summary>
        public bool CheckIfStillValid() {

            if((DateTime.Now - StartOfBan).TotalMinutes > ForHowLong) {
                return false;
            }
            return true;
        }
    }
}
