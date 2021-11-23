using System;
using UnityEngine;
using UnityEngine.UI;

namespace RandomEvents
{
    internal class MyModMeta : ModMeta
    {
        Button eventmodechecker;
        Toggle fastforward;

        public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
        {
            Text text = WindowManager.SpawnLabel();
            text.text = "This mod adds random and real world (1970-2020) events to the game.";
            WindowManager.AddElementToElement(text.gameObject, parent.gameObject, new Rect(0f, 0f, 400f, 128f),
                new Rect(0f, 0f, 0f, 0f));

            Text settingsDesc = WindowManager.SpawnLabel();
            settingsDesc.text = "<b>Settings</b>";
            eventmodechecker = WindowManager.SpawnButton();
            eventmodechecker.onClick.AddListener(EventMode_Click);
            eventmodechecker.gameObject.name = "EventModeButton";
            if (EventsHandlerV2.eventMode == 1)
            {               
                eventmodechecker.GetComponentInChildren<Text>().text = "Random Events";
            }
            else if (EventsHandlerV2.eventMode == 2)
            {                
                eventmodechecker.GetComponentInChildren<Text>().text = "World Events";
            }
            else if (EventsHandlerV2.eventMode == 3)
            {             
                eventmodechecker.GetComponentInChildren<Text>().text = "World and Random Events";
            }

            fastforward = WindowManager.SpawnCheckbox();
            fastforward.onValueChanged.AddListener(Fastforward_Click);
            fastforward.isOn = EventsHandlerV2.skipfastforward;
            Text ttxt = fastforward.GetComponentInChildren<Text>();
            ttxt.text = "Skip Fastforward";
            ttxt.resizeTextForBestFit = true;
            ttxt.alignment = TextAnchor.MiddleCenter;

            WindowManager.AddElementToElement(settingsDesc.gameObject, parent.gameObject, new Rect(10f, 70f, 400f, 128f),
                new Rect(0f, 0f, 0f, 0f));
            WindowManager.AddElementToElement(eventmodechecker.gameObject, parent.gameObject, new Rect(10f, 100f, 120f, 40f),
               new Rect(0f, 0f, 0f, 0f));
            WindowManager.AddElementToElement(fastforward.gameObject, parent.gameObject, new Rect(10f, 150f, 160f, 25f),
               new Rect(0f, 0f, 0f, 0f));
            //Add slider to set cooldown time

            instance = this;
        }

        void EventMode_Click()
		{
            if(EventsHandlerV2.Instance == null)
			{
                return;
			}

            if(EventsHandlerV2.eventMode == 1)
			{
                EventsHandlerV2.eventMode = 2;
                eventmodechecker.GetComponentInChildren<Text>().text = "World Events";
            }
            else if(EventsHandlerV2.eventMode == 2)
			{
                EventsHandlerV2.eventMode = 3;
                eventmodechecker.GetComponentInChildren<Text>().text = "World and Random Events";
            }
            else if(EventsHandlerV2.eventMode == 3)
			{
                EventsHandlerV2.eventMode = 1;
                eventmodechecker.GetComponentInChildren<Text>().text = "Random Events";
            }
            EventsHandlerV2.Instance.SaveSetting("EventMode", EventsHandlerV2.eventMode + "");
        }

        void Fastforward_Click(bool check)
		{
            if (EventsHandlerV2.Instance == null)
            {
                return;
            }

            EventsHandlerV2.skipfastforward = check;
            EventsHandlerV2.Instance.SaveSetting("SkipFastForward", EventsHandlerV2.skipfastforward.ToString());
        }

        public override string Name => "Random Events";
        public static MyModMeta instance;
    }
}
