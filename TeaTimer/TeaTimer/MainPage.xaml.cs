using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Android;
using Plugin.LocalNotifications;
using System.Threading;
using System.Numerics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ViewExtensions = Microsoft.Maui.Controls.ViewExtensions;
using Microsoft.Maui.Controls.Internals;
using System.Runtime.CompilerServices;
using TeaTimer;

namespace TeaTimer
{
    public partial class MainPage : ContentPage
    {
        int temp = 1;
        bool left = false;

        public event EventHandler tick;

        public Dictionary<LongPressButton, Timer> buttons = new Dictionary<LongPressButton, Timer>();

        ContextMenu menu;

        SaveSystem System;
        public MainPage()
        {
            InitializeComponent();
            System = new SaveSystem(this, layout);
            System.Load();
            Create.Clicked += (s, e) => { CreateButton(); };
            Device.StartTimer(new TimeSpan(0, 0, 1), () => { OnTick(null); return true; });
            Device.StartTimer(new TimeSpan(0, 0, 2), () => { System.Save(); return true; });
        }

        public LongPressButton CreateButton()
        {
            if (buttons.Count >= 8)
                return null;

            LongPressButton button = new LongPressButton
            {
                Text = "Timer",
            };

            button.Opacity = 0;
            layout.Children.Add(button);
            ViewExtensions.FadeTo(button, 1);
            Grid.SetRow(button, temp);
            Grid.SetColumn(button, Convert.ToInt32(left));
            button.Position = new Vector2(Convert.ToInt32(left), temp);
            buttons.Add(button, new Timer(Convert.ToInt32(Input.Text), button));
            button.Text = Input.Text;
            buttons[button].OnCreate();
            button.LongPress += (s, e) =>
            {
                if (menu == null)
                    menu = new ContextMenu(layout, button);
                else return;
                InitializeContextMenu();
            };
            button.Clicked += (s, e) =>
            {
                buttons[button].StartStop();
            };

            tick += buttons[button].Tick;

            left = !left;
            if (!left)
                temp++;

            return button;
        }


        void InitializeContextMenu()
        {
            if (menu == null)
                return;

            menu.Delete += (s, e) => { Delete(menu.button); };
            ViewExtensions.FadeTo(menu.button, 0.5);
            menu.Close += CloseContextMenu;
            menu.Reset += buttons[menu.button].Reset;
            menu.Edit += buttons[menu.button].Edit;
            menu.Edit += RequestCloseContextMenu;
            menu.Reset += RequestCloseContextMenu;
            menu.button.IsEnabled = false;
        }

        void Delete(LongPressButton button)
        {
            menu.RequestClose();
            buttons.Remove(button);
            ViewExtensions.FadeTo(button, 0);
            layout.Children.Remove(button);
            var tempArray = layout.Children.Where(x => {return x.GetType() == typeof(LongPressButton); });
            temp = 1;
            left = false;
            foreach (var item in tempArray)
            {
                layout.SetRow(item, temp);
                layout.SetColumn(item, Convert.ToInt32(left));
                ((LongPressButton)item).Position = new Vector2(Convert.ToInt32(left), temp);
                left = !left;

                if (!left)
                    temp++;
            }

        }

        public async void RequestCloseContextMenu(object s, EventArgs e)
        {
            await Task.Delay(50);
            menu.RequestClose();
        }

        void CloseContextMenu(object s, EventArgs e)
        {
            menu.button.IsEnabled = true;
            ViewExtensions.FadeTo(menu.button, 1);
            menu = null;
        }

        protected virtual void OnTick(EventArgs e)
        {
            EventHandler handler = tick;
            handler?.Invoke(this, e);
        }
    }

    public class Timer
    {
        public int seconds;
        public int temp;

        public int restTime;
        public bool stop = false;

        public LongPressButton Button { get; private set; }

        public Timer(int seconds, LongPressButton button)
        {
            this.seconds = seconds;
            this.Button = button;
        }

        public void OnCreate()
        {
            StartStop("Stop");
            restTime = seconds - temp;
        }

        public void StartStop(string input = null)
        {
            if (input == null)
                stop = !stop;
            else if (input == "Start")
                stop = false;
            else if (input == "Stop")
                stop = true;

            if (stop)
                Button.BackgroundColor = Colors.Red;
            else
                Button.BackgroundColor = Colors.Green;
        }

        public void Tick(object s, EventArgs e)
        {
            if (stop)
                return;

            if (seconds <= temp)
                Alarm();

            restTime = seconds - temp;
            Button.Text = restTime.ToString();
            temp++;
        }

        public async void Reset(object sender, EventArgs e)
        {
            await ViewExtensions.RotateTo(Button, 20, 100);
            await ViewExtensions.RotateTo(Button, -20, 100);
            await ViewExtensions.RotateTo(Button, 0, 100);
            temp = 0;
            restTime = seconds - temp;
            Button.Text = restTime.ToString();
        }

        void Alarm()
        {
            temp = 0;
            StartStop("Stop");
            CrossLocalNotifications.Current.Show("Alarm", "The Timer ran out");
        }

        public async void Edit(object sender, EventArgs e)
        {
            seconds = Convert.ToInt32(await App.Current.MainPage.DisplayPromptAsync("Edit Time", "Write new Time", "OK", "Cancel", seconds.ToString(), -1, Keyboard.Numeric, seconds.ToString()));
            Reset(this, EventArgs.Empty);
            await ViewExtensions.RotateTo(Button, 360);
            return;
        }

        #region LongPress
        //Task CheckLongPress()
        //{
        //    return Task.Run(async () =>
        //    {
        //        await Task.Delay(1000, token.Token);
        //        LongPress?.Invoke(this, new EventArgs());
        //        return Task.CompletedTask;
        //    });
        //}

        //public void StartPress(object s, EventArgs e)
        //{
        //    token = new CancellationTokenSource();
        //    CheckLongPress();
        //}

        //public void EndPress(object s, EventArgs e)
        //{
        //    token.Cancel();
        //}
        #endregion
    }

    class ContextMenu
    {
        Grid layout;
        Grid Option;
        
        public LongPressButton button { get; private set; }

        public event EventHandler Delete;
        public event EventHandler Edit;
        public event EventHandler Reset;
        public event EventHandler Close;

        List<Button> buttons = new List<Button>();

        public ContextMenu(Grid grid, LongPressButton button)
        {
            layout = grid;
            Close += CloseMethod;
            this.button = button;
            LongPress();
        }

        void LongPress()
        {
            Option = new Grid
            {
                RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } },
                ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
                ColumnSpacing = 0,
                RowSpacing = 0,
            };
            Option.Scale = 0;
            layout.Add(Option, (int)button.Position.X, (int)button.Position.Y);


            Button button1 = new Button { Text = "Delete" };
            Option.Add(button1, 0, 0);
            

            buttons.Add(button1);
            button1.Clicked += (s, e) => { Delete.Invoke(this, new EventArgs()); };

            Button button2 = new Button { Text = "Edit" };
            Option.Add(button2, 0, 1);
            buttons.Add(button2);
            button2.Clicked += (s, e) => { Edit.Invoke(this, new EventArgs()); };

            Button button3 = new Button { Text = "Reset" };
            Option.Add(button3, 1, 0);
            buttons.Add(button3);
            button3.Clicked += (s, e) => { Reset.Invoke(this, new EventArgs()); };

            Button button4 = new Button { Text = "Close" };
            Option.Add(button4, 1, 1);
            buttons.Add(button4);
            button4.Clicked += (s, e) => { Close.Invoke(this, new EventArgs()); };
            ViewExtensions.ScaleTo(Option, 1);
        }

        public void RequestClose()
        {
            Close.Invoke(this, new EventArgs());
        }

        async void CloseMethod(object s, EventArgs e)
        {
            await ViewExtensions.ScaleTo(Option, 0);
            foreach (Button b in buttons)
                Option.Children.Remove(b);
            layout.Children.Remove(Option);
        }
    }

    public static class Extension
    {
        public static void Add(this Grid grid, IView view, int Row, int Column)
        {
            grid.Add(view);
            grid.SetRow(view, Row);
            grid.SetColumn(view, Column);
        }
    }
}
