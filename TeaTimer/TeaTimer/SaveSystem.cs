using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Plugin.LocalNotifications;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace TeaTimer
{
    public class SaveSystem
    {
        private MainPage Page;
        private Grid layout;

        public SaveSystem(MainPage mainPage, Grid grid)
        {
            Page = mainPage;
            layout = grid;
        }


        public void Save()
        {
            int tempCount = 0;
            
            List<IView> buttons = layout.Children.Where(x => { return x.GetType() == typeof(LongPressButton); }).ToList();
            ClearSave();

            foreach (View button in buttons)
            {
                tempCount++;
                string Time = Page.buttons[(LongPressButton)button].seconds.ToString();
                string TimeLeft = Page.buttons[(LongPressButton)button].temp.ToString();
                string Stop;
                if (Page.buttons[(LongPressButton)button].stop)
                    Stop = "Stop";
                else Stop = "Start";
                    string temp = Time + "_" + TimeLeft + "_" + Stop;
                Preferences.Set("Button_" + tempCount.ToString(), temp);
            }
        }

        public void Load()
        {
            for (int i = 0; i < 8; i++)
            {
                string[] savedData = Preferences.Get("Button_" + (i + 1).ToString(), "Empty").Split('_');

                if (savedData[0] == "Empty")
                    return;

                LongPressButton temp = Page.CreateButton();
                Page.buttons[temp].seconds = Convert.ToInt32(savedData[0]);
                Page.buttons[temp].temp = Convert.ToInt32(savedData[1]);
                temp.Text = (Page.buttons[temp].seconds - Page.buttons[temp].temp).ToString();
                Page.buttons[temp].StartStop(savedData[2]);
            }
        }

        public void ClearSave()
        {
            for (int i = 1; i < 8; i++)
            {
                Preferences.Set("Button_" + i.ToString(), null);
            }
        }
    }
}
