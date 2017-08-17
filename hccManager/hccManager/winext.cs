using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccManager
{
    class WinExt
    {
        public List<string> addCachedExternalDataList;
        public Dictionary<string,string> addCachedAliasList;

        public WinExt()
        {
            addCachedExternalDataList = new List<string>();
            addCachedAliasList = new Dictionary<string, string>();
        }
        public void AddCachedExternalData(string url)
        {
            System.Diagnostics.Debug.WriteLine("AddCachedExternalData " + url);
            addCachedExternalDataList.Add(url);
        }
        public void AddCachedAlias(string aliasUrl, string url)
        {
            System.Diagnostics.Debug.WriteLine("AddCachedAlias " + aliasUrl + " " + url);
            if (!addCachedAliasList.ContainsKey(aliasUrl))
            {
                addCachedAliasList.Add(aliasUrl, url);
            }
        }
        
    }
    class JSInt
    {
        public delegate void OnError(string errMsg);
        public OnError onError = null;

        public WinExt winext;

        private Jint.Engine engine;
        public JSInt()
        {
            engine = new Jint.Engine((options) => { options.DebugMode(); });
            // engine.Step += (jsSender, info) => {
            //     Device.BeginInvokeOnMainThread(() => {
            //         System.Diagnostics.Debug.WriteLine(info.CurrentStatement);
            //     });
            //
            //     return Jint.Runtime.Debugger.StepMode.Into;
            // };
            winext = new WinExt();
            engine.SetValue("external", winext);
            engine.SetValue("print", new Action<object>(Print));
        }
        public void execute(string code)
        {
            try
            {
                engine.Execute(code);
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine("JSParse Error: " + ex.Message);
                    onError?.Invoke("JSParse Error: " + ex.Message);
                });
            }
        }
        public void Print(object s)
        {
            if (s == null)
                s = "null";
            Device.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine(s);
            });
        }
    }
}
