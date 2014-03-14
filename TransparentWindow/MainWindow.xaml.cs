﻿using DragDropLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using DataObject = System.Windows.DataObject;

namespace DesignHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image|*.jpg;*.png|All Files|*.*";
            if (open.ShowDialog()==true)
            {
                OpenImage(open.FileName);
            }
        }

        private void OpenImage(string file)
        {
            string ext = System.IO.Path.GetExtension(file).ToLower();
            if (ext != ".bmp" && ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".tmp")
            {
                MessageBox.Show("文件扩展名不是常见图片类型，我懒得打开。");
                return;
            }
            try
            {
                BitmapImage myBitmapImage = new BitmapImage(new Uri(file));
                OpenImage(myBitmapImage);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // Create source
        }

        private void OpenImage(BitmapSource myBitmapImage)
        {
            this.image.Source = myBitmapImage;
            this.image.Stretch = Stretch.UniformToFill;

            this.Width = myBitmapImage.PixelWidth;
            this.Height = myBitmapImage.PixelHeight + this.toolbar.ActualHeight;
        }
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

#region Move Window
		        bool moving = false;
        double x1, y1;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.moving = true;
                Point pos = e.GetPosition(this);
                this.x1 = pos.X;
                this.y1 = pos.Y;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                this.moving = false;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.moving)
            {
                Point pos = e.GetPosition(this);
                this.Left += pos.X - this.x1;
                this.Top += pos.Y - this.y1;

            }
        }

	#endregion    

        private void StackPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double o = this.image.Opacity;
            if (e.Delta > 0)
            {
                o += .05;
            }
            if (e.Delta < 0)
            {
                o -= .05;
            }
            if (o > 1) o = 1;
            if (o < .05) o = .1;
            this.image.Opacity = o;
        }

        double movement = 10;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    this.Left -= movement;
                    break;
                case Key.Right:
                    this.Left += movement;
                    break;
                case Key.Up:
                    this.Top -= movement;
                    break;
                case Key.Down:
                    this.Top += movement;
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    movement = 1;
                    break;
                default:
                    break;
            }
            Debug.WriteLine(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case  Key.LeftCtrl:
                case Key.RightCtrl:
                    movement = 10;
                    break;
            }
        }
        protected override void OnDragEnter(DragEventArgs e)
        {
            Win32Point wp;
            e.Effects = DragDropEffects.Copy;
            Point p = e.GetPosition(this);
            wp.x = (int)p.X;
            wp.y = (int)p.Y;
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragEnter(wndHelper.Handle, (ComIDataObject)e.Data, ref wp, (int)e.Effects);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragLeave();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            Win32Point wp;
            e.Effects = DragDropEffects.Copy;
            Point p = e.GetPosition(this);
            wp.x = (int)p.X;
            wp.y = (int)p.Y;
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragOver(ref wp, (int)e.Effects);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            Win32Point wp;
            e.Effects = DragDropEffects.Copy;
            Point p = e.GetPosition(this);
            wp.x = (int)p.X;
            wp.y = (int)p.Y;
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.Drop((ComIDataObject)e.Data, ref wp, (int)e.Effects);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files  = e.Data.GetData(DataFormats.FileDrop) as string[];
                this.OpenImage(files[0]);
            }

            System.Windows.IDataObject data = e.Data;
            string[] formats = data.GetFormats();
            if (formats.Contains("text/html"))
            {
                var obj = data.GetData("text/html");
                string html = string.Empty;
                if (obj is string)
                {
                    html = (string)obj;
                }
                else if (obj is MemoryStream)
                {
                    MemoryStream ms = (MemoryStream)obj;
                    byte[] buffer = new byte[ms.Length];
                    ms.Read(buffer, 0, (int)ms.Length);
                    if (buffer[1] == (byte)0)  // Detecting unicode
                    {
                        html = System.Text.Encoding.Unicode.GetString(buffer);
                    }
                    else
                    {
                        html = System.Text.Encoding.ASCII.GetString(buffer);
                    }
                }
                // Using a regex to parse HTML, but JUST FOR THIS EXAMPLE :-)
                var match = new Regex(@"<img[^>]+src=""([^""]*)""").Match(html);
                if (match.Success)
                {
                    Uri uri = new Uri(match.Groups[1].Value);
                    SetImageFromUri(uri);
                }
            }
        }
        private void SetImageFromUri(Uri uri)
        {
            string fileName = System.IO.Path.GetTempFileName();
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += delegate
                {
                    this.OpenImage(fileName);
                };
                webClient.DownloadFileAsync(uri, fileName);
            }
            
        }
        private void CommandBinding_PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //MessageBox.Show("Clipboard operation occured!");
            if (Clipboard.ContainsImage())
            {
                this.OpenImage(Clipboard.GetImage());
            }
        }
    
    }
}