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

namespace ChatClient
{

    /// <summary>
    /// Main Chat Window class.
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {

=       /// <summary>
        /// The server variable that will be used to communicate with server.
        /// </summary>
        public static IChatService Server;
        /// <summary>
        /// The channel factory that will be used to create channel connection with server.
        /// </summary>
        private static DuplexChannelFactory<IChatService> _channelFactory;
        /// <summary>
        /// The client callback variable. Created here, initialized at early time.
        /// To make sure the callback class will hold reference to MainWindow and wont be null.
        /// If it's created dynamically, it can't find MainWindow, since it's created before 
        /// MainWindow finishes creating itself.
        /// </summary>
        private ClientCallback _clientCallback;

        /// <summary>
        /// The temporary paragraph.
        /// Used to create new block of text that will be added to FlowDocument.
        /// </summary>
        private Paragraph tempParagraph;
        /// <summary>
        /// The time span.
        /// Used to create part of paragraph block.
        /// </summary>
        private Span _time_span_;
        /// <summary>
        /// The message span.
        /// Used to create part of paragraph block.
        /// </summary>
        private Span _message_span_;
        /// <summary>
        /// The chat log.
        /// Exposes API for chat logging.
        /// </summary>
        private ChatLogAPI chatLog;

        /// <summary>
        /// The user login.
        /// </summary>
        private string _LOGIN_;
        /// <summary>
        /// The user password.
        /// </summary>
        private string _PASSWORD_;
        /// <summary>
        /// The chat room name.
        /// </summary>
        private string _ROOM_NAME_;

        /// <summary>
        /// Set to true when connection with server was established.
        /// Used to not inform server about application exit if connection wasn't established at all - else may crash/timeout application.
        /// </summary>
        public bool _IS_CONNECTION_DONE_ = false;

        /// <summary>
        /// The users list containing string,string as user name and user color.
        /// </summary>
        private Dictionary<string, string> usersList;

        /// <summary>
        /// Bool that is set to true if client was kicked or banned by other user.
        /// Used similarly to _IS_CONNECTION_DONE_ - so there won't be useless try to connect server about logging out, since server already removed this user.
        /// </summary>
        private bool AmIBeingKicked = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <param name="userRoomName">Name of the user chat room.</param>
        /// <param name="RegisterAsNewUser">if set to <c>true</c> [register as new user].</param>
        public MainWindow(string userName, string userPassword, string userRoomName, bool RegisterAsNewUser = false)
        {
            InitializeComponent();

            //Fill the combo box with colors as items to choose from.
            ColorPickerComboBox.ItemsSource = typeof(Colors).GetProperties();

            //Initialize empty users list.
            usersList = new Dictionary<string, string>();
            InitializeVariables(userName, userPassword, userRoomName);
            InitializeClientCallback();
            //Try to connect to server.
            //And register as new user if such option was selected.
            if (!InitializeServerConnection(RegisterAsNewUser))
            {
                MessageBox.Show("Failed to login. Application shutdown.");
                Application.Current.Shutdown();
            }
            //Get whole users list from server.
            RefreshUsersList();
            //Create log file to log chat talk.
            InitializeLogFile();
            StatusTextBlock_1.Text = "Welcome " + userName + " to chat room! Have a nice day! ";
            StatusBarClock();
        }
        /// <summary>
        /// Destroyer method - called just before class is destroyed.
        /// Try to close connection with server when closing chat window.
        /// Don't do that if client has been kicked, banned, or server connection wasn't established at all.
        /// </summary>
        ~MainWindow()
        {
            if (!AmIBeingKicked && _IS_CONNECTION_DONE_)
            {
                CloseConnectionWithServer();
            }
        }
        /// <summary>
        /// Abort connection with server.
        /// Should be trying to close it and if it fails aborting. But whatever.
        /// </summary>
        private void CloseConnectionWithServer()
        {
            _channelFactory.Abort();
        }
        /// <summary>
        /// Logouts from server.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
        private void LogoutFromServer(object sender, CancelEventArgs e)
        {
            if (AmIBeingKicked) return;
            try
            {
                Server.Logout(_LOGIN_, _ROOM_NAME_);
            }
            catch (Exception err)
            {
                AddMessageToFlowDocument("Failed to contact server about logout. Error e: " + err.ToString());
            }
            _channelFactory.Abort();
        }
        /// <summary>
        /// Add single new user to users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
        public void AddUserToList(string userName, Color userColor)
        {
            usersList.Add(userName, userColor.ToString());
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            //Show notification about new user joining room.
            AddMessageToFlowDocument(userName + " joined chat room.");
        }
        /// <summary>
        /// Remove single user from users list.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public void RemoveUserFromList(string userName)
        {
            usersList.Remove(userName);
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
            //Show notification about user leaving room.
            AddMessageToFlowDocument(userName + " left chat room.");
        }
        /// <summary>
        /// Refresh whole users list.
        /// </summary>
        /// <param name="_usersList">The users list. Can be null to force whole list refresh from server.</param>
        public void RefreshUsersList(Dictionary<string, string> _usersList = null)
        {

            if (_usersList != null)
            {
                usersList = _usersList;
            }
            else
            {
                usersList = Server.GetUsersList();
            }
            //Set dynamic resources to ordered dictionary to refresh items on list.
            Resources["UsersList"] = usersList.OrderBy(user => user.Key);
        }
        /// <summary>
        /// Set basic variables that were taken from login form.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <param name="userRoomName">Name of the user room.</param>
        public void InitializeVariables(string userName, string userPassword, string userRoomName)
        {
            _LOGIN_ = userName;
            _PASSWORD_ = userPassword;
            _ROOM_NAME_ = userRoomName;
        }
        /// <summary>
        /// Create callback channel.
        /// </summary>
        public void InitializeClientCallback()
        {
            _clientCallback = new ClientCallback();
        }
        /// <summary>
        /// Create connection with service.
        /// </summary>
        /// <param name="RegisterAsNewUser">if set to <c>true</c> [register as new user].</param>
        /// <returns></returns>
        public bool InitializeServerConnection(bool RegisterAsNewUser)
        {
            //Provide information to create duplex channel factory that will allow two way communication to and from server.
            _channelFactory = new DuplexChannelFactory<IChatService>(new ClientCallback(), "ChatServiceEndPoint");
            //Create new channel that will allow communication with server.
            Server = _channelFactory.CreateChannel();

            //If user wants to be registered, negiotiate with server.
            if (RegisterAsNewUser)
            {
                if (!Server.RegisterNewUserToDB(_LOGIN_, _PASSWORD_))
                {
                    MessageBox.Show("Registration failed!");
                    return false;
                }
                MessageBox.Show("Registration completed succesfully.");
            }
            //Try to login to server with provided data.
            if (Server.Login(_LOGIN_, _PASSWORD_, _ROOM_NAME_) != 0)
            {
                return false;
            }
            else
            {
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
        public void InitializeLogFile()
        {
            chatLog = new ChatLogAPI();
            GenerateLogFile();
        }
        /// <summary>
        /// Receiver of "Enter" key press when focusing message box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.</param>
        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            //Check if used key was Enter
            if (e.Key == Key.Return)
            {
                string tempString = UserMessageTextBox.Text;
                //Check if message isn't empty or null.
                if (tempString != null && tempString != "")
                {
                    try
                    {
                        Server.SendMessageToAll(tempString, _LOGIN_); //Send message to server (and other clients)
                    }
                    catch (Exception err)
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
        /// <param name="message">The message.</param>
        /// <param name="userName">Name of the user that send the message. Empty if method is used as command run instead of message from users.</param>
        public void AddMessageToFlowDocument(string message, string userName = "")
        {
            if (message == "" || message == null)
            {
                return;
            }
            //string that will be outed by layoutmessage method so we can write stylised with html message to our log file
            string _message_to_log;
            if (userName != "" && userName != null)
            {
                tempParagraph = layoutMessage(userName, message, out _message_to_log);
            }
            else
            {
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
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void loadedBlock(object sender, RoutedEventArgs e)
        {
            var bloc = sender as Block;
            if (bloc != null)
            {
                bloc.BringIntoView();
            }
        }
        /// <summary>
        /// Method to layout message before adding it to the main chat window
        /// _message_ is a string that goes out of this method and returns constructed, stylised (in future :P) html message that will be written in log file.
        /// </summary>
        /// <param name="_nick">The name of user that sends message.</param>
        /// <param name="_message">The message.</param>
        /// <param name="_message_">The message that will be outed for log write.</param>
        /// <param name="isThisCommand">if set to <c>true</c> [is this command]. Which means this isn't message from user but a command that shouldn't include users Name, and should be build from other style.</param>
        /// <returns></returns>
        private Paragraph layoutMessage(string _nick, string _message, out string _message_, bool isThisCommand = false)
        {
            Paragraph tempP = new Paragraph();
            //Get current time.
            var _datetime_ = DateTime.Now;
            //Style span component that holds current Time.
            _time_span_ = new Span(new Run("[" + _datetime_.ToString("HH:mm:ss") + "] "));
            //Sets Time Span style to the one specified in xaml resources.
            //In future release may be customized by user.
            _time_span_.Style = (Style)(this.Resources["_TIME_SPAN_"]);
            tempP.Inlines.Add(_time_span_);

            //If this isn't command type of message, add users name to paragraph.
            if (!isThisCommand)
            {
                tempP.Inlines.Add(new Bold(new Run(_nick + ": ")));
            }
            _message_span_ = new Span(new Run(_message));
            //If it's normal message use italic style from xaml resources, if it's not user command style.
            if (!isThisCommand)
            {
                _message_span_.Style = (Style)(this.Resources["_ITALIC_SPAN_"]);
            }
            else
            {
                _message_span_.Style = (Style)(this.Resources["_COMMAND_SPAN_"]);
            }
            tempP.Inlines.Add(_message_span_);
            //Create outed message that will be used to store in chat log file.
            _message_ = layoutForLogMessage(_datetime_.ToString("HH:mm:ss"), _nick, _message);

            return tempP;
        }
        /// <summary>
        /// Method that layouts our message for log file.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="nick">The nick.</param>
        /// <param name="msg">The message.</param>
        /// <returns></returns>
        private string layoutForLogMessage(string time, string nick, string msg)
        {
            string layoutedMessage = "[" + time + "] " + nick + ": " + msg + "<br />" + Environment.NewLine;

            return layoutedMessage;
        }

        /// <summary>
        /// Method that creates new chat log.
        /// </summary>
        private void GenerateLogFile()
        {
            if (!chatLog.CreateNewChatLog(_ROOM_NAME_))
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Method that saves message to log file.
        /// </summary>
        /// <param name="roomName">Name of the room.</param>
        /// <param name="_message_to_log">The message to log.</param>
        private void SaveToLog(string roomName, string _message_to_log)
        {
            if (!chatLog.WriteToChatLog(roomName, _message_to_log))
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Method that responds to IP button.
        /// </summary>
        private void ShowIPButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if there's some user selected in users list.
            if (UsersListBox.SelectedItem == null)
            {
                AddMessageToFlowDocument("Command Run -> Show IP of user: Failed! You have to select user from list.");
            }
            else
            {
                //Access to users list userName
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                try
                {
                    string ipAddress = Server.ShowIpInfo(_LOGIN_, selectedUser);
                    string message = "Command Run -> Show IP of user: " + selectedUser + " -> IP : " + ipAddress;
                    AddMessageToFlowDocument(message);
                }
                catch (Exception err)
                {
                    AddMessageToFlowDocument("Command Run -> Show IP of user: failed!");
                }
            }
        }
        /// <summary>
        /// Method that responds to user being kicked.
        /// </summary>
        /// <param name="userName">Name of the user that kicked the client.</param>
        public void YouHaveBeenKicked(string userName)
        {
            string message = "You have been kicked from server by: " + userName;
            AddMessageToFlowDocument(message);
            AmIBeingKicked = true;
            this.Close();
        }
        /// <summary>
        /// Method that responds to user being banned.
        /// </summary>
        /// <param name="userName">Name of the user that banned the client.</param>
        public void YouHaveBeenBanned(string userName)
        {
            AddMessageToFlowDocument("You have been banned from server by: " + userName);
            AmIBeingKicked = true;
            this.Close();
        }
        /// <summary>
        /// Method that responds to Kick button
        /// </summary>
        private void KickUserButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if there's some user selected in users list.
            if (UsersListBox.SelectedItem == null)
            {
                AddMessageToFlowDocument("Command Run -> Kick user: Failed! You have to select user from list.");
            }
            else if (((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key == _LOGIN_)
            {
                AddMessageToFlowDocument("Command Run -> Kick user: Failed! You can't kick yourself.");
            }
            else
            {
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                string message = "Command Run -> Kick user: " + selectedUser + ". Request have been sent to server.";
                AddMessageToFlowDocument(message);

                bool tempKick = false;
                try
                {
                    tempKick = Server.KickUserFromService(_LOGIN_, selectedUser, _ROOM_NAME_);
                }
                catch (Exception err)
                {
                    AddMessageToFlowDocument("Failed to contact server about kicking user. Error e: " + err.ToString());
                }
                if (tempKick)
                {
                    RemoveUserFromList(selectedUser);
                }
            }
        }
        /// <summary>
        /// Method that responds to Ban button.
        /// </summary>
        private void BanUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListBox.SelectedItem == null)
            {
                AddMessageToFlowDocument("Command Run -> Ban User: Failed! You have to select user from list.");
            }
            else if (((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key == _LOGIN_)
            {
                AddMessageToFlowDocument("Command Run -> Ban user: Failed! You can't ban yourself.");
            }
            else
            {
                string selectedUser = ((KeyValuePair<string, string>)UsersListBox.SelectedItem).Key;
                AddMessageToFlowDocument("Command Run -> Ban user: " + selectedUser + ". Request have been sent to server.");
                if (Server.BanUserFromService(_LOGIN_, selectedUser, _ROOM_NAME_))
                {
                    RemoveUserFromList(selectedUser);
                }
            }
        }
        /// <summary>
        /// Method that responds to color being picked.
        /// </summary>
        private void OnColorPick(object sender, SelectionChangedEventArgs e)
        {
            //Gets selected color from Combo Box item selection.
            Color selectedColor = (Color)(ColorPickerComboBox.SelectedItem as PropertyInfo).GetValue(null, null);
            UpdateColorForUser(_LOGIN_, selectedColor);
            try
            {
                Server.ChangeUserColor(_LOGIN_, selectedColor);
            }
            catch (Exception err)
            {
                AddMessageToFlowDocument("Failed to contact server about changing color. Error e: " + err.ToString());
            }
            AddMessageToFlowDocument("Server has been contacted about your color change.");
        }
        /// <summary>
        /// Method that responds to other users color change.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
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
        /// <param name="userName">Name of the user.</param>
        /// <param name="userColor">Color of the user.</param>
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
