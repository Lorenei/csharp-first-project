using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace MainChatWindow
{
    class ChatLogAPI {

        private static Dictionary<string, string> logsDictionary;

        public ChatLogAPI() {
            logsDictionary = new Dictionary<string, string>();
        }

        public bool CreateNewChatLog(string roomName) {

            if(roomName == null || roomName == "") {
                //debug line
                MessageBox.Show("Error! Room name variable is either empty or null.");
                return false;
            }

            if (logsDictionary.ContainsKey(roomName)) {
                //debug line
                MessageBox.Show("Error! Our dictionary already contains key for wanted room name.");
                return false;
            }
            string logName = roomName + "_" + currentDateTime() + ".html";
            logsDictionary.Add(roomName, logName);

            if(File.Exists(logsDictionary[roomName])) {
                //debug line
                MessageBox.Show("Error! Our log file already exists in log folder even though we didn't have it in dictionary.");
                return false;
            }

            try {
                //File.Create(logsDictionary[roomName]);
                File.AppendAllText(logsDictionary[roomName], GenerateHTMLBody());
            }
            catch (Exception e) {
                //debug line
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
            if(!logsDictionary.ContainsKey(roomName)) {
                //debug line
                MessageBox.Show("Error! Our dictionary lost the value for that key.");
                return false;
            }
            if(logsDictionary[roomName] == null || logsDictionary[roomName] == "" || messageToWrite == null || messageToWrite == "") {
                //debug line
                MessageBox.Show("Error! Our room name, dictionary key value or message variable is missing.");
                return false;
            }

            try {
                File.AppendAllText(logsDictionary[roomName], messageToWrite);
            }
            catch(Exception e) {
                //debug line
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

        //public static void CloseAllFiles() {
        //    foreach (var tmp in logsDictionary) {
                
        //    }
        //}
    }
}
