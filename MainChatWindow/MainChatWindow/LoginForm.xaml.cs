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

    public partial class LoginForm : Window {

        MainWindow mainWindow;

        public LoginForm() {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e) {
            //Disable all input boxes so user can't change anything while program is trying to establish connection with server.
            DisableInputBoxes();

            //Safety check.
            if(!ValidateInput())
            {
                MessageBox.Show("You must enter your username and password to proceed.");
                EnableInputBoxes();
                return;
            }

            mainWindow = new MainWindow(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text, RegisterUserCheckBox.IsChecked.Value);

            this.Hide(); //Hide login form.
            mainWindow.Show(); //Show main chat window.
            try
            {
                this.Owner = mainWindow;
            }
            catch (Exception err)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Enabled all input boxes.
        /// </summary>
        private void EnableInputBoxes() {
            LoginTextBox.IsEnabled = true;
            PasswordTextBox.IsEnabled = true;
            RoomTextBox.IsEnabled = true;
        }
        /// <summary>
        /// Disables all input boxes.
        /// </summary>
        private void DisableInputBoxes() {
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            RoomTextBox.IsEnabled = false;
        }
        /// <summary>
        /// Validate input boxes.
        /// </summary>
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
    }
}
