using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modules.LogSystem;
using Modules.Utility.Singleton;

namespace Modules.FSM
{
#if UNITY_EDITOR
    [AddComponentMenu("Finite State Machine/FSM_Manager")]
#endif
    public class FSMManager : MonoSingleton<FSMManager>, IModule
    {
        public List<FSM_Controller> FSMC { get; private set; } = new List<FSM_Controller>();

        private List<int> _DeletedFSMCIndex = new List<int>();

        public void Prepare()
        {
            LogManager.Info("FSM.FSMManager", "FSM已经初始化");
        }

        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(EndOfFrameUpdate());
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < FSMC.Count; i++)
            {
                if (FSMC[i] == null)
                {
                    //_DeletedFSMCIndex.Add(i);
                    FSMC.RemoveAt(i--);
                    continue;
                }
                if (!FSMC[i].ManuallyExecute)
                    FSMC[i].FixedUpdate();
            }

            /*if (_DeletedFSMCIndex.Count != 0)
            {
                foreach (int idx in _DeletedFSMCIndex)
                    FSMC.RemoveAt(idx);
                _DeletedFSMCIndex.Clear();
            }*/
        }

        private void Update()
        {
            for (int i = 0; i < FSMC.Count; i++)
            {
                if (FSMC[i] == null)
                {
                    //_DeletedFSMCIndex.Add(i);
                    FSMC.RemoveAt(i--);
                    continue;
                }
                if (!FSMC[i].ManuallyExecute)
                    FSMC[i].Update();
            }

            /*if (_DeletedFSMCIndex.Count != 0)
            {
                foreach (int idx in _DeletedFSMCIndex)
                    FSMC.RemoveAt(idx);
                _DeletedFSMCIndex.Clear();
            }*/
        }

        private void LateUpdate()
        {
            for (int i = 0; i < FSMC.Count; i++)
            {
                if (FSMC[i] == null)
                {
                    //_DeletedFSMCIndex.Add(i);
                    FSMC.RemoveAt(i--);
                    continue;
                }
                if (!FSMC[i].ManuallyExecute)
                    FSMC[i].LateUpdate();
            }

            /*if (_DeletedFSMCIndex.Count != 0)
            {
                foreach (int idx in _DeletedFSMCIndex)
                    FSMC.RemoveAt(idx);
                _DeletedFSMCIndex.Clear();
            }*/
        }

        private void OnGUI()
        {
            for (int i = 0; i < FSMC.Count; i++)
            {
                if (FSMC[i] == null)
                {
                    //_DeletedFSMCIndex.Add(i);
                    FSMC.RemoveAt(i--);
                    continue;
                }
                if (!FSMC[i].ManuallyExecute)
                    FSMC[i].OnGUI();
            }

            /*if (_DeletedFSMCIndex.Count != 0)
            {
                foreach (int idx in _DeletedFSMCIndex)
                    FSMC.RemoveAt(idx);
                _DeletedFSMCIndex.Clear();
            }*/
        }

        public IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                for (int i = 0; i < FSMC.Count; i++)
                {
                    if (FSMC[i] == null)
                    {
                        //_DeletedFSMCIndex.Add(i);
                        FSMC.RemoveAt(i--);
                        continue;
                    }
                    //if (!FSMC[i].ManuallyExecute)
                    FSMC[i].FrameEnd();
                }

                /*if (_DeletedFSMCIndex.Count != 0)
                {
                    foreach (int idx in _DeletedFSMCIndex)
                        FSMC.RemoveAt(idx);
                    _DeletedFSMCIndex.Clear();
                }*/
            }
        }

        public void RegistFSMC(FSM_Controller fsmc)
        {
            if (!FSMC.Contains(fsmc))
                FSMC.Add(fsmc);
        }

        public void UnRegistFSMC(FSM_Controller fsmc)
        {
            int idx = FSMC.IndexOf(fsmc);
            FSMC[idx] = null;
        }

        public FSM_Controller NewFSMC()
        {
            FSM_Controller fsmc = new FSM_Controller();
            RegistFSMC(fsmc);
            fsmc.ManuallyExecute = false;

            return fsmc;
        }
    }
}
