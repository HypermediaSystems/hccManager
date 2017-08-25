using HMS.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace hccManager
{
    public partial class App : Application
    {
        public App(ISql SQL)
        {
            InitializeComponent();

            MainPage = new hccManager.MainPage(SQL);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
