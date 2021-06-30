using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.IO;

namespace FileManager {
    /// <summary>
    /// Logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// Left Panel of Manager
        /// </summary>
        ManagerViewModel LeftManager;
        /// <summary>
        /// Right panel of manager
        /// </summary>
        ManagerViewModel RightManager;
        /// <summary>
        /// Height for panels, for resize
        /// </summary>
        private double h;
        /// <summary>
        /// Main constructor
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            LeftManager = new ManagerViewModel();
            RightManager = new ManagerViewModel(); //initialize managers
            LeftPanel.DataContext = LeftManager;
            RightPanel.DataContext = RightManager; //get data context for window Panels
            LeftDrives.SelectedIndex = 0;
            RightDrives.SelectedIndex = 0;         //select c:/
            h = this.Height - LeftScroll.Height;    //get height for panels
        }
        /// <summary>
        /// Select left drive
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Events</param>
        private void LeftDrives_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (LeftDrives.SelectedIndex > -1) LeftManager.ChangeDrive(LeftDrives.SelectedIndex);
            LeftList.Items.Refresh();  //View new information
        }
        /// <summary>
        /// Refresh drives lists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e) {
            LeftManager.RefreshDrive();
            RightManager.RefreshDrive();
            LeftDrives.Items.Refresh();
            RightDrives.Items.Refresh();
            LeftDrives.SelectedIndex=0;
            RightDrives.SelectedIndex = 0;
        }
        /// <summary>
        /// Select right drive
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Events</param>
        private void RightDrives_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (RightDrives.SelectedIndex > -1) RightManager.ChangeDrive(RightDrives.SelectedIndex);
            RightList.Items.Refresh();
        }
        /// <summary>
        /// Up to left manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLeftUp_Click(object sender, RoutedEventArgs e) {
            LeftManager.Up();
            LeftList.Items.Refresh();
        }
        /// <summary>
        /// Up to right manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRightUp_Click(object sender, RoutedEventArgs e) {
            RightManager.Up();
            RightList.Items.Refresh();
        }
        /// <summary>
        /// Refresh info for move, copy or delete files
        /// </summary>
        private void Refresh() {
            LeftManager.Refresh();
            RightManager.Refresh();
            RightList.Items.Refresh(); 
            LeftList.Items.Refresh();
        }
        /// <summary>
        /// Go down for left manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftDownClick(object sender, EventArgs e) { //if selected record is file, or not selected, go out
            if (LeftList.SelectedIndex == -1 || LeftManager.Dir[LeftList.SelectedIndex].Type == "File: " || LeftList.SelectedItems.Count!=1) return;
            try {
                LeftManager.Refresh(LeftList.SelectedIndex); //go to selected directory
            }
            catch (Exception ex) { 
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ButtonLeftUp_Click(sender, new RoutedEventArgs()); //if can`t go down, back to current directory
            }
            LeftList.Items.Refresh();  //refresh new info
        }
        /// <summary>
        /// Go down for right manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightDownClick(object sender, EventArgs e) {//if selected record is file, or not selected, go out
            if (RightList.SelectedIndex == -1 || RightManager.Dir[RightList.SelectedIndex].Type == "File: " || RightList.SelectedItems.Count != 1) return;
            try {
                RightManager.Refresh(RightList.SelectedIndex);//go to selected directory
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ButtonRightUp_Click(sender, new RoutedEventArgs());//if can`t go down, back to current directory
            }
            RightList.Items.Refresh();//refresh new info
        }
        /// <summary>
        /// Move Files and directories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveButton_Click(object sender, RoutedEventArgs e) {
            foreach (var d in LeftList.SelectedItems) {
                if (LeftManager.Dir[LeftList.Items.IndexOf(d)].Type == "Folder: ") { //for directories get processing
                    try {
                        Directory.CreateDirectory(System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name));
                    } //copy structure of all dorectoris 
                    catch (Exception) {
                        MessageBox.Show("Cannot create Folder in " + RightManager.Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue; //for error go to next record
                    }
                    Thread proc = new Thread(ProcessingFiles);
                    object[] move = { (int)0, System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name), 
                                        System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name) , true};
                    proc.Start(move);  //new proc
                } 
                else{
                    try { //for files just move
                        File.Move(System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name),
                            System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name));
                    }
                    catch (Exception) { //can`t move? go to next file
                        MessageBox.Show("Cannot move File " + LeftManager.Dir[LeftList.Items.IndexOf(d)].Name
                            + " to " + RightManager.Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// Copy fules and directories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            foreach (var d in LeftList.SelectedItems) {
                if (LeftManager.Dir[LeftList.Items.IndexOf(d)].Type == "Folder: ") {//for directories get processing
                    try {
                        Directory.CreateDirectory(System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name));
                    }//copy structure of all dorectoris 
                    catch (Exception) {
                        MessageBox.Show("Cannot create Folder in " + RightManager.Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;//for error go to next record
                    }
                    Thread proc = new Thread(ProcessingFiles);
                    object[] move = { (int)1, System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name), 
                                        System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name) , false};
                    proc.Start(move);
                }
                else {//for files just move
                    try {
                        File.Copy(System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name),
                            System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name));
                    }
                    catch (Exception) {//can`t move? go to next file
                        MessageBox.Show("Cannot move File " + LeftManager.Dir[LeftList.Items.IndexOf(d)].Name
                            + " to " + RightManager.Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// Delete files and directories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            foreach (var d in LeftList.SelectedItems) {
                if (LeftManager.Dir[LeftList.Items.IndexOf(d)].Type == "Folder: ") {
                    Thread proc = new Thread(ProcessingFiles); //new processing
                    object[] move = { (int)2, System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name), 
                                        System.IO.Path.Combine(RightManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name) , false};
                    proc.Start(move);
                }
                else {
                    try {
                        File.Delete(System.IO.Path.Combine(LeftManager.Path, LeftManager.Dir[LeftList.Items.IndexOf(d)].Name));
                    }
                    catch (Exception) { //if can`t delete, go to next file
                        MessageBox.Show("Cannot Delete File " + LeftManager.Dir[LeftList.Items.IndexOf(d)].Name
                            + " to " + RightManager.Path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// processing for files and folders
        /// </summary>
        /// <param name="Params">Params for processing:
        /// 0: Operation. 0 - moving files, 1 - copy files, 2 - delete files
        /// 1: Sourse path
        /// 2: Destination path(can be null for deleting)
        /// 3: boolean - delete Directory after processing?</param>
        private void ProcessingFiles(object Params) {
            object[] Par = (object[])Params; //get array
            int TypeProcessing = (int)Par[0];
            string Sourse = (string)Par[1];
            string Destination = (string)Par[2];
            bool m = (bool)Par[3];                  //parse array
            switch ((int)TypeProcessing) {          //let`s go
                case 0: { //Move
                    foreach (var d in Directory.GetDirectories(Sourse)) {
                        try { //for all directory making all structure
                            Directory.CreateDirectory(System.IO.Path.Combine(Destination, Path.GetFileName(d)));
                        }
                        catch (Exception) { //can`t create? next directory
                            MessageBox.Show("Cannot create Folder in "+Destination, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            m = false; //can`t make, dont remove
                            continue;
                        }
                        object[] move = { TypeProcessing, System.IO.Path.Combine(Sourse, d), System.IO.Path.Combine(Destination, Path.GetFileName(d)), false };
                        ProcessingFiles(move); //recursive process
                    }
                    foreach (var f in Directory.GetFiles(Sourse)) {
                        try { //for all files - just move
                            File.Move(System.IO.Path.Combine(Sourse, Path.GetFileName(f)), System.IO.Path.Combine(Destination, Path.GetFileName(f)));
                        }
                        catch (Exception) { //can`t move? next file
                            MessageBox.Show("Cannot move File " + f + " to " + Destination, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            m = false; //can`t move, don`t remove
                            continue;
                        }
                    }
                    if (m == true) Directory.Delete(Sourse,true); //if this root sourse, delete Directory
                    break;
                }
                case 1: { //Copy
                    foreach (var d in Directory.GetDirectories(Sourse)) {
                        try {//for all directory making all structure
                            Directory.CreateDirectory(System.IO.Path.Combine(Destination, Path.GetFileName(d)));
                        }
                        catch (Exception) {
                            MessageBox.Show("Cannot create Folder in " + Destination, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;//can`t create? next directory
                        }
                        object[] move = { TypeProcessing, System.IO.Path.Combine(Sourse, d), System.IO.Path.Combine(Destination, Path.GetFileName(d)), false };
                        ProcessingFiles(move);//recursive process
                    }
                    foreach (var f in Directory.GetFiles(Sourse)) {
                        try {//for all files - just copy
                            File.Copy(System.IO.Path.Combine(Sourse, Path.GetFileName(f)), System.IO.Path.Combine(Destination, Path.GetFileName(f)));
                        }
                        catch (Exception) {//can`t copy? next file
                            MessageBox.Show("Cannot move File " + f + " to " + Destination, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                    }
                    break;
                }
                case 2: { //Delete
                    foreach (var d in Directory.GetDirectories(Sourse)) {
                        object[] move = { TypeProcessing, System.IO.Path.Combine(Sourse, d), System.IO.Path.Combine(Destination, Path.GetFileName(d)), false };
                        ProcessingFiles(move); //for all directory go to all structure
                    }
                    foreach (var f in Directory.GetFiles(Sourse)) {
                        try {
                            File.Delete(System.IO.Path.Combine(Sourse, Path.GetFileName(f)));
                        } //just delete file
                        catch (Exception) {
                            MessageBox.Show("Cannot delete File " + f, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue; //can`t delete? next!
                        }
                    }
                    try {
                        Directory.Delete(Sourse); //for end delete sourse directory
                    }
                    catch (Exception) {
                        MessageBox.Show("Cannot delete Folder " + Sourse, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                }
            }
            this.Dispatcher.BeginInvoke((ThreadStart)delegate(){
                Refresh(); //show info about new records
            });
        }
        /// <summary>
        /// Resize windows - resize panels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            LeftScroll.Height = this.Height - h;
            RightScroll.Height = this.Height - h;
        }

    }
}
