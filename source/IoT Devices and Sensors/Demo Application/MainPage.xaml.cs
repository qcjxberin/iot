﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Porrey.Uwp.IoT.Devices;
using Porrey.Uwp.IoT.Devices.KeyPad;
using Porrey.Uwp.IoT.Devices.Lifx;
using Porrey.Uwp.IoT.Sensors;
using Porrey.Uwp.Ntp;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyTime3
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			await TestKeyPad1();
			base.OnNavigatedTo(e);
		}

		private Task TestKeyPad1()
		{
			IGpioPinMapping[] mappings = new IGpioPinMapping[]
			{
				new GpioPinMappingRow(row: 1, pinNumber: 18),
				new GpioPinMappingColumn(column: 1, pinNumber: 22),
				new GpioPinMappingColumn(column: 2, pinNumber: 23),
				new GpioPinMappingColumn(column: 3, pinNumber: 24),
				new GpioPinMappingColumn(column: 4, pinNumber: 25),
			};

			IKeyPad keypad = new Membrane1x4(mappings);
			keypad.KeyEvent += Keypad_KeyEvent;
			return Task.FromResult(0);
		}

		private Task TestKeyPad2()
		{
			IGpioPinMapping[] mappings = new IGpioPinMapping[]
			{
				new GpioPinMappingColumn(column: 3, pinNumber: 13),
				new GpioPinMappingColumn(column: 2, pinNumber: 16),
				new GpioPinMappingColumn(column: 1, pinNumber: 18),
				new GpioPinMappingRow(row: 4, pinNumber: 22),
				new GpioPinMappingRow(row: 3, pinNumber: 23),
				new GpioPinMappingRow(row: 2, pinNumber: 24),
				new GpioPinMappingRow(row: 1, pinNumber: 25),
			};

			IKeyPad keypad = new Membrane3x4(mappings);
			keypad.KeyEvent += Keypad_KeyEvent;
			return Task.FromResult(0);
		}

		private void Keypad_KeyEvent(object sender, KeyPadEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("Key = {0}, Event Type = {1}", e.KeyValue, e.KeyDownEventType));
		}

		private async Task TestLifx()
		{
			// ***
			// *** Initialize the API
			// ***
			LifxApi api = new LifxApi("c1c1b8b4760452f88074f9457b728ce8b2c92f97cbabcd4dec6d7ccabd82011c");

			// ***
			// *** Validate a color
			// ***
			Color color = await api.ValidateColor("#ff0000");

			// ***
			// *** Lists the lights
			// ***
			IEnumerable<Light> lights = await api.ListLights(Selector.All);

			// ***
			// *** List the scenes
			// ***
			IEnumerable<Scene> sceneList = await api.ListScenes();

			// ***
			// *** Select a single light
			// ***			
			Light light = lights.Where(t => t.Label == "iWindow").SingleOrDefault();

			// ***
			// ***
			// ***
			Mix mix = new Mix()
			{
				States = new SentState[]
				{
					new SentState() { Brightness = 1, Color = "Red" },
					new SentState() { Brightness = 1, Color = "Green" },
					new SentState() { Brightness = 1, Color = "Blue" },
					new SentState() { Power = "off" }
				},
				Defaults = new SentState() { Power = "on", Duration = 1, }
			};

			await api.Cycle(light, mix);

			// ***
			// *** Create a list selector
			// ***
			ISelector items = Selector.CreateList(lights.ElementAt(0), lights.ElementAt(1));

			if (light != null)
			{
				await api.TogglePower(light, 3);
				await api.SetState(light, new SentState() { Power = "on", Duration = 2, Color = "#1fa1b2" });
			}
		}

		private async Task TestTemperatureSensor()
		{
			// ***
			// *** Temperature and Humidity
			// ***
			Htu21df temp = new Htu21df();
			await temp.InitializeAsync();

			Htu21df.BatteryStatus batteryStatus = await temp.GetBatteryStatus();
			Htu21df.HeaterStatus heaterStatus = await temp.GetHeaterStatus();

			Htu21df.Resolution res1 = await temp.GetResolution();

			await temp.SetResolution(Htu21df.Resolution.Rh8Temp12);
			Htu21df.Resolution res2 = await temp.GetResolution();

			await temp.SetResolution(Htu21df.Resolution.Rh10Temp13);
			Htu21df.Resolution res3 = await temp.GetResolution();

			await temp.SetResolution(Htu21df.Resolution.Rh11Temp11);
			Htu21df.Resolution res4 = await temp.GetResolution();

			await temp.SetResolution(Htu21df.Resolution.Rh12Temp14);
			Htu21df.Resolution res5 = await temp.GetResolution();

			float h = await temp.ReadHumidityAsync();
			float t1 = await temp.ReadTemperatureAsync();
		}

		private async Task TestLightSensor()
		{
			// ***
			// *** Light sensor
			// ***
			SI1145 uv = new SI1145();
			await uv.InitializeAsync();
			int value1 = await uv.GetVisibleAsync();
			int value2 = await uv.GetUvAsync();
			int value3 = await uv.GetIrAsync();
			int value4 = await uv.GetProximityAsync();
		}

		private async Task TestClock()
		{
			// ***
			// *** Clock
			// ***
			Ds1307 dtc = new Ds1307();
			await dtc.InitializeAsync();

			// ***
			// *** Get the date and time from the clock
			// ***
			DateTime dt = await dtc.GetAsync();

			// ***
			// *** Create an NTP client and get the date and time
			// ***
			NtpClient ntp = new NtpClient();
			DateTime? ndt = await ntp.GetAsync("0.pool.ntp.org", "1.pool.ntp.org", "2.pool.ntp.org", "3.pool.ntp.org");

			// ***
			// *** Update the clock if we have a result from the servers
			// ***
			if (ndt.HasValue)
			{
				await dtc.SetAsync(ndt.Value);
			}
		}
	}
}
