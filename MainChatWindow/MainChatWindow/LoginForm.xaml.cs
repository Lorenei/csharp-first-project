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
    /// Logic for login form window.
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class LoginForm : Window {

        MainWindow mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginForm"/> class.
        /// </summary>
        public LoginForm() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the SignInButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            //Disable all input boxes so user can't change anything while program is trying to establish connection with server.
            DisableInputBoxes();

            //Safety check.
            if (!ValidateInput())
            {
                MessageBox.Show("You must enter your username and password to proceed.");
                EnableInputBoxes();
                return;
            }

            //Creates new instance of MainWindow to start server connection process - giving all needed data into constructor.
            mainWindow = new MainWindow(LoginTextBox.Text, PasswordTextBox.Password, RoomTextBox.Text, RegisterUserCheckBox.IsChecked.Value);

            this.Hide(); //Hide login form.
            mainWindow.Show(); //Show main chat window.
            try
            {
                this.Owner = mainWindow; //Try to make mainWindow owner of application.
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception err)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Enabled all input boxes.
        /// </summary>
        private void EnableInputBoxes()
        {
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
