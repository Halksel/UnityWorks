﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Zenject;

namespace Sandbox
{
    using Record = InputRecorder.InputRecord;
    /// <summary>
    /// 入力管理
    /// レコード読み込みで入力再現も可能
    /// </summary>
    public class InputManager : MonoBehaviour, DebugInput.IDebugActions
    {

        public enum InputType
        {
            Debug,
            Basis,
            UI,
        }

        struct InputProxy
        {
            public InputProxy(IInputActionCollection input, int priority)
            {
                this.input = input;
                this.priority = priority;
                this.input.Enable();
            }
            public IInputActionCollection input;
            public int priority;
        }

        private IInputActionCollection constructInput(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Debug:
                    return new DebugInput();
                case InputType.Basis:
                    return new BasisInput();
                case InputType.UI:
                    return new UIInput();
                default:
                    return null;
            }
        }

        public bool Setup()
        {
            var input = new DebugInput();
            input.Enable();
            input.Debug.SetCallbacks(this);
            _inputProxies.Add(new InputProxy(input, -1));
            return true;
        }

        public IInputActionCollection CreateCurrentPriorityProxy(InputType inputType)
        {
            var input = constructInput(inputType);
            if (inputType == InputType.Debug)
            {
                _inputProxies.Add(new InputProxy(input, -1));
            }
            else
            {
                _inputProxies.Add(new InputProxy(input, _currentInputPriority));
            }
            switchInputPriority();
            return input;

        }

        public IInputActionCollection CreateTopPriorityProxy(InputType inputType)
        {
            ++_currentInputPriority;
            return CreateCurrentPriorityProxy(inputType);
        }

        public void DeleteInputProxy(IInputActionCollection input)
        {
            var proxy = _inputProxies.Where(p => p.input == input).Single();
            proxy.input.Disable();
            _inputProxies.Remove(proxy);
            _currentInputPriority = _inputProxies.OrderBy(p => p.priority).Last().priority;
            switchInputPriority();
        }

        private void switchInputPriority()
        {
            foreach (var proxy in _inputProxies)
            {
                if (proxy.priority == -1 || proxy.priority == _currentInputPriority)
                {
                    proxy.input.Enable();
                }
                else
                {
                    proxy.input.Disable();
                }
            }
        }

        IEnumerator EmurateInput()
        {
            int idx = 0;
            _records = inputRecorder.GetValidRecords();
            SceneManager.LoadScene("_scn_debug");
            while (true)
            {
                _time += Time.deltaTime;
                while (idx < _records.Count() && _time > _records[idx].time)
                {
                    if (!_records[idx].valid)
                    {
                        ++idx;
                        continue;
                    }
                    Debug.Log(_records[idx].time);
                    var record = _records[idx];
                    // 入力レコードによって値をセット
                    var device = InputSystem.GetDeviceById(record.deviceId);
                    using (StateEvent.From(device, out var eventPtr))
                    {
                        object value = null;
                        try
                        {
                            value = record.value;
                            if (value == null)
                            {
                                throw new ArgumentException("");
                            }

                            _time = record.time;
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"{e}");
                            ++idx;
                            break;
                        }
                        device.allControls[record.index].WriteValueFromObjectIntoEvent(eventPtr, value);
                        InputSystem.QueueEvent(eventPtr);
                    }
                    ++idx;
                }
                InputSystem.Update();

                if (idx >= _records.Count())
                {
                    _isRecord = false;
                    Debug.Log("End of Emulate");
                    inputRecorder.IsRecord = true;
                    StopCoroutine(EmurateInput());
                    break;
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void PlayRecord()
        {
            if (_isRecord) return;
            inputRecorder.IsRecord = false;
            _time = 0;
            _isRecord = true;
            StartCoroutine(EmurateInput());
        }


        public void OnTest1(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Disabled:
                    {

                    }
                    break;
                case InputActionPhase.Waiting:
                    {

                    }
                    break;
                case InputActionPhase.Started:
                    {
                        PlayRecord();
                    }
                    break;
                case InputActionPhase.Performed:
                    {

                    }
                    break;
                case InputActionPhase.Canceled:
                    {

                    }
                    break;
                default:
                    break;
            }
        }

        public void OnTest2(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Disabled:
                    {

                    }
                    break;
                case InputActionPhase.Waiting:
                    {

                    }
                    break;
                case InputActionPhase.Started:
                    {

                    }
                    break;
                case InputActionPhase.Performed:
                    {

                    }
                    break;
                case InputActionPhase.Canceled:
                    {

                    }
                    break;
                default:
                    break;
            }
            Debug.Log($"{context.phase} : {context.ReadValueAsObject()}");
        }

        public void OnTest3(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Disabled:
                    {

                    }
                    break;
                case InputActionPhase.Waiting:
                    {

                    }
                    break;
                case InputActionPhase.Started:
                    {

                    }
                    break;
                case InputActionPhase.Performed:
                    {

                    }
                    break;
                case InputActionPhase.Canceled:
                    {

                    }
                    break;
                default:
                    break;
            }
            Debug.Log($"{context.phase} : {context.ReadValueAsObject()}");
        }
        public void OnSaveInput(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    {
                        //InputRecorder.Instance.SaveInputRecord();
                    }
                    break;
            }
        }

        public void OnLoadInput(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    {
                        //InputRecorder.Instance.LoadInputRecord();
                        //PlayRecord();
                    }
                    break;
            }
        }

        [Inject]
        private InputRecorder inputRecorder;

        [SerializeField]
        private static int _currentInputPriority;

        private static List<InputProxy> _inputProxies = new List<InputProxy>();
        private static bool _isRecord = false;
        private static double _time;
        private static List<Record> _records;
    }
}
