﻿using ApiLibs.General;
using Com.AugustCellars.CoAP;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Tomidix.NetStandard.Tradfri.Extensions;
using Tomidix.NetStandard.Tradfri.Models;

namespace Tomidix.NetStandard.Tradfri.Controllers
{
    public class DeviceController : SubService
    {
        public DeviceController(TradfriController controller) : base(controller) { }

        /// <summary>
        /// Acquires TradfriDevice object
        /// </summary>
        /// <param name="refresh">If set to true, than it will ignore existing cached value and ask the gateway for the object</param>
        /// <returns></returns>
        public Task<TradfriDevice> GetTradfriDevice(long id)
        {
            return MakeRequest<TradfriDevice>($"/{(int)TradfriConstRoot.Devices}/{id}");
        }

        private static bool HasLight(TradfriDevice device)
        {
            return device?.LightControl != null;
        }

        private static bool HasControl(TradfriDevice device)
        {
            return device?.Control != null;
        }

        /// <summary>
        /// Changes the color of the light device
        /// </summary>
        /// <param name="device">A <see cref="TradfriDevice"/></param>
        /// <param name="value">A color from the <see cref="TradfriColors"/> class</param>
        /// <returns></returns>
        public async Task SetColor(TradfriDevice device, string value)
        {
            await SetColor(device.ID, value);
            if (HasLight(device))
            {
                device.LightControl[0].ColorHex = value;
            }
        }

        /// <summary>
        /// Changes the color of the light device
        /// </summary>
        /// <param name="id">Id of the device</param>
        /// <param name="value">A color from the <see cref="TradfriColors"/> class</param>
        /// <returns></returns>
        public async Task SetColor(long id, string value)
        {
            SwitchStateLightRequest set = new SwitchStateLightRequest()
            {
                Options = new[]
                {
                    new SwitchStateLightRequestOption()
                    {
                        Color = value
                    }
                }
            };
            await HandleRequest($"/{(int)TradfriConstRoot.Devices}/{id}", Call.PUT, content: set, statusCode: System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Changes the color of the light device
        /// </summary>
        /// <param name="device">A <see cref="TradfriDevice"/></param>
        /// <param name="r">Red component, 0-255</param>
        /// <param name="g">Green component, 0-255</param>
        /// <param name="b">Blue component, 0-255</param>
        /// <returns></returns>
        public async Task SetColor(TradfriDevice device, int r, int g, int b)
        {
            (int x, int y, int intensity) = ColorExtension.CalculateCIEFromRGB(r, g, b);
            await SetColor(device.ID, x, y, intensity);
            if (HasLight(device))
            {
                device.LightControl[0].ColorX = x;
                device.LightControl[0].ColorY = y;
            }
        }

        /// <summary>
        /// Changes the color of the light device
        /// </summary>
        /// <param name="id">Id of the device</param>
        /// <param name="value">A color from the <see cref="TradfriColors"/> class</param>
        /// <returns></returns>
        public async Task SetColor(long id, int x, int y, int? intensity)
        {
            SwitchStateLightXYRequest set = new SwitchStateLightXYRequest()
            {
                Options = new[]
                {
                    new SwitchStateLightXYRequestOption()
                    {
                        ColorX = x,
                        ColorY = y,
                        LightIntensity = intensity
                    }
                }
            };
            await HandleRequest($"/{(int)TradfriConstRoot.Devices}/{id}", Call.PUT, content: set, statusCode: HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Set Dimmer for Light Device
        /// </summary>
        /// <param name="device">A <see cref="TradfriDevice"/></param>
        /// <param name="value">Dimmer intensity (0-255)</param>
        public async Task SetDimmer(TradfriDevice device, int value)
        {
            await SetDimmer(device.ID, value);
            device.LightControl[0].Dimmer = value;
        }

        /// <summary>
        /// Set Dimmer for Light Device
        /// </summary>
        /// <param name="id">Id of the device</param>
        /// <param name="value">Dimmer intensity (0-255)</param>
        /// <returns></returns>
        public async Task SetDimmer(long id, int value)
        {
            SwitchStateLightRequest set = new SwitchStateLightRequest()
            {
                Options = new[]
                {
                    new SwitchStateLightRequestOption()
                    {
                        LightIntensity = value
                    }
                }
            };
            await HandleRequest($"/{(int)TradfriConstRoot.Devices}/{id}", Call.PUT, content: set, statusCode: System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Turns a specific light on or off
        /// </summary>
        /// <param name="device">A <see cref="TradfriDevice"/></param>
        /// <param name="state">On (True) or Off(false)</param>
        /// <returns></returns>
        public async Task SetLight(TradfriDevice device, bool state)
        {
            await SetLight(device.ID, state);
            if (HasLight(device))
            {
                device.LightControl[0].State = state ? Bool.True : Bool.False;
            }
        }

        /// <summary>
        /// Turns a specific light on or off
        /// </summary>
        /// <param name="id">Id of the device</param>
        /// <param name="state">On (True) or Off(false)</param>
        /// <returns></returns>
        public async Task SetLight(long id, bool state)
        {
            SwitchStateLightRequest set = new SwitchStateLightRequest()
            {
                Options = new[]
                {
                    new SwitchStateLightRequestOption()
                    {
                        IsOn = state ? 1 : 0
                    }
                }
            };
            await HandleRequest($"/{(int)TradfriConstRoot.Devices}/{id}", Call.PUT, content: set);
        }

        /// <summary>
        /// Turns a specific control outlet on or off
        /// </summary>
        /// <param name="device">An <see cref="TradfriDevice"/></param>
        /// <param name="state">On (True) or Off (false)</param>
        /// <returns></returns>
        public async Task SetOutlet(TradfriDevice device, bool state)
        {
            await SetOutlet(device.ID, state);
            if (HasControl(device))
            {
                device.Control[0].State = state ? Bool.True : Bool.False;
            }
        }

        /// <summary>
        /// Turns a specific control outlet on or off
        /// </summary>
        /// <param name="id">Id of the device</param>
        /// <param name="state">On (True) or Off (false)</param>
        /// <returns></returns>
        public async Task SetOutlet(long id, bool state)
        {
            SwitchStateOutletRequest set = new SwitchStateOutletRequest()
            {
                Options = new[]
                {
                    new SwitchStateLightRequestOption()
                    {
                        IsOn = state ? 1 : 0
                    }
                }
            };
            await HandleRequest($"/{(int)TradfriConstRoot.Devices}/{id}", Call.PUT, content: set);
        }

        /// <summary>
        /// Observes a device and gets update notifications
        /// </summary>
        /// <param name="device">Device on which you want to be notified</param>
        /// <param name="callback">Action to take for each device update</param>
        public void ObserveDevice(TradfriDevice device, Action<TradfriDevice> callback)
        {
            Action<Response> update = (Response response) =>
            {
                if (!string.IsNullOrWhiteSpace(response?.PayloadString))
                {
                    device = JsonConvert.DeserializeObject<TradfriDevice>(response.PayloadString);
                    callback.Invoke(device);
                }
            };
            // this specific combination of parameter values is handled in TradfriController's HandleRequest for Observable
            HandleRequest($"/{(int)TradfriConstRoot.Devices}/{device.ID}", Call.GET, null, null, update, HttpStatusCode.Continue);
        }
    }

    internal class SwitchStateLightRequest
    {
        [JsonProperty("3311")]
        public SwitchStateLightRequestOption[] Options { get; set; }
    }
    internal class SwitchStateLightXYRequest
    {
        [JsonProperty("3311")]
        public SwitchStateLightXYRequestOption[] Options { get; set; }
    }

    internal class SwitchStateOutletRequest
    {
        [JsonProperty("3312")]
        public SwitchStateLightRequestOption[] Options { get; set; }
    }

    internal class SwitchStateRequestOption
    {
        [JsonProperty("5850")]
        public int? IsOn { get; set; }
    }

    internal class SwitchStateLightRequestOption : SwitchStateRequestOption
    {
        [JsonProperty("5851")] //TradfriConstAttr.LightDimmer
        public int? LightIntensity { get; set; }

        [JsonProperty("5706")]
        public string Color { get; set; }

        [JsonProperty("9039")] //TradfriConstAttr.Mood
        public long? Mood { get; set; }
    }

    internal class SwitchStateLightXYRequestOption : SwitchStateRequestOption
    {
        [JsonProperty("5851")] //TradfriConstAttr.LightDimmer
        public int? LightIntensity { get; set; }
        [JsonProperty("5709")]
        public int ColorX { get; set; }
        [JsonProperty("5710")]
        public int ColorY { get; set; }
    }
}
