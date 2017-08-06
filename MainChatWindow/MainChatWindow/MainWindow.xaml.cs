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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;

namespace ChatClient {
    
    public partial class MainWindow : Window {

        //Service variables
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

        private Dictionary<string, string> usersList;

        private bool AmIBeingKicked = false;

        public MainWindow(string userName, string userPassword, string userRoomName, bool RegisterAsNewUser = false) {
            InitializeComponent(); 

            //Fill the combo box with colors.
            ColorPickerComboBox.ItemsSource = typeof(Colors).GetProperties();

            usersList = new Dictionary<string, string>();
            InitializeVariables(userName, userPassword, userRoomName);
            InitializeClientCallback();
            //Try to connect to server.
            if(!InitializeServerConnection(RegisterAsNewUser))
            {
                MessageBox.Show("Failed to login. Application shutdown.");
                Application.Current.Shutdown();
            }

            RefreshUsersList();
            InitializeLogFile();
            StatusTextBlock_1.Text = "Welcome " + userName + " to chat room! Have a nice day! ";
            StatusBarClock();
        }
        /// <summary>
        /// Destroyer method - called just before class is destroyed.
        /// Try to close connection with server when closing chat window.
        /// </summary>
        ~MainWindow() {
            if (!AmIBeingKicked && _IS_CONNECTION_DONE_) {
                CloseConnectionWithServer();
            }
        }
        /// <summary>
        /// Abort connection with server.
        /// Should be trying to close it and if it fails aborting. But whatever.
        /// </summary>
        private void CloseConnectionWithServer() {
            _channelFactory.Abort();
        }
        //Send logout request to server before closing app window.
        private void LogoutFromServer(object sender, CancelEventArgs e)
        {
            if (AmIBeingKicked) return;
            try
            {
                Server.Logout(_LOGIN_, _ROOM_NAME_);
            }
            catch(Exception err)
            {
                AddMessageToFlowDocument("Failed to contact server about logout. Error e: " + err.ToString());
            }
            _channelFactory.Abort();
        }
        /// <summary>
        /// Add single new user to users list.
        /// </summary>
        public void AddUserToList(string userName, Color userColor)
        {
            usersList.Add(userName, userColor.ToString());
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            AddMessageToFlowDocument(userName + " joined chat room.");
        }
        /// <summary>
        /// Remove single user from users list.
        /// </summary>
        public void RemoveUserFromList(string userName)
        {
            usersList.Remove(userName);
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            AddMessageToFlowDocument(userName + " left chat room.");
        }
        /// <summary>
        /// Refresh whole users list.
        /// </summary>
        public void RefreshUsersList(Dictionary<string, string> _usersList = null) {
            
            if (_usersList != null)
            {
                usersList = _usersList;
            }
            else
            {
                usersList = Server.GetUsersList();
            }
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
        }
        /// <summary>
        /// Set basic variables that were taken from login form.
        /// </summary>
        public void InitializeVariables(string userName, string userPassword, string userRoomName) {
            _LOGIN_ = userName;
            _PASSWORD_ = userPassword;
            _ROOM_NAME_ = userRoomName;
        }
        /// <summary>
        /// Create callback channel.
        /// </summary>
        public void InitializeClientCallback() {
            _clientCallback = new ClientCallback();
        }
        /// <summary>
        /// Create connection with service.
        /// </summary>
        public bool InitializeServerConnection(bool RegisterAsNewUser) {
            _channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            Server = _channelFactory.CreateChannel();
            
            //If user wants to be registered, negiotiate with server.
            if(RegisterAsNewUser)
            {
                if(!Server.RegisterNewUserToDB(_LOGIN_, _PASSWORD_)) {
                    MessageBox.Show("Registration failed!");
                    return false;
                }
                MessageBox.Show("Registration completed succesfully.");
            }

            if(Server.Login(_LOGIN_, _PASSWORD_, _ROOM_NAME_) != 0) {
                return false;
            }
            else {
                //Add LogoutFromServer event only after login is successful. Else application would
                //try to disconnect from nonexistent server.
                Closing += new CancelEventHandler(LogoutFromServer);
                _IS_CONNECTION_DONE_ = true;
                return true;
            }
        }
        /// <summary>
        /// Create chat logs.
        /// </summary>
        public void InitializeLogFile() {
            chatLog = new ChatLogAPI();
            GenerateLogFile();
        }
        /// <summary>
        /// Receiver of "Enter" key press when focusing message box.
        /// </summary>
        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                string tempString = UserMessageTextBox.Text;
                if(tempString != null && tempString != "") {
                    try
                    {
                        Server.SendMessageToAll(tempString, _LOGIN_); //Send message to server (and other clients)
                    }
                    catch(Exception err)
                    {
                        AddMessageToFlowDocument("Failed to send message to server. Error e: " + err.ToString());
                    }
                    AddMessageToFlowDocument(tempString, _LOGIN_); //Add your own message directly to chat window.
                    UserMessageTextBox.Clear(); //Clear message text box.
                }
            }
        }

        /// <summary>
        /// Method that adds text to chat window. It also generates lines for chat log.
        /// Can also handle commands texts.
        /// </summary>
        public void AddMessageToFlowDocument(string message, string userName = "") {
            if(message == "" || message == null) {
                return;
            }
            //string that will be outed by layoutmessage method so we can write stylised with html message to our log file
            string _message_to_log;
            if (userName != "" && userName != null) {
                tempParagraph = layoutMessage(userName, message, out _message_to_log);
            }
            else {
                tempParagraph = layoutMessage(userName, message, out _message_to_log, true);
            }
            //Adding loadedBlock event to event listener so when new message is rendered it will bring it to view (scroll to new message in chat)
            //It does work, but probably should be added just once at initialization or something..
            //This event is needed since BringToView function right now would try to bring to view elements that are not yet completely rendered.
            tempParagraph.Loaded += loadedBlock;
            OknoChatowe.Document.Blocks.Add(tempParagraph);

            //Write our message to our log file
            SaveToLog(_ROOM_NAME_, _message_to_log);
        }
        /// <summary>
        /// Method added to event to scroll main chat window to last added message 
        /// </summary>
        private void loadedBlock(object sender, RoutedEventArgs e) {
            var bloc = sender as Block;
            if(bloc != null) {
                bloc.BringIntoView();
            }
        }
        /// <summary>
        /// Method to layout message before adding it to the main chat window
        /// _message_ is a string that goes out of this method and returns constructed, stylised (in future :P) html message that will be written in log file. 
        /// </summary>
        private Paragraph layoutMessage(string _nick, string _message, out string _message_, bool isThisCommand = false) {
            Paragraph tempP = new Paragraph();
            var _datetime_ = DateTime.Now;

            _time_span_ = new Span(new Run("[" + _datetime_.ToString("HH:mm:ss") + "] "));
            _time_span_.Style = (Style)(this.Resources["_TIME_SPAN_"]);
            tempP.Inlines.Add(_time_span_);

            if (!isThisCommand) {
                tempP.Inlines.Add(new Bold(new Run(_nick + ": ")));
            }

            _message_span_ = new Span(new Run(_message));
            if (!isThisCommand) {
                _message_span_.Style = (Style)(this.Resources["_ITALIC_SPAN_"]);
            }
            else {
                _message_span_.Style = (Style)(this.Resources["_COMMAND_SPAN_"]);
            }
            tempP.Inlines.Add(_message_span_);

            _message_ = layoutForLogMessage(_datetime_.ToString("HH:mm:ss"), _nick, _message);

            return tempP;
        }
        /// <summary>
        /// Method that layouts our message for log file.
        /// </summary>
        private string layoutForLogMessage(string time, string nick, string msg) {
            string layoutedMessage = "[" + time + "] " + nick + ": " + msg + "<br />" + Environment.NewLine;

            return layoutedMessage;
        }

        /// <summary>
        /// Method that creates new chat log.
        /// </summary>
        private void GenerateLogFile() {
            if(!chatLog.CreateNewChatLog(_ROOM_NAME_)) {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Method that saves message to log file.
        /// </summary>
        private void SaveToLog(string roomName, string _message_to_log) {
            if(!chatLog.WriteToChatLog(roomName, _message_to_log)) {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Method that responds to IP button.
        /// </summary>
        private void ShowIPButton_Click(object sender, RoutedEventArgs e) {
            if(UsersListBox.SelectedItem == null) {
                AddMessageToFlowDocument("Command Run -> Show IP of user: Failed! You have to select user from list.");
            }
            else {
                //Access to users list userName
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                try
                {
                    string ipAddress = Server.ShowIpInfo(_LOGIN_, selectedUser);
                    string message = "Command Run -> Show IP of user: " + selectedUser + " -> IP : " + ipAddress;
                    AddMessageToFlowDocument(message);
                }
                catch(Exception err)
                {
                    AddMessageToFlowDocument("Command Run -> Show IP of user: failed!");
                }
            }
        }
        /// <summary>
        /// Method that responds to user being kicked.
        /// </summary>
        public void YouHaveBeenKicked(string userName) {
            string message = "You have been kicked from server by: " + userName;
            AddMessageToFlowDocument(message);
            AmIBeingKicked = true;
            this.Close();
        }
        /// <summary>
        /// Method that responds to user being banned.
        /// </summary>
        public void YouHaveBeenBanned(string userName) {
            AddMessageToFlowDocument("You have been banned from server by: " + userName);
            AmIBeingKicked = true;
            this.Close();
        }
        /// <summary>
        /// Method that responds to Kick button
        /// </summary>
        private void KickUserButton_Click(object sender, RoutedEventArgs e) {
            if (UsersListBox.SelectedItem == null) {
                AddMessageToFlowDocument("Command Run -> Kick user: Failed! You have to select user from list.");
            }
            else if(((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key == _LOGIN_) {
                AddMessageToFlowDocument("Command Run -> Kick user: Failed! You can't kick yourself.");
            }
            else {
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                string message = "Command Run -> Kick user: " + selectedUser + ". Request have been sent to server.";
                AddMessageToFlowDocument(message);

                bool tempKick = false;
                try
                {
                    tempKick = Server.KickUserFromService(_LOGIN_, selectedUser, _ROOM_NAME_);
                }
                catch(Exception err)
                {
                    AddMessageToFlowDocument("Failed to contact server about kicking user. Error e: " + err.ToString());
                }
                if(tempKick) {
                    RemoveUserFromList(selectedUser);
                }
            }
        }
        /// <summary>
        /// Method that responds to Ban button.
        /// </summary>
        private void BanUserButton_Click(object sender, RoutedEventArgs e) {
            if (UsersListBox.SelectedItem == null) {
                AddMessageToFlowDocument("Command Run -> Ban User: Failed! You have to select user from list.");
            }
            else if (((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key == _LOGIN_) {
                AddMessageToFlowDocument("Command Run -> Ban user: Failed! You can't ban yourself.");
            }
            else {
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                AddMessageToFlowDocument("Command Run -> Ban user: " + selectedUser + ". Request have been sent to server.");
                if(Server.BanUserFromService(_LOGIN_, selectedUser, _ROOM_NAME_)) {
                    RemoveUserFromList(selectedUser);
                }
            }
        }
        /// <summary>
        /// Method that responds to color being picked.
        /// </summary>
        private void OnColorPick(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(ColorPickerComboBox.SelectedItem as PropertyInfo).GetValue(null, null);
            UpdateColorForUser(_LOGIN_, selectedColor);
            try
            {
                Server.ChangeUserColor(_LOGIN_, selectedColor);
            }
            catch(Exception err)
            {
                AddMessageToFlowDocument("Failed to contact server about changing color. Error e: " + err.ToString());
            }
            AddMessageToFlowDocument("Server has been contacted about your color change.");
        }
        /// <summary>
        /// Method that responds to other users color change.
        /// </summary>
        public void GetUserColor(string userName, Color userColor)
        {
            if (usersList.ContainsKey(userName))
            {
                UpdateColorForUser(userName, userColor);
            }
        }
        /// <summary>
        /// Method that updates color for users.
        /// </summary>
        private void UpdateColorForUser(string userName, Color userColor)
        {
            usersList[userName] = userColor.ToString();
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
        }
        /// <summary>
        /// Method that responds to menu bar item About.
        /// </summary>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Arkadiusz Richert\narshenik@gmail.com\ngithub.com/Lorenei\n2017 - current.");
        }
        /// <summary>
        /// Method that responds to menu bar item Exit.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LogoutFromServer(new object(), new CancelEventArgs());
            CloseConnectionWithServer();
            Application.Current.Shutdown();
        }
        /// <summary>
        /// Method that makes clock to show on status bar.
        /// </summary>
        private void StatusBarClock()
        {
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                StatusTextBlock_3.Text = DateTime.Now.ToString("HH:mm:s");
            }, this.Dispatcher);
        }
    }
}
