using System;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window {

        //Service vars
        public static IChatService Server;
        private static DuplexChannelFactory<IChatService> _channelFactory;
        private ClientCallback _clientCallback;

        private Paragraph tempParagraph;
        private Span _time_span_;
        private Span _message_span_;
        private ChatLogAPI chatLog;

        private string _LOGIN_;
        private string _PASSWORD_;
        private string _ROOM_NAME_;

        public bool _IS_CONNECTION_DONE_ = false;

        private Dictionary<string, int> usersList;

        public MainWindow() {
            InitializeComponent(); //If it's possible it could be moved as last command so we won't need to use close if initialization fails?

            //_LOGIN_ = login;
            //_PASSWORD_ = password;
            //_ROOM_NAME_ = room;

            //Service initialization
            //_clientCallback = new ClientCallback();
            //_channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            //_channelFactory = new DuplexChannelFactory<IChatService>(_clientCallback, "ChatServiceEndPoint");
            //Server = _channelFactory.CreateChannel();

            //Login on server function here if it fails, change connection to false.
            //if(Server.Login(_LOGIN_, _PASSWORD_, _ROOM_NAME_) != 0) {
            //    _IS_CONNECTION_DONE_ = false;
            //}
            //else {
            //create new log file for current chat room
            //    chatLog = new ChatLogAPI();
            //    GenerateLogFile();

            //    _IS_CONNECTION_DONE_ = true;
            //}
            Dictionary<string, int> usersList = new Dictionary<string, int>();
            InitializeVariables("Lorenei", "brak", "test_room");
            InitializeClientCallback();
            //temp number for debug multi clients
            int tmpLogin = 0;
            _LOGIN_ += tmpLogin;
            while(!InitializeServerConnection()) {
                MessageBox.Show("Login failed, trying again.");
                tmpLogin++;
                _LOGIN_ = "Lorenei" + tmpLogin;
            }
            RefreshUsersList();
            InitializeLogFile();
        }
        public void AddUserToList(string userName, int userColor)
        {
            var myResources = Resources["UsersList"];
            string newString = "";
            foreach(var item in UsersListBox.Items)
            {
                newString += item;
            }
            //Dictionary<string, int> tempDic = new Dictionary<string, int>();
            usersList.Add(userName, userColor);
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            //UsersListBox.Items.Add(tempDic);
            //UsersListBox.ItemsSource.
            //MessageBox.Show(newString);
        }
        public void RefreshUsersList(Dictionary<string, int> _usersList = null) {
            
            if (_usersList != null)
            {
                usersList = _usersList;
            }
            else
            {
                usersList = Server.GetUsersList();
            }
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            //UsersListBox.Items.SortDescriptions.Clear();
            //UsersListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
            //foreach(var user in usersList) {
                //UsersListBox.
            //}
        }
        public void InitializeVariables(string userName, string userPassword, string userRoomName) {
            _LOGIN_ = userName;
            _PASSWORD_ = userPassword;
            _ROOM_NAME_ = userRoomName;
        }
        public void InitializeClientCallback() {
            _clientCallback = new ClientCallback();
        }
        public bool InitializeServerConnection() {
            _channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            Server = _channelFactory.CreateChannel();
            if(Server.Login(_LOGIN_, _PASSWORD_, _ROOM_NAME_) != 0) {
                return false;
            }
            else {
                return true;
            }
        }
        public void InitializeLogFile() {
            chatLog = new ChatLogAPI();
            GenerateLogFile();
        }
        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                string tempString = UserMessageTextBox.Text;
                if(tempString != null && tempString != "") {
                    //AddMessageToFlowDocument(tempString);
                    Server.SendMessageToAll(tempString, _LOGIN_);
                    AddMessageToFlowDocument(tempString, _LOGIN_);
                    UserMessageTextBox.Clear();
                    MessageBox.Show("Message sent");
                }
            }
        }

        //should be activated by server response about whether message was accepted or not
        public void AddMessageToFlowDocument(string message, string userName = "") {
            if(message == "" || message == null) {
                return;
            }
            //string that will be outed by layoutmessage method so we can write stylised with html message to our log file
            string _message_to_log;
            tempParagraph = layoutMessage(userName, message, out _message_to_log);
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

        private string layoutForLogMessage(string time, string nick, string msg) {
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
