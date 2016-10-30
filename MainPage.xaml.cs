// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Microsoft.IoT.Lightning.Providers;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private int current = 0;
        private int[] colors = {0xFF00, 0x00FF, 0x0FF0, 0xF00F};
        private PwmPin pinR;
        private PwmPin pinG;
        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();

            LED.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

                var pwmControllers = PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider()).AsTask().Result;

                var pwm = pwmControllers[1]; // use the on-device controller

                if (pwm == null)
                {
                    pinR = pinG = null;
                    GpioStatus.Text = "There is no PWM controller on this device.";
                }
                else
                {
                    pwm.SetDesiredFrequency(50);

                    pinR = pwm.OpenPin(17);
                    pinR.Polarity = PwmPulsePolarity.ActiveLow;
                    pinR.SetActiveDutyCyclePercentage(0);
                    pinR.Stop();

                    pinG = pwm.OpenPin(18);
                    pinG.Polarity = PwmPulsePolarity.ActiveLow;
                    pinG.SetActiveDutyCyclePercentage(0);
                    pinG.Stop();

                    GpioStatus.Text = "PWM pins initialized correctly.";

                    pinR.Start();
                    pinG.Start();

                    if (pinR != null && pinG != null)
                    {
                        timer.Start();
                    }
                }
            }
        }

        private double Map(double x, int inMin, int inMax, int outMin, int outMax)
        {
            return ((x - inMin)*(outMax - outMin)/(inMax - inMin) + outMin);
        }

        private void SetColor(int color)
        {
            double rVal = color >> 8;
            double gVal = color & 0x00FF;

            rVal = Map(rVal, 0, 255, 0, 1);
            gVal = Map(gVal, 0, 255, 0, 1);

            pinR.SetActiveDutyCyclePercentage(rVal);
            pinG.SetActiveDutyCyclePercentage(gVal);
        }

        private void Timer_Tick(object sender, object e)
        {
            var color = colors[current++];

            if (current == colors.Length)
            {
                current = 0;
            }

            SetColor(color);
        }
    }
}
