using System;
using System.Collections.Generic;
using OmniFrame.Common;

namespace Plc
{
    public class PlcMonitor
    {
        private List<PlcDevice> _plcList = new List<PlcDevice>();
        private List<PlcDeviceMonitor> _monitorList = new List<PlcDeviceMonitor>();

        public void AddPlc(PlcDevice plc)
        {
            if (plc != null && !_plcList.Contains(plc))
            {
                _plcList.Add(plc);
                _monitorList.Add(new PlcDeviceMonitor(plc));
            }
        }

        public bool AddMonitorItem(int plcIndex, SoftElement element, int addr)
        {
            var monitor = GetDeviceMonitor(plcIndex);
            if (monitor != null)
            {
                monitor.AddMonitorItem(element, addr);
                return true;
            }
            return false;
        }

        public bool ReadBit(int plcIndex, SoftElement element, int addr)
        {
            var monitor = GetDeviceMonitor(plcIndex);
            if (monitor != null)
            {
                return monitor.ReadBit(element, addr);
            }
            return false;
        }

        public bool ReadWord(int plcIndex, SoftElement element, int addr, ref ushort val)
        {
            var monitor = GetDeviceMonitor(plcIndex);
            if (monitor != null)
            {
                return monitor.ReadWord(element, addr, ref val);
            }
            return false;
        }

        public bool Update()
        {
            bool result = true;
            foreach (var monitor in _monitorList)
            {
                if (!monitor.Update())
                {
                    result = false;
                }
            }
            return result;
        }

        private PlcDeviceMonitor GetDeviceMonitor(int plcIndex)
        {
            foreach (var monitor in _monitorList)
            {
                if (monitor.PlcDevice.Index == plcIndex)
                    return monitor;
            }
            return null;
        }

        public void Clear()
        {
            _monitorList.Clear();
            _plcList.Clear();
        }
    }

    public class PlcDeviceMonitor
    {
        public PlcDevice PlcDevice { get; private set; }

        private Dictionary<SoftElement, PlcElementMonitor> _elementMonitors = new Dictionary<SoftElement, PlcElementMonitor>();

        public PlcDeviceMonitor(PlcDevice plcDevice)
        {
            PlcDevice = plcDevice;
        }

        public void AddMonitorItem(SoftElement element, int addr)
        {
            if (!_elementMonitors.ContainsKey(element))
            {
                _elementMonitors[element] = new PlcElementMonitor(PlcDevice, element);
            }
            _elementMonitors[element].AddAddress(addr);
        }

        public bool ReadBit(SoftElement element, int addr)
        {
            if (_elementMonitors.ContainsKey(element))
            {
                return _elementMonitors[element].ReadBit(addr);
            }
            return false;
        }

        public bool ReadWord(SoftElement element, int addr, ref ushort val)
        {
            if (_elementMonitors.ContainsKey(element))
            {
                return _elementMonitors[element].ReadWord(addr, ref val);
            }
            return false;
        }

        public bool Update()
        {
            bool result = true;
            foreach (var monitor in _elementMonitors.Values)
            {
                if (!monitor.Update())
                {
                    result = false;
                }
            }
            return result;
        }
    }

    public class PlcElementMonitor
    {
        private PlcDevice _plcDevice;
        private SoftElement _element;
        private List<DataBlock> _dataBlocks = new List<DataBlock>();
        private Dictionary<int, int> _addrToBlockMap = new Dictionary<int, int>();

        public PlcElementMonitor(PlcDevice plcDevice, SoftElement element)
        {
            _plcDevice = plcDevice;
            _element = element;
        }

        public void AddAddress(int addr)
        {
            if (_addrToBlockMap.ContainsKey(addr))
                return;

            bool merged = false;
            foreach (var block in _dataBlocks)
            {
                if (block.AllowMerge(addr))
                {
                    block.Merge(addr);
                    merged = true;
                    break;
                }
            }

            if (!merged)
            {
                DataBlock newBlock;
                if (_element == SoftElement.X || _element == SoftElement.Y || _element == SoftElement.M)
                {
                    newBlock = new BitBlock(addr, 1);
                }
                else
                {
                    newBlock = new WordBlock(addr, 1);
                }
                _dataBlocks.Add(newBlock);
            }

            RebuildAddressMap();
        }

        public bool ReadBit(int addr)
        {
            if (_addrToBlockMap.TryGetValue(addr, out int blockIndex))
            {
                return _dataBlocks[blockIndex].ReadBit(addr);
            }
            return false;
        }

        public bool ReadWord(int addr, ref ushort val)
        {
            if (_addrToBlockMap.TryGetValue(addr, out int blockIndex))
            {
                return _dataBlocks[blockIndex].ReadWord(addr, ref val);
            }
            return false;
        }

        public bool Update()
        {
            if (!_plcDevice.IsOpen)
                return false;

            bool result = true;
            foreach (var block in _dataBlocks)
            {
                if (!block.ReadFromPlc(_plcDevice, _element))
                {
                    result = false;
                }
            }
            return result;
        }

        private void RebuildAddressMap()
        {
            _addrToBlockMap.Clear();
            for (int i = 0; i < _dataBlocks.Count; i++)
            {
                var block = _dataBlocks[i];
                for (int addr = block.Start; addr <= block.End; addr++)
                {
                    _addrToBlockMap[addr] = i;
                }
            }
        }
    }
}
