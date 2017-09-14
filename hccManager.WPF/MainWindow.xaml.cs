using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace hccManager.WPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly private ISql SQL;
        readonly private SqLiteCache sqLiteCache;
        private HttpCachedClient hcClient;

        public MainWindow()
        {
            InitializeComponent();
            this.SQL = new HMS.Net.Http.NET.SQLImplementation.SqlNET();
            this.sqLiteCache = new SqLiteCache(SQL, "");
            this.sqLiteCache.CreateAsync().ContinueWith(t =>
            {
                this.hcClient = new HttpCachedClient(this.sqLiteCache);
                Application.Current.Dispatcher.BeginInvoke(
DispatcherPriority.Background,
new Action(() => {
    DataContext = this.hcClient;
}));
            });
        }

        private async void btnBackup_ClickedAsync(object sender, EventArgs e)
        {
            Boolean ret = false;
            string errMsg = "";
            var hcClientLocale = (DataContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "upload");

            server_status_set("Backuping to " + serverUrl + " ...");
            try
            {
                ret = await hcClientLocale.BackupAsync(serverUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // ToDo log this error
                ret = false;
                errMsg = ex.ToString();
            }
            if (!ret)
            {
                server_status_set("Backuping to " + serverUrl + " ended with error. " + errMsg);
            }
            else
            {
                server_status_set("Backuping to " + serverUrl + " done.");
            }
            updateDatabaseTab();
        }

        private async void btnRestore_ClickedAsync(object sender, EventArgs e)
        {
            Boolean ret = false;
            string errMsg = "";

            var hcClientLocal = (DataContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient._dbName + ".sqlite");

            server_status_set("Restoring from " + serverUrl + " ...");

            try
            {
                ret = await hcClientLocal.RestoreAsync(serverUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // ToDo log this error
                ret = false;
                errMsg = ex.ToString();
            }

            if (!ret)
            {
                server_status_set("Restoring from " + serverUrl + " ended with error. " + errMsg);
            }
            else
            {
                server_status_set("Restoring from " + serverUrl + " done.");
            }

            updateDatabaseTab();
        }

        private async void btnDeleteAll_Clicked(object sender, EventArgs e)
        {
            var hcClientLocale = (DataContext as HMS.Net.Http.HttpCachedClient);
            await hcClientLocale.DeleteAllCachedDataAsync().ConfigureAwait(false);

            updateDatabaseTab();
        }
        private async void btnReset_Clicked(object sender, EventArgs e)
        {
            var hcClientLocale = (DataContext as HMS.Net.Http.HttpCachedClient);
            await hcClientLocale.ResetAsync();
            updateDatabaseTab();
        }

        private async void btnLoop_Clicked(object sender, EventArgs e)
        {
            var hcClientLocale = (DataContext as HMS.Net.Http.HttpCachedClient);

            const string debugUrl = "debugUrl";
            await hcClientLocale.AddCachedStringAsync(debugUrl, "DebugData").ConfigureAwait(false);

            int i1 = 0;
            int i2 = 0;
            Task.Run(async () =>
            {
                for (i1 = 0; i1 < 100; i1++)
                {
                    HccResponse hccResponse = await hcClientLocale.GetCachedStringAsync(debugUrl).ConfigureAwait(false);
                    // , (json, hi) => System.Diagnostics.Debug.WriteLine("tbLoop_Clicked1 " + i1.ToString() + "  " + i2.ToString())).ConfigureAwait(false);
                    Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Background,
                          new Action(() =>
                          {
                              btnLoop.Content = "Loop " + i1.ToString() + "  " + i2.ToString();
                          }));
                    Task.Delay(100).Wait();
                }
            });

            Task.Run(async () =>
            {
                for (i2 = 0; i2 < 200; i2++)
                {
                    HccResponse hccResponse = await hcClientLocale.GetCachedStringAsync(debugUrl).ConfigureAwait(false);
                    // , (json, hi) => System.Diagnostics.Debug.WriteLine("tbLoop_Clicked2 " + i1.ToString() + "  " + i2.ToString())).ConfigureAwait(false);
                    Task.Delay(50).Wait();
                    Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Background,
                          new Action(() =>
                          {
                              btnLoop.Content = "Loop " + i1.ToString() + "  " + i2.ToString();
                          }));
                }
            });
            updateDatabaseTab();
        }
        private void updateDatabaseTab()
        {
            Application.Current.Dispatcher.BeginInvoke(
                          DispatcherPriority.Background,
                          new Action(() =>
                          {
                              DataContext = null;
                              DataContext = this.hcClient;
                              // this.ApplyBindings();
                              // this.UpdateChildrenLayout();
                          }));


        }

        private void server_status_set(string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(
      DispatcherPriority.Background,
      new Action(() => {
          lblServerStatus.Content = msg;
      }));
        }
        private void server_status_add(string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(
      DispatcherPriority.Background,
      new Action(() => {
          lblServerStatus.Content += msg;
      }));
        }
    }
}
