using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using GeoNorgeAPI;
using www.opengis.net;

namespace Sample
{
    public partial class MainWindow : Window
    {
        private readonly GeoNorge _geonorgeApi;

        public MainWindow()
        {
            InitializeComponent();
            MetadataViewModel model = Resources["MetadataModel"] as MetadataViewModel;
            if (model != null)
                model.Metadata = new ObservableCollection<Metadata>(new List<Metadata>());

            _geonorgeApi = new GeoNorge();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Searching...";

            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.DoWork += WorkerOnDoWork;
            worker.RunWorkerAsync(txtSearch.Text);
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            SearchResultsType results = _geonorgeApi.Search((string)args.Argument);
            args.Result = results;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            MetadataViewModel model = Resources["MetadataModel"] as MetadataViewModel;
            if (model != null)
            {
                SearchResultsType results = (SearchResultsType) args.Result;
                
                model.Metadata.Clear();

                if (results.Items != null)
                {
                    foreach (var item in results.Items)
                    {
                        RecordType record = (RecordType) item;

                        string title = null;
                        string uri = null;
                        string uuid = null;
                        string creator = null;

                        for (int i = 0; i < record.ItemsElementName.Length; i++)
                        {
                            var name = record.ItemsElementName[i];
                            var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                            if (name == ItemsChoiceType24.title)
                                title = value;
                            else if (name == ItemsChoiceType24.URI && value != null && value.StartsWith("http"))
                                uri = value;
                            else if (name == ItemsChoiceType24.identifier)
                                uuid = value;
                            else if (name == ItemsChoiceType24.creator)
                                creator = value;
                        }
                        model.Metadata.Add(new Metadata { Title = title, Uri = uri, Uuid = uuid, Creator = creator});
                    }
                    lblStatus.Content = "Showing " + results.numberOfRecordsReturned + " of total " + results.numberOfRecordsMatched + " matches.";
                }
                else
                {
                    lblStatus.Content = "No match!";
                }
                
            }
        }
    }

    public class MetadataViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Metadata> _metadata;

        public ObservableCollection<Metadata> Metadata
        {
            get { return _metadata; }
            set
            {
                _metadata = value;
                SendPropertyChanged("Metadata");
            }
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        private void SendPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class Metadata
    {
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Uri { get; set; }
        public string Uuid { get; set; }
    }

}
