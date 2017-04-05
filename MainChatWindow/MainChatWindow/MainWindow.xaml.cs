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

namespace MainChatWindow {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window {

        Paragraph tempParagraph;
        public MainWindow() {
            InitializeComponent();
            //tempParagraph = new Paragraph();
        }

        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                //< Paragraph FontFamily = "Segoe UI Emoji" FontSize = "13" >
                //< Span Foreground = "Gray" >[15:15:15] </ Span >
                //< Bold > Lorenei:</ Bold >
                //< Italic > No coś tam 😖 napiszemy żeby było widać że to chat no nie ?
                //</ Italic >
                //< Image Source = "emots/1f609.png" Width = "16" ></ Image >
                //</ Paragraph >
                string tempString = UserMessageTextBox.Text;
                if(tempString != null && tempString != "")
                {
                    AddMessageToFlowDocument(tempString);
                    UserMessageTextBox.Clear();
                }                           
            }
        }

        private void AddMessageToFlowDocument(string message)
        {
            if(message == "" || message == null)
            {
                return;
            }

            tempParagraph = new Paragraph(new Run(message));
            tempParagraph.Loaded += loadedBlock;
            OknoChatowe.Document.Blocks.Add(tempParagraph);
            
            //tempParagraph.BringIntoView();
        }

        private void loadedBlock(object sender, RoutedEventArgs e)
        {
            var bloc = sender as Block;
            if(bloc != null)
            {
                bloc.BringIntoView();
            }
        }
    }
}
