using UnityEngine;
using Zenject;

namespace Sandbox
{
    public class InputRecorderInstaller : MonoInstaller
    {
        [SerializeField]
        private InputRecorder _inputRecorderPrefab;
        public override void InstallBindings()
        {
            Container.Bind<InputRecorder>().FromComponentInNewPrefab(_inputRecorderPrefab).AsSingle();
        }
    }
}