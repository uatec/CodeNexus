using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeNexus;
using CodeRaven.Data.Redis;
using System.Diagnostics;
using MahApps.Metro.Controls;

namespace CodeNexus.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public static string TextEditor = @"C:\Program Files\Sublime Text 2\sublime_text.exe";

        public static RoutedUICommand openFile = new RoutedUICommand("openFile", "openFile", typeof(MainWindow),
                                                                     new InputGestureCollection{
                                                                         new MouseGesture(MouseAction.LeftDoubleClick)});

        public MainWindow()
        {
            InitializeComponent();
            this.MyDotNetProperty = String.Empty;
            TopLevelContainer.DataContext = this;

            //lstResults.CommandBindings.Add(new CommandBinding(openFile, Executed));
            //lstResults.InputBindings.Add(new InputBinding(openFile, new MouseGesture(MouseAction.LeftDoubleClick)));
        }

        private void listBoxItemDoubleClick(object sender, MouseButtonEventArgs args)
        {
            Process.Start(TextEditor, String.Format("\"{0}\"", (sender as ListBoxItem).Content.ToString()));

        }

        private string m_sValue;

        public string MyDotNetProperty
        {
            get { return m_sValue; }
            set
            {
                m_sValue = value;
                if (String.IsNullOrEmpty(m_sValue))
                {
                    _results = new ObservableCollection<string>();
                }
                else
                {
                    IIndex index = new RedisIndex("localhost", 6379, 0);
                    index.GetAsync(m_sValue).ContinueWith(t =>
                                                              {

                                                                  _results =
                                                                      new ObservableCollection<string>(
                                                                          t.Result.OrderBy(s => s));

                                                                  if (null != this.PropertyChanged)
                                                                  {
                                                                      PropertyChanged(this,
                                                                                      new PropertyChangedEventArgs(
                                                                                          "MyDotNetProperty"));
                                                                      PropertyChanged(this,
                                                                                      new PropertyChangedEventArgs(
                                                                                          "Results"));
                                                                  }


                                                              });
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        //public MainWindow()
        //{
        //    InitializeComponent();
        //    this.DataContext = this;

        //}

        private ObservableCollection<string> _results;
        public ObservableCollection<string> Results { get { return _results; } }



        //private string _searchTerm = String.Empty;
        //public string SearchTerm
        //{
        //    get { return _searchTerm; }
        //    set
        //    {
        //        _searchTerm = value;
        //        Task.Factory.StartNew(() =>
        //        {
        //            try
        //            {
        //                IIndex index = new RedisIndex("localhost", 6739, 0);
        //                if (null != this.PropertyChanged)
        //                {
        //                    PropertyChanged(this, new PropertyChangedEventArgs("MyDotNetProperty"));
        //                    PropertyChanged(this, new PropertyChangedEventArgs("Results"));
        //                }
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        });
        //    }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
    }
}
