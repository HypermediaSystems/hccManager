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
        private ISql SQL;
        private SqLiteCache sqLiteCache = null;
        private HttpCachedClient hcClient;
        public MainPage(ISql SQL)
        {
            InitializeComponent();
            listView.ItemSelected += listView_ItemSelected;
            listView.ItemTapped += ListView_ItemTapped;

            this.SQL = SQL; //  Xamarin.Forms.DependencyService.Get<iSQL>();
            this.sqLiteCache = new SqLiteCache(SQL, "");
            this.hcClient = new HttpCachedClient(this.sqLiteCache);

            BindingContext = this.hcClient;

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
            BindingContext = this.hcClient;
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

        private async void btnBackup_Clicked(object sender, EventArgs e)
        {
            Boolean ret = false;
            string errMsg = "";
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "upload");

            lblServerStatus.Text = "Backuping to " + serverUrl + " ...";
            // System.Net.Http.Headers.AuthenticationHeaderValue authenticationHeaderValue = null;
            try
            {
                ret = await hcClient.BackupAsync(serverUrl);
            }
            catch (Exception ex)
            {
                // ToDo log this error
                ret = false;
                errMsg = ex.ToString();

            }
            if (ret == false)
            {
                lblServerStatus.Text = "Backuping to " + serverUrl + " ended with error. " + errMsg;
            }
            else
            {
                lblServerStatus.Text = "Backuping to " + serverUrl + " done.";
            }
            updateDatabaseTab();
        }

        private async void btnRestore_Clicked(object sender, EventArgs e)
        {
            Boolean ret = false;
            string errMsg = "";

            var hcClientLocal = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string serverUrl = tbServer.Text.Trim();

            serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient._dbName + ".sqlite");

            lblServerStatus.Text = "Restoring from " + serverUrl + " ...";

            try
            {
                ret = await hcClientLocal.RestoreAsync(serverUrl);
            }
            catch (Exception ex)
            {
                // ToDo log this error
                ret = false;
                errMsg = ex.ToString();

            }

            if (ret == false)
            {
                lblServerStatus.Text = "Restoring from " + serverUrl + " ended with error. " + errMsg;
            }
            else
            {
                lblServerStatus.Text = "Restoring from " + serverUrl + " done.";
            }

            updateDatabaseTab();

        }
        private void btnDeleteAll_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);
            hcClient.DeleteAllCachedData();

            updateDatabaseTab();

        }
        private void tbLoop_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string debugUrl = "debugUrl";
            hcClient.AddCachedString(debugUrl, "DebugData");


            int i1 = 0;
            int i2 = 0;
            Task.Run(async () =>
            {
                for (i1 = 0; i1 < 100; i1++)
                {
                    string url = tbUrl.Text.Trim();
                    await hcClient.GetCachedStringAsync(debugUrl, (json, hi) =>
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
                    await hcClient.GetCachedStringAsync(debugUrl, (json, hi) =>
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

            if (!server.EndsWith("/",StringComparison.CurrentCulture))
                server += "/";

            import_status_set("getting list from " + server + " ... ");
            // get the list
            HttpClient httpClient = new HttpClient();

            HccConfig.Rootobject hccConfig;
            try
            {
                string configUrl = server + "config?site=" + site;
                if( cbNodeJS.IsToggled == false )
                {
                    configUrl = server + ".hccConfig.json";
                }
                using (HttpResponseMessage response = await httpClient.GetAsync(configUrl, HttpCompletionOption.ResponseContentRead))
                {
                    string json = await response.Content.ReadAsStringAsync();

                    hccConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<HccConfig.Rootobject>(json);
                }
            }
            catch (Exception ex)
            {
                // ToDo log this error
                import_status_set("ERROR:site " + ex.ToString());
                return;
            }


            if ( !string.IsNullOrEmpty(hccConfig.fileList) )
            {
                try
                {
                    string configUrl = server + "config?site=" + site + "&url=" + hccConfig.fileList;
                    if (cbNodeJS.IsToggled == false)
                    {
                        configUrl = server + hccConfig.fileList;
                    }
                    using (HttpResponseMessage response = await httpClient.GetAsync(configUrl, HttpCompletionOption.ResponseContentRead))
                    {
                        string json = await response.Content.ReadAsStringAsync();

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
                    import_status_set("ERROR:site " + ex.ToString());
                    return;
                }
            }


            import_status_set("got list from " + server + " with " + hccConfig.files.Length.ToString() + " entries");

            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            hcClient.AddCachedMetadata("hcc.url", hccConfig.url);
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
                    if (cbNodeJS.IsToggled == false)
                    {
                        entryUrl = server + hccConfig.files[i].url;
                    }
                    using (HttpResponseMessage response = await httpClient.GetAsync(entryUrl, HttpCompletionOption.ResponseContentRead))
                    {
                        string headerString = hcClient.GetCachedHeader(response.Headers);

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
                            byte zipped = hcClient.zipped;
                            if (hccConfig.files[i].zipped == "1")
                                zipped = 1;
                            else if (hccConfig.files[i].zipped == "0")
                                zipped = 0;

                            if (hcClient.encryptFunction != null)
                            {
                                data = hcClient.encryptFunction(hccConfig.files[i].url, data);

                                hcClient.AddCachedStream(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url), data, headers: headerString, zipped: hcClient.zipped, encrypted: 1);
                            }
                            else
                            {

                                hcClient.AddCachedStream(HccUtil.url_join(hccConfig.url, hccConfig.files[i].url), data, headers: headerString, zipped: zipped); // hcc.zipped);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ToDo log this error
                    import_status_set("ERROR:entry " + ex.ToString());
                    return;
                }

            }
            hcClient.AddCachedMetadata("hcc.defaultHTML", hccDefaultHTML);

            import_status_set("got list of external from " + server + " with " + hccConfig.externalUrl.Length.ToString() + " entries");
            for (int i = 0; i < hccConfig.externalUrl.Length; i++)
            {
                import_status_set("get entry " + (i + 1).ToString() + " - " + hccConfig.externalUrl.Length.ToString());
                try
                {
                    await addUrl(httpClient, hccConfig.externalUrl[i], hccConfig.zipped);
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
                    import_status_set("ERROR:entry " + ex.ToString());
                    return;
                }
            }// for
            // get the   
            try
            {
                string externalJSUrl = server + "entry?site=" + site + "&url=" + hccConfig.externalJS.js;
                if (cbNodeJS.IsToggled == false)
                {
                    externalJSUrl = server + ".hccExternal.js";
                }
                using (HttpResponseMessage response = await httpClient.GetAsync(externalJSUrl, HttpCompletionOption.ResponseContentRead))
                {
                    string headerString = hcClient.GetCachedHeader(response.Headers);

                    code.Text = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                // ToDo log this error
                import_status_set("ERROR:entry " + ex.ToString());
                return;
            }
        }
        private async Task addUrl(HttpClient httpClient, Externalurl externalUrl, string zippedDef)
        {
            using (HttpResponseMessage response = await httpClient.GetAsync(externalUrl.url, HttpCompletionOption.ResponseContentRead))
            {
                string headerString = hcClient.GetCachedHeader(response.Headers);

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

                        hcClient.AddCachedStream(externalUrl.url, data, headers: headerString, zipped: zipped, encrypted: 1);
                    }
                    else
                    {
                        hcClient.AddCachedStream(externalUrl.url, data, headers: headerString, zipped: zipped);
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

        private async void btnExecute_Clicked(object sender, EventArgs e)
        {
            lblServerStatus.Text = "";
            JSInt jsint = new JSInt();
            jsint.onError = ((errMsg) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine(errMsg);
                    lblServerStatus.Text += errMsg;
                });
            });
            jsint.execute(code.Text);

            code.Text = String.Join(Environment.NewLine, jsint.winext.addCachedAliasList) + String.Join(Environment.NewLine, jsint.winext.addCachedExternalDataList);
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

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
                await addUrl(httpClient, externalUrl, null);

            }
            foreach ( KeyValuePair<string,string> kv in jsint.winext.addCachedAliasList)
            {
                string aliasUrl = kv.Key;
                string url = kv.Value;
                hcClient.AddCachedAliasUrl(aliasUrl, url);

            }
            updateDatabaseTab();

        }

        private void btnReset_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);
            hcClient.Reset();
            updateDatabaseTab();

        }

        private void btnLoop_Clicked(object sender, EventArgs e)
        {
            var hcClient = (BindingContext as HMS.Net.Http.HttpCachedClient);

            string debugUrl = "debugUrl";
            hcClient.AddCachedString(debugUrl, "DebugData");


            int i1 = 0;
            int i2 = 0;
            Task.Run(async () =>
            {
                for (i1 = 0; i1 < 100; i1++)
                {
                    string url = tbUrl.Text.Trim();
                    await hcClient.GetCachedStringAsync(debugUrl, (json, hi) =>
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
                    await hcClient.GetCachedStringAsync(debugUrl, (json, hi) =>
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
            updateDatabaseTab();

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
