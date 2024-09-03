using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Modules.FSM.Example
{
    public class FSMExample : MonoBehaviour
    {
        public FSM_Controller FSMC;

        public Button NextBtn;
        public Button LastBtn;

        public TMP_Text Display;

        bool NextBtn_Flag;
        bool LastBtn_Flag;

        float Time_Regist;

        void Awake()
        {
            new GameObject("FSM_Manager").AddComponent<FSMManager>();

            FSMC = new FSM_Controller();

            FSM_State stt = FSMC.Root_FSM.AddState("Main",
            enter: (x) =>
            {
                Time_Regist = Time.time;
            },
            update: (x) =>
            {
                if (Time.time - Time_Regist >= 2.0f)
                {
                    Debug.Log("Main FSM is running.");
                    Time_Regist = Time.time;
                }
            });
            FSMC.Root_FSM.SetBeginPath("Main", FSM.ALWAYS);

            FSM fsm = stt.SetAsFSM("Main", true);

            fsm.AddState("Begin",
            enter: (x) =>
            {
                Display.text = "Begin";
                Debug.Log("Enter Begin");
            },
            update: (x) =>
            {
                if (NextBtn_Flag)
                    FSMC.StateTransfer("State1");
                else if (LastBtn_Flag)
                    FSMC.StateTransfer("Begin");
                NextBtn_Flag = false;
                LastBtn_Flag = false;
            },
            exit: (x) =>
            {
                Debug.Log("Exit Begin");
            });

            fsm.AddState("State1",
            enter: (x) =>
            {
                Display.text = "State1";
                Debug.Log("Enter State1");
            },
            update: (x) =>
            {
                if (NextBtn_Flag)
                    FSMC.StateTransfer("State2", Time.time);
                else if (LastBtn_Flag)
                    FSMC.StateTransfer("Begin");
                NextBtn_Flag = false;
                LastBtn_Flag = false;
            },
            exit: (x) =>
            {
                Debug.Log("Exit State1");
            });

            fsm.AddState("State2",
            enter: (x) =>
            {
                Debug.Log("Enter State2");
                NextBtn.interactable = false;
            },
            update: (x) =>
            {
                float delta_t = Time.time - x.GetAttachedData<float>();
                Display.text = "State will transfer automaticallly in " + (5 - (int)delta_t).ToString() + "s";
                if (delta_t >= 5.0f)
                    x.GotoState("Begin");
                if (LastBtn_Flag)
                    x.GotoState("State1");
                LastBtn_Flag = false;
            },
            exit: (x) =>
            {
                Debug.Log("Exit State2");
                NextBtn.interactable = true;
            });

            fsm.SetBeginPath("Begin", FSM.ALWAYS);
            fsm.AddTransferPath("Begin", "Begin", FSM.MANUAL);
            fsm.AddTransferPath("Begin", "State1", FSM.MANUAL);
            fsm.AddTransferPath("State1", "State2", FSM.MANUAL);
            fsm.AddTransferPath("State1", "Begin", FSM.MANUAL);
            fsm.AddTransferPath("State2", "Begin", FSM.MANUAL);
            fsm.AddTransferPath("State2", "State1", FSM.MANUAL);

            NextBtn.onClick.AddListener(() => NextBtn_Flag = true);
            LastBtn.onClick.AddListener(() => LastBtn_Flag = true);
        }

        void Start()
        {
            FSMC.Run();
        }

    }
}
