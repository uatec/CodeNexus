using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using MahApps.Metro.Controls;
using System.Threading.Tasks;
using System.Threading;

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
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public string FileMask
        {
            get { return _fileMask; }
            set {
                if (String.IsNullOrEmpty(value))
                {
                    _fileMask = "*";
                }
                else
                {
                    _fileMask = value;
                }
                UpdateResults();
            }
        }

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
                    UpdateResults();
                }
            }
        }

        private void UpdateResults()
        {
            // cancel old thread, we are in charge now
            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();

            Task<IEnumerable<string>>.Factory.StartNew(() => _search(_tokenSource.Token, m_sValue)).ContinueWith
                        ((t) =>
                        {
                            _results = new ObservableCollection<string>(t.Result.OrderBy(s => s));

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


        private IEnumerable<T> SafeEnumerate<T>(IEnumerable<T> enumerable)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            bool endOfList = false;
            while (!endOfList)
            {
                try
                {
                    endOfList = !enumerator.MoveNext();
                }
                catch (Exception)
                {
                    // shut up, let's skip this one
                    continue;
                }
                yield return enumerator.Current;
            }
        }


        private IEnumerable<string> _search(CancellationToken cancellationToken, string searchTerm)
        {
            Thread thread = Thread.CurrentThread;
            cancellationToken.Register(thread.Abort);
            DirectoryInfo di = new DirectoryInfo("C:\\");

            foreach (FileInfo fileInfo in SafeEnumerate(di.EnumerateFiles("*.cs", SearchOption.AllDirectories)))
            {
                try
                {

                    using (FileStream fileStream = fileInfo.OpenRead())
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                string line = streamReader.ReadLine().ToLower();

                                if (line.Contains(searchTerm))
                                {
                                    yield return fileInfo.FullName;

                                }
                                MatchCollection matches = wordFinder.Matches(line);

                                foreach (Match match in matches)
                                {
                                    _index.Add(fileInfo.FullName, match.Value);
                                }
                            }
                        }
                    }
                }
                catch (AggregateException aggregateException)
                {
                    foreach (Exception ex in aggregateException.InnerExceptions)
                    {
                        Console.WriteLine("{0} ! {1}", fileInfo.FullName, ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} ! {1}", fileInfo.FullName, ex.Message);
                }
                _filesIndexed++;
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
        private string _fileMask;
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
