/*
 * Copyright (c) 2012-2014 Microsoft Mobile.
 * */

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SurveillanceCamera.Model
{
    // How to: Display Data in a ListBox
    // http://msdn.microsoft.com/en-us/library/cc265158%28v=VS.95%29.aspx

    // Creating Data Classes
    // http://msdn.microsoft.com/en-us/library/gg680258%28v=pandp.11%29.aspx

    // Model of one item
    [DataContract]
    public class LogModel : INotifyPropertyChanged
    {
        private string _time;

        public LogModel()
        {
        }

        public LogModel(string time)
        {
            Time = time;
        }
       
        [DataMember]
        public string Time
        {
            get { return _time; }
            set
            {
                if (_time == value) return;
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    // List of model item
    public class Logs : ObservableCollection<LogModel>
    {
        public Logs()
        {
            RestoreModelFromFile();
        }

        public void AddAlarmToLog()
        {
            // Current time
            string time = GetNow();
            
            // Create model item
            LogModel item = new LogModel(time);
            
            // Store item to model
            Insert(0, item);

            // Append item to isolated storage
            StoreWholeModelToFile();
        }

        public string GetNow()
        {
            return String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }

        public void ClearLogs()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(@"Logs\alarmhistory.log"))
                    store.DeleteFile(@"Logs\alarmhistory.log");

            }
            this.ClearItems();
        }

        private void AppendModelToFile(LogModel modelItem)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists("Logs"))
                    store.CreateDirectory("Logs");

                using (IsolatedStorageFileStream isoStream = store.OpenFile(@"Logs\alarmhistory.log",
                    System.IO.FileMode.Append))
                {
                    using (var isoFileWriter = new System.IO.StreamWriter(isoStream))
                    {
                        isoFileWriter.WriteLine(modelItem.Time);
                        //System.Diagnostics.Debug.WriteLine("store: " + modelItem.Time);
                    }
                }
            }
        }

        private void StoreWholeModelToFile()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists("Logs"))
                    store.CreateDirectory("Logs");

                if (store.FileExists(@"Logs\alarmhistory.log"))
                    store.DeleteFile(@"Logs\alarmhistory.log");

                using (IsolatedStorageFileStream isoStream = store.OpenFile(@"Logs\alarmhistory.log",
                    System.IO.FileMode.OpenOrCreate))
                {
                    using (var isoFileWriter = new System.IO.StreamWriter(isoStream))
                    {
                        foreach (LogModel item in this.Items) 
                        {
                            isoFileWriter.WriteLine(item.Time);
                            //System.Diagnostics.Debug.WriteLine("store: " + item.Time);
                        }
                    }                    
                }
            }
        }

        public void RestoreModelFromFile()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(@"Logs\alarmhistory.log"))
                {
                    using (IsolatedStorageFileStream isoStream = store.OpenFile(@"Logs\alarmhistory.log",
                        System.IO.FileMode.OpenOrCreate))
                    {
                        using (var isoFileReader = new System.IO.StreamReader(isoStream))
                        {
                            string line = isoFileReader.ReadLine();
                            while (line != null)
                            {
                                Add(new LogModel(line));
                                //System.Diagnostics.Debug.WriteLine("restore: " + line);
                                line = isoFileReader.ReadLine();
                            }

                        }
                    }
                }
            }
        }


    }



}
