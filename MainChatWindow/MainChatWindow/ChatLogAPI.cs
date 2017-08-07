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

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatLogAPI"/> class.
        /// </summary>
        public ChatLogAPI() {
            logsDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates the new chat log.
        /// </summary>
        /// <param name="roomName">Name of the chat room.</param>
        /// <returns>True if success, false if failed.</returns>
        public bool CreateNewChatLog(string roomName) {

            //Check if room name was provided.
            if(roomName == null || roomName == "") {
                MessageBox.Show("Error! Room name variable is either empty or null.");
                return false;
            }

            //Check if there already is created log for this chat room.
            if (logsDictionary.ContainsKey(roomName)) {
                MessageBox.Show("Error! Our dictionary already contains key for wanted room name.");
                return false;
            }
            string logName = roomName + "_" + currentDateTime() + ".html";
            logsDictionary.Add(roomName, logName);

            //Safety check if file exists.
            if(File.Exists(logsDictionary[roomName])) {
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

        /// <summary>
        /// Writes to chat log.
        /// </summary>
        /// <param name="roomName">Name of the chat room.</param>
        /// <param name="messageToWrite">The message to write.</param>
        /// <returns>
        /// True if message succesfuly written to log, false if it failed.
        /// </returns>
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
            //Try to add message to our log file.
            try {
                File.AppendAllText(logsDictionary[roomName], messageToWrite);
            }
            catch(Exception e) {
                MessageBox.Show("Error while trying to append text to our log file! Error path: " + e.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns current the date time.
        /// </summary>
        /// <returns>
        /// String containing current date and time so it can be used as part of the chat log name.
        /// </returns>
        private string currentDateTime() {
            return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        }

        /// <summary>
        /// Generates the HTML body.
        /// Future release may allow to customize it.
        /// </summary>
        /// <returns>String containing beginning HTML body.</returns>
        private string GenerateHTMLBody() {
            return "<!DOCTYPE html><head lang=\"en\"><meta charset=\"utf-8\"><title>Log</title></head><html><body>";
        }
    }
}
