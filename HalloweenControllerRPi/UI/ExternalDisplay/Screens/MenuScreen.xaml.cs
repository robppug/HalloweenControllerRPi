﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HalloweenControllerRPi.Device.Controllers.Providers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace HalloweenControllerRPi.UI.ExternalDisplay
{
    public sealed partial class MenuScreen : UserControl, IMenuButtonUser
    {
        public MenuScreen()
        {
            this.InitializeComponent();

            MenuFunctions = new Dictionary<MenuButton, MenuNode<UserControl>>();

            MenuFunctions.Add(MenuButton.Left, new MenuNode<UserControl>(null, new DetectingScreen()));
            MenuFunctions.Add(MenuButton.Right, new MenuNode<UserControl>(null, new DetectingScreen()));

            foreach (UIElement c in MainCanvas.Children)
                c.UseLayoutRounding = true;
        }

        public Dictionary<MenuButton, MenuNode<UserControl>> MenuFunctions { get; set; }
    }
}
