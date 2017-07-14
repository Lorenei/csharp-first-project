﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ServiceModel;
using ChatInterfaces;

namespace ChatClient {
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window {

        //Service vars
        public static IChatService Server;
        private static DuplexChannelFactory<IChatService> _channelFactory;

        private Paragraph tempParagraph;
        private Span _time_span_;
        private Span _message_span_;
        private ChatLogAPI chatLog;

        private string _LOGIN_;
        private string _PASSWORD_;
        private string _ROOM_NAME_;

        public bool _IS_CONNECTION_DONE_ = false;

        public ChatWindow(string login, string password, string room) {
            InitializeComponent();

            _LOGIN_ = login;
            _PASSWORD_ = password;
            _ROOM_NAME_ = room;

            //Service initialization
            _channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            Server = _channelFactory.CreateChannel();

            //create new log file for current chat room
            chatLog = new ChatLogAPI();
            GenerateLogFile();

            _IS_CONNECTION_DONE_ = true;

        }
        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                string tempString = UserMessageTextBox.Text;
                if(tempString != null && tempString != "") {
                    AddMessageToFlowDocument(tempString);
                    UserMessageTextBox.Clear();
                }
            }
        }

        //should be activated by server response about whether message was accepted or not
        private void AddMessageToFlowDocument(string message) {
            if(message == "" || message == null) {
                return;
            }
            //string that will be outed by layoutmessage method so we can write stylised with html message to our log file
            string _message_to_log;
            tempParagraph = layoutMessage(_LOGIN_, message, out _message_to_log);
            tempParagraph.Loaded += loadedBlock;
            OknoChatowe.Document.Blocks.Add(tempParagraph);

            //write our message to our log file
            SaveToLog(_ROOM_NAME_, _message_to_log);

            //tempParagraph.BringIntoView();
        }
        //method added to event to scroll main chat window to last added message
        private void loadedBlock(object sender, RoutedEventArgs e) {
            var bloc = sender as Block;
            if(bloc != null) {
                bloc.BringIntoView();
            }
        }

        //method to layout message before adding it to the main chat window
        //_message_ is a string that goes out of this method and returns constructed, stylised (in future :P) html message that will be written in log file.
        private Paragraph layoutMessage(string _nick, string _message, out string _message_) {
            Paragraph tempP = new Paragraph();
            var _datetime_ = DateTime.Now;

            _time_span_ = new Span(new Run("[" + _datetime_.ToString("HH:mm:ss") + "] "));
            _time_span_.Style = (Style)(this.Resources["_TIME_SPAN_"]);
            tempP.Inlines.Add(_time_span_);

            tempP.Inlines.Add(new Bold(new Run(_nick + ": ")));

            _message_span_ = new Span(new Run(_message));
            _message_span_.Style = (Style)(this.Resources["_ITALIC_SPAN_"]);
            tempP.Inlines.Add(_message_span_);

            _message_ = layoutForLogMessage(_datetime_.ToString("HH:mm:ss"), _nick, _message);

            return tempP;
        }

        private String layoutForLogMessage(string time, string nick, string msg) {
            string layoutedMessage = "[" + time + "] " + nick + ": " + msg + "<br />" + Environment.NewLine;

            return layoutedMessage;
        }

        private void GenerateLogFile() {
            if(!chatLog.CreateNewChatLog(_ROOM_NAME_)) {
                Application.Current.Shutdown();
            }
        }

        private void SaveToLog(string roomName, string _message_to_log) {
            if(!chatLog.WriteToChatLog(roomName, _message_to_log)) {
                Application.Current.Shutdown();
            }
        }
    }
}