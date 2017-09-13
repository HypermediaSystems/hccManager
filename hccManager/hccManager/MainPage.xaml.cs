using hcc;
using HccConfig;
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
        readonly private ISql SQL;
        readonly private SqLiteCache sqLiteCache;
        private HttpCachedClient hcClient;
        public MainPage(ISql SQL)
        {
            InitializeComponent();
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            this.SQL = SQL;
            this.sqLiteCache = new SqLiteCache(SQL, "");
            this.sqLiteCache.CreateAsync().ContinueWith(t =>
            {
                this.hcClient = new HttpCachedClient(this.sqLiteCache);
                Device.BeginInvokeOnMainThread(() =>
                    BindingContext = this.hcClient
                );
            });

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
            Device.BeginInvokeOnMainThread(() =>
            {
                BindingContext = null;
                BindingContext = this.hcClient;
                this.ApplyBindings();
                this.UpdateChildrenLayout();
            });
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            detailGrid.BindingContext = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((SqLiteCacheItem)e.Item).url);
        }

        private async void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            detailGrid.BindingContext = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((SqLiteCacheItem)e.SelectedItem).url);
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            refreshList();
        }

        private void refreshList()
        {
            listView.ItemTapped -= ListView_ItemTapped;
            listView.ItemSelected -= listView_ItemSelected;
            BindingObj bo = new BindingObj();
            listView.BindingContext = bo;

            IEnumerable<SqLiteCacheItem> list = null;
            Task.Run(async () =>
            {
                list = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntriesAsync(tbUrl.Text);
            }).Wait();
            listView.ItemsSource = list;


            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            listView.SelectedItem = ((IEnumerable<SqLiteCacheItem>)listView.ItemsSource).FirstOrDefault();
        }

        private async void btnSelect_Clicked(object sender, EventArgs e)
        {
            detailGrid.BindingContext = await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DBEntryAsync(((Button)sender).Text);
        }

        private async void btnEntryDelete_Clicked(object sender, EventArgs e)
        {
            string url = HccTag.GetTag((Button)sender);
            await (BindingContext as HMS.Net.Http.HttpCachedClient)?.DeleteCachedDataAsync(url);
            refreshList();
        }

        private async void btnBackup_ClickedAsync(object sender, EventArgs e)
        {
            Boolean ret = false;
            string errMsg = "";
            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);

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
            if ( !ret )
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

            var hcClientLocal = (BindingContext as HMS.Net.Http.HttpCachedClient);

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

            if ( !ret )
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
            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);
            await hcClientLocale.DeleteAllCachedDataAsync().ConfigureAwait(false);

            updateDatabaseTab();
        }

      
        private async void btnImport_ClickedAsync(object sender, EventArgs e)
        {
            string server = tbImportServer.Text.Trim();
            string site = tbImportSite.Text.Trim();

            if (!server.EndsWith("/",StringComparison.CurrentCulture))
                server += "/";

            import_status_set("getting list from " + server + " ... ");
            // get the list
            HttpClient httpClient = new HttpClient();

            HccConfig.Rootobject hccConfig;
            try
            {
                string configUrl = server + "config?site=" + site;
                if( !cbNodeJS.IsToggled  )
                {
                    configUrl = server + ".hccConfig.json";
                }
                using (HttpResponseMessage response = await httpClient.GetAsync(configUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    hccConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<HccConfig.Rootobject>(json);
                }
            }
            catch (Exception ex)
            {
                // ToDo log this error
                import_status_set("ERROR:site " + ex);
                return;
            }

            if ( !string.IsNullOrEmpty(hccConfig.fileList) )
            {
                try
                {
                    string configUrl = server + "config?site=" + site + "&url=" + hccConfig.fileList;
                    if ( !cbNodeJS.IsToggled )
                    {
                        configUrl = server + hccConfig.fileList;
                    }
                    using (HttpResponseMessage response = await httpClient.GetAsync(configUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                    {
                        string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var files = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(json);
                        var fl = hccConfig.files.ToList();
                        foreach(var f in files)
                        {
                            File file = new File();
                            file.replace = true;
                            file.url = f;
                            file.zipped = "1";
                            fl.Add(file);
                        }

                        hccConfig.files = fl.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    // ToDo log this error
                    import_status_set("ERROR:site " + ex);
                    return;
                }
            }

            import_status_set("got list from " + server + " with " + hccConfig.files.Length.ToString() + " entries");

            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);

            await hcClientLocale.AddCachedMetadataAsync("hcc.url", hccConfig.url).ConfigureAwait(false);
            string hccDefaultHTML = "index.html";

            for (int i = 0; i < hccConfig.files.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.files.Length.ToString());

                if( hccConfig.files[i].defaulthtml )
                {
                    hccDefaultHTML = hccConfig.files[i].url;
                }

                try
                {
                    string entryUrl = server + "entry?site=" + site + "&url=" + hccConfig.files[i].url;
                    if ( !cbNodeJS.IsToggled)
                    {
                        entryUrl = server + hccConfig.files[i].url;
                    }
                    using (HttpResponseMessage response = await httpClient.GetAsync(entryUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                    {
                        string headerString = hcClientLocale.GetCachedHeader(response.Headers);

                        Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

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

                            string[] externalURLs =  { "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" };
                            if (hccConfig.files[i].replace)
                            {
                                string html = Encoding.UTF8.GetString(data, 0, data.Length);
                                foreach (var pat in externalURLs)
                                {
                                    html = html.Replace(pat, "external/" + pat.Substring(7));
                                }
                                data = Encoding.UTF8.GetBytes(html);
                            }
                            byte zipped = hcClientLocale.zipped;
                            if (hccConfig.files[i].zipped == "1")
                                zipped = 1;
                            else if (hccConfig.files[i].zipped == "0")
                                zipped = 0;

                            if (hcClientLocale.encryptFunction != null)
                            {
                                data = hcClientLocale.encryptFunction(hccConfig.files[i].url, data);

                                await hcClientLocale.AddCachedStreamAsync(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url),
                                    data,
                                    headers: headerString,
                                    zipped: hcClientLocale.zipped,
                                    encrypted: 1).ConfigureAwait(false);
                            }
                            else
                            {
                                await hcClientLocale.AddCachedStreamAsync(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url),
                                    data,
                                    headers: headerString,
                                    zipped: zipped).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ToDo log this error
                    import_status_set("ERROR:entry " + ex);
                    return;
                }
            }
            await hcClientLocale.AddCachedMetadataAsync("hcc.defaultHTML", hccDefaultHTML).ConfigureAwait(false);

            import_status_set("got list of external from " + server + " with " + hccConfig.externalUrl.Length.ToString() + " entries");
            for (int i = 0; i < hccConfig.externalUrl.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.externalUrl.Length.ToString());
                try
                {
                    await addUrlAsync(httpClient, hccConfig.externalUrl[i], hccConfig.zipped).ConfigureAwait(false);
#if fasle
                    using (HttpResponseMessage response = await httpClient.GetAsync(hccConfig.externalUrl[i].url, HttpCompletionOption.ResponseContentRead))
                    {
                        string headerString = hcClient.getCachedHeader(response.Headers);

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
                            Byte zipped = hcClient.zipped;
                            if (hccConfig.zipped != null)
                            {
                                zipped = Byte.Parse(hccConfig.zipped);
                            }
                            if (hccConfig.externalUrl[i].zipped != null)
                            {
                                zipped = Byte.Parse(hccConfig.externalUrl[i].zipped);
                            }

                            if (hcClient.encryptFunction != null)
                            {
                                data = hcClient.encryptFunction(hccConfig.externalUrl[i].url, data);

                                hcClient.AddCachedStream(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped, encrypted: 1);
                            }
                            else
                            {
                                hcClient.AddCachedStream(hccConfig.externalUrl[i].url, data, headers: headerString, zipped: zipped);
                            }
                        }
                    }
#endif
                }
                catch (Exception ex)
                {
                    // ToDo log this error
                    import_status_set("ERROR:entry " + ex);
                    return;
                }
            }// for
            // get the   
            try
            {
                string externalJSUrl = server + "entry?site=" + site + "&url=" + hccConfig.externalJS.js;
                if ( !cbNodeJS.IsToggled )
                {
                    externalJSUrl = server + ".hccExternal.js";
                }
                using (HttpResponseMessage response = await httpClient.GetAsync(externalJSUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    code_set(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
            catch (Exception ex)
            {
                // ToDo log this error
                import_status_set("ERROR:entry " + ex);
                return;
            }
        }

        private async Task addUrlAsync(HttpClient httpClient, Externalurl externalUrl, string zippedDef)
        {
            using (HttpResponseMessage response = await httpClient.GetAsync(externalUrl.url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
            {
                string headerString = hcClient.GetCachedHeader(response.Headers);

                Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

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
                    Byte zipped = hcClient.zipped;
                    if ( !string.IsNullOrEmpty(zippedDef) )
                    {
                        zipped = Byte.Parse(zippedDef);
                    }
                    if (externalUrl.zipped != null)
                    {
                        zipped = Byte.Parse(externalUrl.zipped);
                    }

                    if (hcClient.encryptFunction != null)
                    {
                        data = hcClient.encryptFunction(externalUrl.url, data);

                        await hcClient.AddCachedStreamAsync(externalUrl.url, data, headers: headerString, zipped: zipped, encrypted: 1).ConfigureAwait(false);
                    }
                    else
                    {
                        await hcClient.AddCachedStreamAsync(externalUrl.url, data, headers: headerString, zipped: zipped).ConfigureAwait(false);
                    }
                }
            }
        }
        private void code_set(string str)
        {
            Device.BeginInvokeOnMainThread(() => {
                code.Text = str;
            });
            
        }
        private void import_status_set(string status)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblImportStatus.Text = status;
            });
        }
        private void server_status_set(string msg)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblServerStatus.Text = msg;
            });
        }
        private void server_status_add(string msg)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblServerStatus.Text += msg;
            });
        }

        private async void btnExecute_ClickedAsync(object sender, EventArgs e)
        {
            server_status_set("");
            JSInt jsint = new JSInt();
            jsint.onError = ((errMsg) => {
                server_status_add(errMsg);
            });
            jsint.execute(code.Text);

            code_set(String.Join(Environment.NewLine, jsint.winext.addCachedAliasList) + String.Join(Environment.NewLine, jsint.winext.addCachedExternalDataList));
            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);

            HttpClient httpClient = new HttpClient();
            int i = 0;
            foreach (var url in jsint.winext.addCachedExternalDataList)
            {
                import_status_set("get external " + (i + 1).ToString() + " - " + jsint.winext.addCachedExternalDataList.Count.ToString());
                i++;

                Externalurl externalUrl = new Externalurl();
                externalUrl.url = url;
                externalUrl.zipped = "0";
                externalUrl.replace = true;
                await addUrlAsync(httpClient, externalUrl, null).ConfigureAwait(false);
            }
            foreach ( KeyValuePair<string,string> kv in jsint.winext.addCachedAliasList)
            {
                string aliasUrl = kv.Key;
                string url = kv.Value;
                await hcClientLocale.AddCachedAliasUrlAsync(aliasUrl, url).ConfigureAwait(false);
            }
            updateDatabaseTab();
        }

        private async void btnReset_Clicked(object sender, EventArgs e)
        {
            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);
            await hcClientLocale.ResetAsync();
            updateDatabaseTab();            
        }

        private async void btnLoop_Clicked(object sender, EventArgs e)
        {
            var hcClientLocale = (BindingContext as HMS.Net.Http.HttpCachedClient);

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
                    Task.Delay(100).Wait();
                    Device.BeginInvokeOnMainThread(() => btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString());
                }
            });
            Task.Run(async () =>
            {
                for (i2 = 0; i2 < 200; i2++)
                {
                    HccResponse hccResponse = await hcClientLocale.GetCachedStringAsync(debugUrl).ConfigureAwait(false);
                    // , (json, hi) => System.Diagnostics.Debug.WriteLine("tbLoop_Clicked2 " + i1.ToString() + "  " + i2.ToString())).ConfigureAwait(false);
                    Task.Delay(50).Wait();
                    Device.BeginInvokeOnMainThread(() => btnLoop.Text = "Loop " + i1.ToString() + "  " + i2.ToString());
                }
            });
            updateDatabaseTab();
        }

        private void SegmentedTabControl_ItemTapped(object sender, int e)
        {
            SegmentedTabControl.FormsPlugin.SegmentedTabControl segmentedTabControl = (SegmentedTabControl.FormsPlugin.SegmentedTabControl)sender;
            Grid[] grdList = { grdNodeJS, grdHccConfig, grdFilesystem };
            for (int c = 0; c< segmentedTabControl.Children.Count; c++)
            {
                grdList[c].IsVisible = (c == e);
            }
            switch (e)
            {
                case 0:
                    System.Diagnostics.Debug.WriteLine($"Selected: {e})");
                    break;
                case 1:
                    System.Diagnostics.Debug.WriteLine($"Selected: {e})");
                    break;
                case 2:
                    System.Diagnostics.Debug.WriteLine($"Selected: {e})");
                    break;
                // If set to -1 then NO segments will be selected
                default:
                    System.Diagnostics.Debug.WriteLine($"No Segments Selected: {e}");
                    break;
            }

        }

        private void btnImportFS_Clicked(object sender, EventArgs e)
        {

        }

        private void btnImportHccConfig_Clicked(object sender, EventArgs e)
        {

        }
    }

    internal class BindingObj
    {
        public SqLiteCacheItem SelectedItem { get; set; }
    }
}
