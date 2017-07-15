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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window {

        ChatWindow chatWindow;

        public MainWindow() {
            InitializeComponent();
            chatWindow = new ChatWindow();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e) {
            //Disable all input boxes so user can't change anything while program is trying to establish connection with server.
            DisableInputBoxes();

            //łaczenie z serwerem w celu zalogowania
            //utworzenie nowego okna z danymi użytkownika + serwera?
            //ChatWindow chatWindow = new ChatWindow(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text);
            chatWindow.InitializeVariables(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text);
            chatWindow.InitializeClientCallback();
            while(!chatWindow.InitializeServerConnection()) {
                MessageBox.Show("Login failed, trying again.");
            }
            //Check whether new window was able to connect with server and establish neccesary log files
            if(chatWindow._IS_CONNECTION_DONE_) {
                this.Close();
                chatWindow.Show();
                chatWindow.
            }
            else {
                MessageBox.Show("Error while connecting to server or establishing log file. Please try again.");
                EnableInputBoxes();
                chatWindow.Close();
            }
        }

        private void EnableInputBoxes() {
            LoginTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            RoomTextBox.IsEnabled = true;
        }
        private void DisableInputBoxes() {
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            RoomTextBox.IsEnabled = false;
        }

        //Event on key down added to textbox where user can type message to be sent to chat room


    }
}
