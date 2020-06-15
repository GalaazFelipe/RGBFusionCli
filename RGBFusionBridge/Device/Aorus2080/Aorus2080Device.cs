﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Gigabyte.ULightingEffects.Win32;

namespace RGBFusionBridge.Device.Aorus2080
{
    public class Aorus2080Device : Device
    {
        [SuppressUnmanagedCodeSecurity()]
        [DllImport("GvLedLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_GvLedSet", ExactSpelling = true)]
        private static extern uint GvLedSet(int nIndex, GVLED_CFG_V1 config);

        private int _VGARGBNewColor;
        private GVLED_CFG_V1 _curSetting = new GVLED_CFG_V1(1, 0, 0, 0, 10, 16711680);
        private bool _changingColor = false;

        public override void Init()
        {
            _deviceType = DeviceType.Aorus2080;
            _curSetting.nSync = -1;
            _ledIndexes.Add(0);
            base.Init();
        }
        public override void Apply()
        {
            if (!_changingColor)
            {
                _changingColor = true;
                SendColorToVGA();
                base.Apply();
                _changingColor = false;
            }
        }

        private void SendColorToVGA()
        {
            _VGARGBNewColor = ((_newLedData[0] & 0x0ff) << 16) | ((_newLedData[1] & 0x0ff) << 8) | (_newLedData[2] & 0x0ff);
            _curSetting.dwColor = (uint)_VGARGBNewColor & 16777215;
            _ = GvLedSet(4097, _curSetting);
            Thread.Sleep(50);
        }
        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

        protected override void ConfirmApply()
        {
            SendColorToVGA();
        }
    }
}