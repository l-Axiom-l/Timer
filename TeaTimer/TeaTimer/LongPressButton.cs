using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace TeaTimer
{
    public partial class LongPressButton : Button
    {
        public LongPressButton()
        {
            OnCreate();
        }

        public Vector2 Position;

        public event EventHandler LongPress;
        CancellationTokenSource token;

        void OnCreate()
        {
            Pressed += StartPress;
            Released += EndPress;
        }

        async void CheckLongPress()
        {
            try
            {
            await Task.Delay(500, token.Token);
            LongPress?.Invoke(this, EventArgs.Empty);
            }
            catch (TaskCanceledException ex)
            {

            }

        }

        void StartPress(object s, EventArgs e)
        {
            token = new CancellationTokenSource();
            CheckLongPress();
        }

        void EndPress(object s, EventArgs e)
        {
            token.Cancel(false);
        }
    }
}
