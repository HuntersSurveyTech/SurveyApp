﻿#pragma checksum "c:\Development\Storage\Hunters\HuntersWP\HuntersWP\Pages\ElementsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7ED211D908781FABA6F780A4B5756975"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using HuntersWP.Models;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace HuntersWP.Pages {
    
    
    public partial class ElementsPage : HuntersWP.Models.BasePage {
        
        internal HuntersWP.Models.BasePage page;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Grid qRefGrid;
        
        internal System.Windows.Controls.TextBlock tbQuestionRef;
        
        internal System.Windows.Controls.TextBlock tbImage;
        
        internal System.Windows.Controls.TextBlock tbQuestionHeading;
        
        internal System.Windows.Controls.TextBlock tbCurrentQNmber;
        
        internal System.Windows.Controls.TextBlock tbQuestionCount;
        
        internal System.Windows.Controls.StackPanel cmb1Panel;
        
        internal Microsoft.Phone.Controls.ListPicker cmb1;
        
        internal System.Windows.Controls.StackPanel tb1ForCmb1Panel;
        
        internal System.Windows.Controls.TextBox tb1ForCmb1;
        
        internal System.Windows.Controls.StackPanel panel2;
        
        internal Microsoft.Phone.Controls.ListPicker cmb2;
        
        internal Microsoft.Phone.Controls.WrapPanel panelTextBoxes;
        
        internal System.Windows.Controls.TextBox tbComment;
        
        internal System.Windows.Controls.TextBlock tbQuestionNum;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/HuntersWP;component/Pages/ElementsPage.xaml", System.UriKind.Relative));
            this.page = ((HuntersWP.Models.BasePage)(this.FindName("page")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.qRefGrid = ((System.Windows.Controls.Grid)(this.FindName("qRefGrid")));
            this.tbQuestionRef = ((System.Windows.Controls.TextBlock)(this.FindName("tbQuestionRef")));
            this.tbImage = ((System.Windows.Controls.TextBlock)(this.FindName("tbImage")));
            this.tbQuestionHeading = ((System.Windows.Controls.TextBlock)(this.FindName("tbQuestionHeading")));
            this.tbCurrentQNmber = ((System.Windows.Controls.TextBlock)(this.FindName("tbCurrentQNmber")));
            this.tbQuestionCount = ((System.Windows.Controls.TextBlock)(this.FindName("tbQuestionCount")));
            this.cmb1Panel = ((System.Windows.Controls.StackPanel)(this.FindName("cmb1Panel")));
            this.cmb1 = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("cmb1")));
            this.tb1ForCmb1Panel = ((System.Windows.Controls.StackPanel)(this.FindName("tb1ForCmb1Panel")));
            this.tb1ForCmb1 = ((System.Windows.Controls.TextBox)(this.FindName("tb1ForCmb1")));
            this.panel2 = ((System.Windows.Controls.StackPanel)(this.FindName("panel2")));
            this.cmb2 = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("cmb2")));
            this.panelTextBoxes = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("panelTextBoxes")));
            this.tbComment = ((System.Windows.Controls.TextBox)(this.FindName("tbComment")));
            this.tbQuestionNum = ((System.Windows.Controls.TextBlock)(this.FindName("tbQuestionNum")));
        }
    }
}

