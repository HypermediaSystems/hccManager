using hcc;
using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccManager
{
    public partial class MainPage : TabbedPage
    {
        private iSQL SQL;
        private SqLiteCache sqLiteCache = null;
        private HttpCachedClient hc;
        public MainPage()
        {
            InitializeComponent();
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            this.SQL = Xamarin.Forms.DependencyService.Get<iSQL>();
            this.sqLiteCache = new SqLiteCache(SQL, "");
            this.hc = new HttpCachedClient(this.sqLiteCache);

            BindingContext = this.hc;

            this.CurrentPageChanged += (object sender, EventArgs e) => {
                var i = this.Children.IndexOf(this.CurrentPage);
                System.Diagnostics.Debug.WriteLine("Page No:" + i);
                if( i == 0 )
                {
                    updateDatabaseTab();
                }
            };

        }
        private void updateDatabaseTab()
        {
            BindingContext = null;
            BindingContext = this.hc;
            this.ApplyBindings();
            this.UpdateChildrenLayout();

        }
        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((SqLiteCacheItem)e.Item).url);
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((SqLiteCacheItem)e.SelectedItem).url);
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            refreshList();
        }
        private void refreshList()
        {
            listView.ItemTapped -= ListView_ItemTapped;
            listView.ItemSelected -= listView_ItemSelected;
            bindingObj bo = new bindingObj();
            listView.BindingContext = bo;
            listView.ItemsSource = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntries(tbUrl.Text);
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            listView.SelectedItem = ((IEnumerable<SqLiteCacheItem>)listView.ItemsSource).FirstOrDefault();

        }
        private void btnSelect_Clicked(object sender, EventArgs e)
        {
            detailGrid.BindingContext = (BindingContext as HMS.Net.Http.HttpCachedClient).DBEntry(((Button)sender).Text);
        }
        private void btnEntryDelete_Clicked(object sender, EventArgs e)
        {
            string url = HccTag.GetTag((Button)sender);
            (BindingContext as HMS.Net.Http.HttpCachedClient).DeleteCachedData(url);
            refreshList();

        }
        delegate Boolean bkup(string url);

        private async void btnBackup_Clicked(object sender, EventArgs e)
        {
            try
            {

                var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

                string serverUrl = tbServer.Text.Trim();

                serverUrl = hcc.HccUtil.url_join(serverUrl, "upload");

                Device.BeginInvokeOnMainThread(() =>
                {
                    lblServerStatus.Text = "Backuping to " + serverUrl + " ...";
                });

                // System.Net.Http.Headers.AuthenticationHeaderValue authenticationHeaderValue = null;
                await hcClient.BackupAsync(serverUrl);
                Device.BeginInvokeOnMainThread(() =>
                {
                    lblServerStatus.Text = "Backuping to " + serverUrl + " done.";
                });
                updateDatabaseTab();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async void btnRestore_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient.dbName + ".sqlite");

            Device.BeginInvokeOnMainThread(() =>
            {
                lblServerStatus.Text = "Restoring from " + serverUrl + " ...";
            });
            await hcClient.RestoreAsync(serverUrl);
            Device.BeginInvokeOnMainThread(() =>
            {
                lblServerStatus.Text = "Restoring from " + serverUrl + " done.";
            });

            updateDatabaseTab();

        }
        private void btnDeleteAll_Clicked(object sender, EventArgs e)
        {
            var hcc = (BindingContext as HMS.Net.Http.HttpCachedClient);
            hcc.DeleteAllCachedData();

            updateDatabaseTab();

        }
        private void tbLoop_Clicked(object sender, EventArgs e)
        {
            var hcc = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string debugUrl = "debugUrl";
            hcc.AddCachedString(debugUrl, "DebugData");


            int i1 = 0;
            int i2 = 0;
            Task.Run(async () =>
            {
                for (i1 = 0; i1 < 100; i1++)
                {
                    string url = tbUrl.Text.Trim();
                    await hcc.GetCachedString(debugUrl, (json, hi) =>
                    {
                        System.Diagnostics.Debug.WriteLine("tbLoop_Clicked1 " + i1.ToString() + "  " + i2.ToString());
                    });
                    Task.Delay(100).Wait();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString();
                    });
                }
            });
            Task.Run(async () =>
            {
                for (i2 = 0; i2 < 200; i2++)
                {
                    string url = tbUrl.Text.Trim();
                    await hcc.GetCachedString(debugUrl, (json, hi) =>
                    {
                        System.Diagnostics.Debug.WriteLine("tbLoop_Clicked2 " + i1.ToString() + "  " + i2.ToString());
                    });
                    Task.Delay(50).Wait();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString();
                    });
                }
            });
        }
        private async void btnImport_Clicked(object sender, EventArgs e)
        {
            string server = tbImportServer.Text.Trim();
            string site = tbImportSite.Text.Trim();

            if (!server.EndsWith("/"))
                server += "/";

            import_status_set("getting list from " + server + " ... ");
            // get the list
            HttpClient httpClient = new HttpClient();

            HccConfig.Rootobject hccConfig;
            using (HttpResponseMessage response = await httpClient.GetAsync(server + "config?site=" + site, HttpCompletionOption.ResponseContentRead))
            {
                string json = await response.Content.ReadAsStringAsync();

                hccConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<HccConfig.Rootobject>(json);
            }
            import_status_set("got list from " + server + " with " + hccConfig.files.Length.ToString() + " entries");

            var hcc = (BindingContext as HMS.Net.Http.HttpCachedClient);

            hcc.AddCachedMetadata("url", hccConfig.url);

            for (int i = 0; i < hccConfig.files.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.files.Length.ToString());
                using (HttpResponseMessage response = await httpClient.GetAsync(server + "entry?site=" + site + "&url=" + hccConfig.files[i].url, HttpCompletionOption.ResponseContentRead))
                {
                    string headerString = hcc.getCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        byte[] data = ((MemoryStream)strm).ToArray();
                        // we have to remove the BOM, since we want to store only text
                        int bomEnd = Bom.GetCursor(data);
                        if (bomEnd > 0)
                        {
                            Byte[] datax = new Byte[data.Length - bomEnd];
                            Array.Copy(data, bomEnd, datax, 0, data.Length - bomEnd);
                            data = datax;
                        }

                        string[] externalURLs = new string[] { "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" };
                        if (hccConfig.files[i].replace == true)
                        {
                            string html = Encoding.UTF8.GetString(data, 0, data.Length);
                            foreach (var pat in externalURLs)
                            {
                                html = html.Replace(pat, "external/" + pat.Substring(7));
                            }
                            data = Encoding.UTF8.GetBytes(html);
                        }

                        if (hcc.encryptFunction != null)
                        {
                            data = hcc.encryptFunction(hccConfig.files[i].url, data);

                            hcc.AddCachedStream(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url), data, headers: headerString, zipped: hcc.zipped, encrypted: 1);
                        }
                        else
                        {
                            hcc.AddCachedStream(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url), data, headers: headerString, zipped: 0); // hcc.zipped);
                        }
                    }
                }
            }

            import_status_set("got list of external from " + server + " with " + hccConfig.externalUrl.Length.ToString() + " entries");
            for (int i = 0; i < hccConfig.externalUrl.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.externalUrl.Length.ToString());
                using (HttpResponseMessage response = await httpClient.GetAsync(hccConfig.externalUrl[i].url, HttpCompletionOption.ResponseContentRead))
                {
                    string headerString = hcc.getCachedHeader(response.Headers);

                    Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

                    Stream strm = new MemoryStream();
                    streamToReadFrom.CopyTo(strm);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        byte[] data = ((MemoryStream)strm).ToArray();
                        // we have to remove the BOM, since we want to store only text
                        int bomEnd = Bom.GetCursor(data);
                        if (bomEnd > 0)
                        {
                            Byte[] datax = new Byte[data.Length - bomEnd];
                            Array.Copy(data, bomEnd, datax, 0, data.Length - bomEnd);
                            data = datax;
                        }
                        Byte zipped = hcc.zipped;
                        if (hccConfig.zipped != null)
                        {
                            zipped = Byte.Parse(hccConfig.zipped);
                        }
                        if (hccConfig.externalUrl[i].zipped != null)
                        {
                            zipped = Byte.Parse(hccConfig.externalUrl[i].zipped);
                        }

                        if (hcc.encryptFunction != null)
                        {
                            data = hcc.encryptFunction(hccConfig.externalUrl[i].url, data);

                            hcc.AddCachedStream(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped, encrypted: 1);
                        }
                        else
                        {
                            hcc.AddCachedStream(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped);
                        }
                    }
                }
            }
        }
        private void import_status_set(string status)
        {
            lblImportStatus.Text = status;
        }
        private void import_status_add(string status)
        {
            lblImportStatus.Text += Environment.NewLine + status;
        }

       
    }


    class bindingObj
    {
        private SqLiteCacheItem _SelectedItem;
        public SqLiteCacheItem SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
            }
        }
    }
    
    
}
