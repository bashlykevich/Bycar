﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;


namespace splashDemo
{
    /// <summary>
    /// Interaction logic for splash.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        Thread loadingThread;
        Storyboard Showboard;
        Storyboard Hideboard;
        private delegate void ShowDelegate(string txt);
        private delegate void HideDelegate();
        ShowDelegate showDelegate;
        HideDelegate hideDelegate;

        public SplashWindow()
        {            
            InitializeComponent();
            showDelegate = new ShowDelegate(this.showText);
            hideDelegate = new HideDelegate(this.hideText);
            Showboard = this.Resources["showStoryBoard"] as Storyboard;
            Hideboard = this.Resources["HideStoryBoard"] as Storyboard;         
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingThread = new Thread(load);
            loadingThread.IsBackground = true;
            loadingThread.Start();
        }
        private void load()
        {
            while (true)
            {
                Thread.Sleep(2000);
                this.Dispatcher.Invoke(showDelegate, "Идёт загрузка данных...");
                Thread.Sleep(2000);
                this.Dispatcher.Invoke(hideDelegate);

                Thread.Sleep(2000);
                this.Dispatcher.Invoke(showDelegate, "Пожалуйста, подождите...");
                Thread.Sleep(2000);
                //load data
                this.Dispatcher.Invoke(hideDelegate);
            }
        }       
        private void showText(string txt)
        {
            txtLoading.Text = txt;
            BeginStoryboard(Showboard);
        }
        private void hideText()
        {
            BeginStoryboard(Hideboard);
        }
    }
}
