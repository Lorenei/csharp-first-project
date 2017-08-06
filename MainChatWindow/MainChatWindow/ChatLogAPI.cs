using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Class that holds API for clients file logs for future each room.
    /// </summary>
    class ChatLogAPI {

        /// <summary>
        /// Dictionary holds informations about file logs, so there wont be two logs for the same chat room.
        /// </summary>
        private static Dictionary<string, string> logsDictionary;

        public ChatLogAPI() {
            logsDictionary = new Dictionary<string, string>();
        }

        public bool CreateNewChatLog(string roomName) {

            //Check if room name was provided.
            if(roomName == null || roomName == "") {
                //debug line
                MessageBox.Show("Error! Room name variable is either empty or null.");
                return false;
            }

            //Check if there already is created log for this chat room.
            if (logsDictionary.ContainsKey(roomName)) {
                //debug line
                MessageBox.Show("Error! Our dictionary already contains key for wanted room name.");
                return false;
            }
            string logName = roomName + "_" + currentDateTime() + ".html";
            logsDictionary.Add(roomName, logName);

            //Safety check if file exists.
            if(File.Exists(logsDictionary[roomName])) {
                //debug line
                MessageBox.Show("Error! Our log file already exists in log folder even though we didn't have it in dictionary.");
                return false;
            }

            //Try to create chat log with predefined HTML body. May be customized in future release.
            try {
                File.AppendAllText(logsDictionary[roomName], GenerateHTMLBody());
            }
            catch (Exception e) {
                MessageBox.Show("Error while trying to create log file. Error path: " + e.ToString());
                logsDictionary.Remove(roomName);
                return false;
            }
            return true;
        }

        //Maybe add </body></html> at the end of file when exiting ?
        //probably should be called from app_exit
        //Which will need dictionary to be static
        public bool WriteToChatLog(string roomName, string messageToWrite) {
            //Safety check if dictionary holds information about chat room.
            if(!logsDictionary.ContainsKey(roomName)) {
                MessageBox.Show("Error! Our dictionary lost the value for that key.");
                return false;
            }
            if(logsDictionary[roomName] == null || logsDictionary[roomName] == "" || messageToWrite == null || messageToWrite == "") {
                MessageBox.Show("Error! Our room name, dictionary key value or message variable is missing.");
                return false;
            }

            try {
                File.AppendAllText(logsDictionary[roomName], messageToWrite);
            }
            catch(Exception e) {
                MessageBox.Show("Error while trying to append text to our log file! Error path: " + e.ToString());
                return false;
            }

            return true;
        }

        private string currentDateTime() {
            return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        }

        private string GenerateHTMLBody() {
            return "<!DOCTYPE html><head lang=\"en\"><meta charset=\"utf-8\"><title>Log</title></head><html><body>";
        }
    }
}
