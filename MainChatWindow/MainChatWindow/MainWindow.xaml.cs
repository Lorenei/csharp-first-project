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

        private Paragraph tempParagraph;
        private Span _time_span_;
        private Span _message_span_;
        public MainWindow() {
            InitializeComponent();
            //tempParagraph = new Paragraph();
        }

        //Event on key down added to textbox where user can type message to be sent to chat room
        private void UserMessageTextBox_OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                string tempString = UserMessageTextBox.Text;
                if(tempString != null && tempString != "")
                {
                    AddMessageToFlowDocument(tempString);
                    UserMessageTextBox.Clear();
                }                           
            }
        }

        //should be activated by server response about whether message was accepted or not
        private void AddMessageToFlowDocument(string message)
        {
            if(message == "" || message == null)
            {
                return;
            }

            tempParagraph = layoutMessage("Lily", message);
            tempParagraph.Loaded += loadedBlock;
            OknoChatowe.Document.Blocks.Add(tempParagraph);
            
            //tempParagraph.BringIntoView();
        }
        //method added to event to scroll main chat window to last added message
        private void loadedBlock(object sender, RoutedEventArgs e)
        {
            var bloc = sender as Block;
            if(bloc != null)
            {
                bloc.BringIntoView();
            }
        }

        //method to layout message before adding it to the main chat window
        private Paragraph layoutMessage(string _nick, string _message)
        {
            Paragraph tempP = new Paragraph();
            var _datetime_ = DateTime.Now;
            //_time_span_ = new Span(new Run("[" + _datetime_.Hour + ":" + _datetime_.Minute + ":" + _datetime_.Second + "] "));
            _time_span_ = new Span(new Run("[" + _datetime_.ToString("hh:mm:ss") + "] "));
            _time_span_.Style = (Style)(this.Resources["_TIME_SPAN_"]);
            tempP.Inlines.Add(_time_span_);

            tempP.Inlines.Add(new Bold(new Run(_nick + ": ")));

            _message_span_ = new Span(new Run(_message));
            _message_span_.Style = (Style)(this.Resources["_ITALIC_SPAN_"]);
            tempP.Inlines.Add(_message_span_);

            return tempP;
        }
    }
}
