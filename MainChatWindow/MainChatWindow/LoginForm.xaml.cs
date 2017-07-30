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

namespace ChatClient {
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window {

        MainWindow mainWindow;

        public LoginForm() {
            InitializeComponent();
            //mainWindow = new MainWindow();
            //this.Owner = chatWindow;
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e) {
            //Disable all input boxes so user can't change anything while program is trying to establish connection with server.
            DisableInputBoxes();

            if(!ValidateInput())
            {
                MessageBox.Show("You must enter your username and password to proceed.");
                EnableInputBoxes();
                return;
            }
            if(RegisterUserCheckBox.IsChecked == true)
            {
                //Will have to add server connection to loginform so we can register, and try to login here instead of already in chat window.
                /*if(!RegisterNewUser())
                {
                    MessageBox.Show("Failed to register new user. Please try different user name.");
                    EnableInputBoxes();
                    return;
                }
                else
                {
                    MessageBox.Show("Registration completed. You will be loged in now.");
                }*/
            }

            mainWindow = new MainWindow(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text, RegisterUserCheckBox.IsChecked.Value);
            //mainWindow.InitializeVariables(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text);
            //if (!mainWindow._IS_CONNECTION_DONE_)
            //{
            //MessageBox.Show("Failed to login!");
            //EnableInputBoxes();
            //return;
            //}

            //mainWindow.InitializeLogFile();
            //mainWindow.InitializeClientCallback();
            //mainWindow.RefreshUsersList();
            this.Hide();
            mainWindow.Show();
            try
            {
                this.Owner = mainWindow;
            }
            catch (Exception err)
            {
                Application.Current.Shutdown();
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
        private bool ValidateInput()
        {
            if(PasswordTextBox.Password == "" || PasswordTextBox.Password == null || LoginTextBox.Text == "" || LoginTextBox.Text == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool RegisterNewUser()
        {
            
            return false;
        }
    }
}
