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

        private Dictionary<string, string> usersList;

        private bool AmIBeingKicked = false;

        public MainWindow(string userName, string userPassword, string userRoomName, bool RegisterAsNewUser = false) {
            InitializeComponent(); //If it's possible it could be moved as last command so we won't need to use close if initialization fails?
            //Closing += new CancelEventHandler(LogoutFromServer);

            ColorPickerComboBox.ItemsSource = typeof(Colors).GetProperties();

            usersList = new Dictionary<string, string>();
            InitializeVariables(userName, userPassword, userRoomName);
            InitializeClientCallback();
            if(!InitializeServerConnection(RegisterAsNewUser))
            {
                MessageBox.Show("Failed to login. Application shutdown.");
                Application.Current.Shutdown();
            }
            //temp number for debug multi clients
            /*int tmpLogin = 0;
            _LOGIN_ += tmpLogin;
            while(!InitializeServerConnection()) {
                MessageBox.Show("Login failed, trying again.");
                tmpLogin++;
                _LOGIN_ = "Lorenei" + tmpLogin;
            }*/
            RefreshUsersList();
            InitializeLogFile();
            StatusTextBlock_1.Text = "Welcome " + userName + " to chat room! Have a nice day! ";
            StatusBarClock();
        }
        ~MainWindow() {
            if (!AmIBeingKicked && _IS_CONNECTION_DONE_) {
                CloseConnectionWithServer();
            }
        }
        private void CloseConnectionWithServer() {
            /*try {
                _channelFactory.Close();
            }
            catch(Exception e) {
                _channelFactory.Abort();
            }*/
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
        //Add single user to users list and refresh the list
        //Need some refactoring, since I don't think that replacing resources with whole new list is proper.
        //Need to find solution how to make automatically sorted resources that allow to add one item to it.
        public void AddUserToList(string userName, Color userColor)
        {
            usersList.Add(userName, userColor.ToString());
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            AddMessageToFlowDocument(userName + " joined chat room.");
        }
        //Remove single user from users list and refresh the list.
        //Need the same refactoring as AddUserToList().
        public void RemoveUserFromList(string userName)
        {
            usersList.Remove(userName);
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            AddMessageToFlowDocument(userName + " left chat room.");
        }
        //Refresh whole users list with list delivered in parameter or get new one from server.
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
        public void InitializeVariables(string userName, string userPassword, string userRoomName) {
            _LOGIN_ = userName;
            _PASSWORD_ = userPassword;
            _ROOM_NAME_ = userRoomName;
        }
        public void InitializeClientCallback() {
            _clientCallback = new ClientCallback();
        }
        public bool InitializeServerConnection(bool RegisterAsNewUser) {
            _channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            Server = _channelFactory.CreateChannel();
            
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
                Closing += new CancelEventHandler(LogoutFromServer);
                _IS_CONNECTION_DONE_ = true;
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
                    try
                    {
                        Server.SendMessageToAll(tempString, _LOGIN_);
                    }
                    catch(Exception err)
                    {
                        AddMessageToFlowDocument("Failed to send message to server. Error e: " + err.ToString());
                    }
                    AddMessageToFlowDocument(tempString, _LOGIN_);
                    UserMessageTextBox.Clear();
                    //MessageBox.Show("Message sent"); //Debug function
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
            if (userName != "" && userName != null) {
                tempParagraph = layoutMessage(userName, message, out _message_to_log);
            }
            else {
                tempParagraph = layoutMessage(userName, message, out _message_to_log, true);
            }
            //Adding loadedBlock event to event listener so when new message is rendered it will bring it to view (scroll to new message in chat)
            //It does work, but probably should be added just once at initialization or something..
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

        private void ShowIPButton_Click(object sender, RoutedEventArgs e) {
            if(UsersListBox.SelectedItem == null) {
                AddMessageToFlowDocument("Command Run -> Show IP of user: Failed! You have to select user from list.");
            }
            else {
                //Access to users list userName
                //MessageBox.Show(((KeyValuePair<string, int>) UsersListBox.SelectedItem).Key);
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                string ipAddress = Server.ShowIpInfo(_LOGIN_, selectedUser);
                string message = "Command Run -> Show IP of user: " + selectedUser + " -> IP : " + ipAddress;
                AddMessageToFlowDocument(message);
            }
        }

        public void YouHaveBeenKicked(string userName) {
            string message = "You have been kicked from server by: " + userName;
            AddMessageToFlowDocument(message);
            AmIBeingKicked = true;
            this.Close();
        }
        public void YouHaveBeenBanned(string userName) {
            AddMessageToFlowDocument("You have been banned from server by: " + userName);
            AmIBeingKicked = true;
            this.Close();
        }

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
                //This is temporary solution. It should be changed to properly kick someone.
                //But since I have no idea how to handle true two-way connection with server, I have to make sure
                //that server doesn't try to send 'remove user from list' function on the same channel.
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
                    //MessageBox.Show(_LOGIN_);
                    RemoveUserFromList(selectedUser);
                }
            }
        }

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

        private void OnColorPick(object sender, SelectionChangedEventArgs e)
        {
            Color selectedColor = (Color)(ColorPickerComboBox.SelectedItem as PropertyInfo).GetValue(null, null);
            UpdateColorForUser(_LOGIN_, selectedColor);
            AddMessageToFlowDocument("OnColorPick(object, selectionchangedeventargs): Zmieniono swój kolor na: " + selectedColor.ToString());
            try
            {
                Server.ChangeUserColor(_LOGIN_, selectedColor);
            }
            catch(Exception err)
            {
                AddMessageToFlowDocument("Failed to contact server about changing color. Error e: " + err.ToString());
            }
            AddMessageToFlowDocument("OnColorPick(object, selectionchangedeventargs): WYsłano informację do serwera o zmianie koloru użytkownika.");
        }

        public void GetUserColor(string userName, Color userColor)
        {
            if (usersList.ContainsKey(userName))
            {
                AddMessageToFlowDocument("GetUserColor(string,Color): Znaleziono klucz w słowniku.");
                UpdateColorForUser(userName, userColor);
            }
        }

        private void UpdateColorForUser(string userName, Color userColor)
        {
            usersList[userName] = userColor.ToString();
            AddMessageToFlowDocument("UpdateColorForUser(string,color): Podmieniono kolor użytkownika: " + userName + " na: " + userColor.ToString());
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            AddMessageToFlowDocument("UpdateColorForUser(string,color): Odświeżono resources.");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Arkadiusz Richert\narshenik@gmail.com\ngithub.com/Lorenei\n2017 - current.");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LogoutFromServer(new object(), new CancelEventArgs());
            CloseConnectionWithServer();
            Application.Current.Shutdown();
        }
        private void StatusBarClock()
        {
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                StatusTextBlock_3.Text = DateTime.Now.ToString("HH:mm:s");
            }, this.Dispatcher);
        }
    }
}
