using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui;
namespace TeaTimer
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {
        }
    }
}
