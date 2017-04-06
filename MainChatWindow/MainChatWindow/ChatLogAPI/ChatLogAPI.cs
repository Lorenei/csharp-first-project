using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MainChatWindow
{
    class ChatLogAPI {

        private Dictionary<string, string> logsDictionary;
        //private List<string> logsList;
        public ChatLogAPI() {
            logsDictionary = new Dictionary<string, string>();
            //logsList = new List<string>();
        }

        public bool CreateNewChatLog(string roomName) {

            if(logsDictionary.ContainsKey(roomName)) {
                return false;
            }
            string logName = roomName + "_" + currentDateTime();
            //logsList.Add(roomName);
            //if(File.Exists(logsDictionary[roomName])) {
                //return false;
            //}

            logsDictionary.Add(roomName, logName);
            try {
                File.Create(logsDictionary[roomName]);
            }
            catch (Exception e) {
                return false;
            }
            return true;
        }

        public bool WriteToChatLog(string roomName, string messageToWrite) {
            if(!logsDictionary.ContainsKey(roomName) || !File.Exists(logsDictionary[roomName])) {
                return false;
            }
            if(logsDictionary[roomName] == null || logsDictionary[roomName] == "" || messageToWrite == null || messageToWrite == "") {
                return false;
            }

            try {
                File.AppendAllText(logsDictionary[roomName], messageToWrite);
            }
            catch(Exception e) {
                return false;
            }

            return true;
        }

        private string currentDateTime() {
            return DateTime.Now.ToString("dd-mm-yyyy_hh:mm:ss");
        }
    }
}
