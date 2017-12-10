﻿using Com.AugustCellars.CoAP;
using Newtonsoft.Json;
using Tomidix.CSharpTradFriLibrary.Models;

namespace Tomidix.CSharpTradFriLibrary.Controllers
{
    public class DeviceController
    {
        private readonly CoapClient cc;
        private long id { get; }
        private TradFriDevice device { get; set; }
        public bool HasLight
        {
            get { return device?.LightControl != null; }
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="_id">device id</param>
        /// <param name="_cc">existing coap client</param>
        /// <param name="loadAutomatically">Load device object automatically (default: true)</param>
        public DeviceController(long _id, CoapClient _cc, bool loadAutomatically = true)
        {
            id = _id;
            cc = _cc;
            if (loadAutomatically)
                GetTradFriDevice();
        }

        /// <summary>
        /// Get device information from gateway
        /// </summary>
        /// <returns></returns>
        public Response Get()
        {
            return cc.GetValues(new TradFriRequest { UriPath = $"/{(int)TradFriConstRoot.Devices}/{id}" });
        }
        /// <summary>
        /// Acquires TradFriDevice object
        /// </summary>
        /// <param name="refresh">If set to true, than it will ignore existing cached value and ask the gateway for the object</param>
        /// <returns></returns>
        public TradFriDevice GetTradFriDevice(bool refresh = false)
        {
            if (!refresh && device != null)
                return device;
            device = JsonConvert.DeserializeObject<TradFriDevice>(Get().PayloadString);
            return device;
        }
        /// <summary>
        /// Turn Off Device
        /// </summary>
        /// <returns></returns>
        public Response TurnOff()
        {
            return cc.SetValues(SwitchState(0));
        }
        /// <summary>
        /// Turn On Device
        /// </summary>
        /// <returns></returns>
        public Response TurnOn()
        {
            return cc.SetValues(SwitchState(1));
        }

        /// <summary>
        /// Does not work at the moment
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Response SetColor(string value)
        {
            return cc.SetValues(new TradFriRequest
            {
                UriPath = $"/{(int)TradFriConstRoot.Devices}/{id}",
                Payload = string.Format(@"{{""{0}"":""{1}""}}", (int)TradFriConstAttr.LightColorHex, value)
            });
        }
        /// <summary>
        /// Set Dimmer for Light Device
        /// </summary>
        /// <param name="value">Dimmer intensity (0-255)</param>
        /// <returns></returns>
        public Response SetDimmer(int value)
        {
            return cc.SetValues(new TradFriRequest
            {
                UriPath = $"/{(int)TradFriConstRoot.Devices}/{id}",
                Payload = string.Format(@"{{""{0}"":[{{ ""{1}"":{2}}}]}}", (int)TradFriConstAttr.LightControl, (int)TradFriConstAttr.LightDimmer, value)
            });
        }

        private TradFriRequest SwitchState(int turnOn = 1)
        {
            if (HasLight)
            {
                device.LightControl[0].State = (Bool)turnOn;
            }
            return new TradFriRequest
            {
                UriPath = $"/{(int)TradFriConstRoot.Devices}/{id}",
                Payload = string.Format(@"{{""{0}"":[{{ ""{1}"":{2}}}]}}", (int)TradFriConstAttr.LightControl, (int)TradFriConstAttr.LightState, turnOn)  //@"{ ""3311"":[{ ""5850"":" + turnOn + "}]}"
            };
        }
    }
}
