using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Netmera
{
    /// <summary>
    /// Contains WebBrowser control for Twitter and Facebook login operations
    /// </summary>
    public partial class LoginUserControl : UserControl
    {
        /// <summary>
        /// loginusercontrol()
        /// </summary>
        public LoginUserControl()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Popup popup = this.Parent as Popup;
            popup.IsOpen = false;
        }
    }
}
