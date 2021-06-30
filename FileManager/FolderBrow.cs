using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System;

namespace FileManager {
    /// <summary>
    /// Type of Record: Directory or File
    /// </summary>
    public enum TypeOfRec { Dir, File }
    /// <summary>
    /// Class for records of Filemanager
    /// </summary>
    public class ManagerViewModel : INotifyPropertyChanged {
        private List<DriveInfo> listDrive;
        /// <summary>
        /// List for all drives computer
        /// </summary>
        public List<DriveInfo> ListDrive { 
            get { return listDrive; }
            set { listDrive = value; NotifyPropertyChanged("ListDrive"); } 
        }
        private DirectoryInfo info;
        /// <summary>
        /// Info about current directory
        /// </summary>
        public DirectoryInfo Info { 
            get { return info; }
            set { info = value; NotifyPropertyChanged("Info"); } 
        }
        private List<FileDirInfo> dir;
        /// <summary>
        /// List with records directory and files
        /// </summary>
        public List<FileDirInfo> Dir { 
            get { return dir; }
            set { dir = value; NotifyPropertyChanged("Dir"); }
        }
        private string path;
        /// <summary>
        /// Current path
        /// </summary>
        public string Path {
            get { return path; }
            set { path = value; NotifyPropertyChanged("Path"); }
        }
        /// <summary>
        /// Main Contstructor
        /// </summary>
        public ManagerViewModel(){
            this.ListDrive = new List<DriveInfo>();
            this.ListDrive.AddRange(DriveInfo.GetDrives());
            this.Info = new DirectoryInfo(ListDrive.First().RootDirectory.ToString());
            this.Dir = new List<FileDirInfo>();
            ChangeDrive(0);
        }
        /// <summary>
        /// View information in current directory or about selected directory from list
        /// </summary>
        /// <param name="index">-1 for update information about current directory, 
        /// 0..n - index of directory in list records</param>
        public void Refresh(int index = -1) {
            try {
                if (this.Path == null || this.Path == "") return;
                if (index != -1) { //if we go to directory down
                    this.Path = System.IO.Path.Combine(this.path, dir[index].Name);
                    this.Info = new DirectoryInfo(this.Path);
                }
                this.Dir.Clear();
                foreach (DirectoryInfo d in Info.GetDirectories()) this.Dir.Add(new FileDirInfo(d.Name, TypeOfRec.Dir)); //get all directories
                foreach (FileInfo f in Info.GetFiles()) this.Dir.Add(new FileDirInfo(f.Name, TypeOfRec.File)); //get all files
            }
            catch (Exception) {
                throw; 
            }
        }
        /// <summary>
        /// Go to the upper directory
        /// </summary>
        public void Up() {
            if (info.Root.ToString() == info.FullName) return; //if path is root, then we can`t go up
                Info = Info.Parent;     //get parent directory
                Path = Info.FullName;
                Refresh();              //refresh info
        }
        /// <summary>
        /// Changing drive
        /// </summary>
        /// <param name="drive">Index of drive in drive list</param>
        public void ChangeDrive(int drive) {
            this.Info = new DirectoryInfo(ListDrive[drive].RootDirectory.ToString()); //get new root, new path
            this.Path = this.ListDrive[drive].RootDirectory.ToString();
            this.Dir.Clear();
            foreach (var d in Info.GetDirectories()) Dir.Add(new FileDirInfo(d.Name, TypeOfRec.Dir)); //get all directories
            foreach (var d in Info.GetFiles()) Dir.Add(new FileDirInfo(d.Name, TypeOfRec.File));      //get all files
            
        }
        /// <summary>
        /// Refresh list of drives
        /// </summary>
        public void RefreshDrive() {
            this.ListDrive.Clear();
            this.ListDrive.AddRange(DriveInfo.GetDrives());
        }
        /// <summary>
        /// New event of property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop = "") {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    /// <summary>
    /// Information about record in directory
    /// </summary>
    public class FileDirInfo : INotifyPropertyChanged{
        private string name;
        private string type;
        /// <summary>
        /// Name of directory of file
        /// </summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        /// <summary>
        /// Type if record
        /// </summary>
        public string Type { get { return type; } set { type = value; OnPropertyChanged("Type"); } }
        /// <summary>
        /// Main Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="t">Type of record</param>
        public FileDirInfo(string name, TypeOfRec t) {
            this.Name = name;
            if (t == TypeOfRec.Dir) this.Type = "Folder: "; else this.Type = "File: ";
        }
        /// <summary>
        /// New event of property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "") {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class Progress : INotifyPropertyChanged {
        private int count;
        public int Count { get { return count; } set { count = value; OnPropertyChanged("Name"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "") {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
