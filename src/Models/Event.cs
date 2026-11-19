using System;
using System.Collections.Generic;
using System.Linq;
using DynamicCSharp;
using OrbCreationExtensions;
using Tyd;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RandomEvents.EventsHandlerV2;

namespace RandomEvents
{
		public class Event
		{
			public string Title { get; set; }
			public string Description { get; set; }
			public bool IsWorldEvent { get; set; } = false;
			public byte Chance { get; set; }
			public List<EventOption> Options { get; set; }
			public List<VariableCondition> Conditions { get; set; }
			public SDateTime Date { get; set; }
			public object EventObject { get; set; }
			public string EventObjectName { get; set; }
			public EventWindow Window { get; set; }

			public bool CheckConditions()
			{
				if (IsWorldEvent)
				{
					if (TimeOfDay.Instance.GetDate(true).Year != Date.Year || TimeOfDay.Instance.GetDate(true).Month != Date.Month)
					{
						return false;
					}
				}
				foreach (VariableCondition condition in Conditions)
				{
					if (!condition.Check())
						return false;
					if (EventObject == null)
						EventObject = condition.ConditionObject;
				}
				return true;
			}

			public void Fire(bool IgnoreConditions = false)
			{
				if (debugMode)
					DevConsole.Console.Log("Calling event " + Title);
				foreach (EventOption option in Options)
					option.SetParent(this);
				if (!IgnoreConditions)
				{
					if (CheckConditions() && !hasEventRunning)
					{
						EventWindow window = new EventWindow(this);
						window.Show();
					}
					else if (hasEventRunning)
						if(debugMode)
							DevConsole.Console.LogWarning("Won't call event, because an event is already running!");
					else
						if(debugMode)
							DevConsole.Console.LogWarning("Conditions are not met for event!");
				}
				else
				{
					EventWindow window = new EventWindow(this);
					window.Show();
				}
			}
		}
		public class EventOption
		{
			public string Title { get; set; }
			public string Description { get; set; }
			public string ButtonText { get; set; }
			public List<EventEffect> Effects { get; set; }
			public PopupManager.NotificationSound NotificationSound { get; set; } = PopupManager.NotificationSound.Neutral;
			public string Notification { get; set; }
			Event Parent { get; set; }

			public void SetParent(Event parent)
			{
				Parent = parent;
				foreach (EventEffect effect in Effects)
					effect.SetParent(parent);
			}

			public void Select()
			{
				string notificationstring = Notification;
				int count = 0;
				foreach (EventEffect effect in Effects)
				{
					string response = effect.Fire();
					notificationstring = notificationstring.Replace("{NAME}", Parent.EventObjectName);
					notificationstring = notificationstring.Replace("${" + count + "}", float.Parse(response).Currency());
					notificationstring = notificationstring.Replace("{" + count + "}", response);
					count++;
				}
				HUD.Instance.AddPopupMessage(notificationstring, "Info", NotificationSound, 1);
				Parent.Window.Close();
				hasEventRunning = false;
				iscoolingdown = true;
			}
		}
}
