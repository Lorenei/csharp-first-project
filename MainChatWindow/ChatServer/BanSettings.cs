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

        /// <summary>
        /// Time for ban in minutes
        /// </summary>
        private int ForHowLong;
        /// <summary>
        /// The start of ban
        /// </summary>
        private DateTime StartOfBan;

        /// <summary>
        /// Initializes a new instance of the <see cref="BanSettings"/> class.
        /// </summary>
        /// <param name="_ForHowLong">For how long. Default 30 minutes.</param>
        public BanSettings(int _ForHowLong = 30) {
            ForHowLong = _ForHowLong;
            StartOfBan = DateTime.Now;
        }

        /// <summary>
        /// Method returns true if ban should be still active, false if ban should be removed.
        /// </summary>
        /// <returns>
        /// True if ban is still valid, false if it should be removed.
        /// </returns>
        public bool CheckIfStillValid() {

            if((DateTime.Now - StartOfBan).TotalMinutes > ForHowLong) {
                return false;
            }
            return true;
        }
    }
}
