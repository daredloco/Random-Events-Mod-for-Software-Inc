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
		public class EventWindow
		{
			private string _title = "Event";
			private bool _shown;
			private GUIWindow window;
			private Event windowEvent;

			public EventWindow(Event windowevent)
			{
				windowEvent = windowevent;
				windowEvent.Window = this;
			}

			public void Show()
			{

				//Add to the newspaper if its worldevent and don't show popup
				if (windowEvent.IsWorldEvent)
				{
					DevConsole.Console.Log($"Write event '{ windowEvent.Title }' to newspaper");
					Utils.WriteNewspaper(windowEvent.Title, windowEvent.Description);
					windowEvent.Options[0].Select();
					return;
				}

				if (_shown)
				{
					window.Close();
					_shown = false;
					return;
				}
				_title = "Event - " + windowEvent.Title;
				Init();
				_shown = true;
				GameSettings.ForcePause = true;
			}

			public void Close()
			{
				if (_shown)
				{
					window.Close();
					_shown = false;
					hasEventRunning = false;
					GameSettings.ForcePause = false;
				}
			}

			void Init()
			{
				if (debugMode)
					DevConsole.Console.Log("Initialize Events window");

				window = WindowManager.SpawnWindow();
				window.InitialTitle = window.TitleText.text = window.NonLocTitle = _title;
				window.name = "EventWindow";
				window.MainPanel.name = "EventPanel";
				window.ShowCentered = true;

				//Remove default Window Buttons
				if (window.name == "EventWindow")
				{
					UnityEngine.Object.Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CloseButton").gameObject);
					UnityEngine.Object.Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CollapseButton").gameObject);
					UnityEngine.Object.Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "ResizeButton").gameObject);
				}

				string descriptionName = "Unknown";

				//Add the {NAME} placeholders if possible
				Actor a = windowEvent.EventObject as Actor;
				SoftwareProduct p = windowEvent.EventObject as SoftwareProduct;
				SoftwareAlpha sa = windowEvent.EventObject as SoftwareAlpha;
				SoftwareWorkItem swi = windowEvent.EventObject as SoftwareWorkItem;
				DesignDocument dd = windowEvent.EventObject as DesignDocument;
				SupportWork sw = windowEvent.EventObject as SupportWork;
				StockMarket s = windowEvent.EventObject as StockMarket;
				if (a != null)
					descriptionName = a.employee.FullName;
				if (p != null)
					descriptionName = p.Name;
				if (s != null)
					descriptionName = s.Name;
				if (sa != null)
					descriptionName = sa.Name;
				if (swi != null)
					descriptionName = swi.Name;
				if (dd != null)
					descriptionName = dd.Name;
				if (sw != null)
					descriptionName = sw.Name;

				//Generate values for all effect options
				List<string> effectvalues = new List<string>();
				foreach(EventOption option in windowEvent.Options)
				{
					foreach (EventEffect effect in option.Effects)
					{
						effect.GenerateValue();
						effectvalues.Add(effect.EffectValue);
					}					
				}
				windowEvent.EventObjectName = descriptionName;
				string eventdescription = windowEvent.Description.Replace("{NAME}", descriptionName); //CHANGES NAME TO THE description name
				for(int i = 0; i < effectvalues.Count; i++)
				{
					string key = "{" + i + "}";
					eventdescription = eventdescription.Replace(key, effectvalues[i]);
				}

				//Add Event description
				Helpers.AddLabel($"{windowEvent.Title}", new Rect(10, 10, 100, 15), window, "EventTitleLabel", true);
				Helpers.AddLabel($"{eventdescription}", new Rect(10, 30, 300, 100), window, "EventDescLabel");

				//Add Buttons
				switch (windowEvent.Options.Count)
				{
					case 1:
						#region effects
						string bt1effects = "";
						foreach (EventEffect effect in windowEvent.Options[0].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						if (bt1effects == "")
							bt1effects = "Unknown Effects";
						#endregion

						Button bt1 = Helpers.AddButton($"{windowEvent.Options[0].ButtonText}", new Rect(120, 145, 120, 30), new UnityAction(windowEvent.Options[0].Select), window);

						//Replace {NAME} with name of object
						string option1description = windowEvent.Options[0].Description.Replace("{NAME}", descriptionName);
						for (int i = 0; i < effectvalues.Count; i++)
						{
							string key = "{" + i + "}";
							option1description = option1description.Replace(key, effectvalues[i]);
						}

						//Add the tooltips
						Helpers.AddTooltip($"{windowEvent.Options[0].Title}", $"{option1description}\n\n<b>Effects:</b>\n{bt1effects}", bt1.gameObject);

						window.MinSize = new Vector2(200, 40);
						break;
					case 2:
						#region effects
						bt1effects = "";
						string bt2effects = "";
						foreach (EventEffect effect in windowEvent.Options[0].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt2effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						if (bt1effects == "")
							bt1effects = "Unknown Effects";
						if (bt2effects == "")
							bt2effects = "Unknown Effects";
						#endregion

						bt1 = Helpers.AddButton($"{windowEvent.Options[0].ButtonText}", new Rect(62, 145, 120, 30), new UnityAction(windowEvent.Options[0].Select), window);
						Button bt2 = Helpers.AddButton($"{windowEvent.Options[1].ButtonText}", new Rect(188, 145, 120, 30), new UnityAction(windowEvent.Options[1].Select), window);

						//Replace {NAME} with name of object
						option1description = windowEvent.Options[0].Description.Replace("{NAME}", descriptionName);
						string option2description = windowEvent.Options[1].Description.Replace("{NAME}", descriptionName);
						for (int i = 0; i < effectvalues.Count; i++)
						{
							string key = "{" + i + "}";
							option1description = option1description.Replace(key, effectvalues[i]);
							option2description = option2description.Replace(key, effectvalues[i]);
						}
						//Add the tooltips
						Helpers.AddTooltip($"{windowEvent.Options[0].Title}", $"{option1description}\n\n<b>Effects:</b>\n{bt1effects}", bt1.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[1].Title}", $"{option2description}\n\n<b>Effects:</b>\n{bt2effects}", bt2.gameObject);

						window.MinSize = new Vector2(200, 40);
						break;
					case 3:
						#region effects
						bt1effects = "";
						bt2effects = "";
						string bt3effects = "";
						foreach (EventEffect effect in windowEvent.Options[0].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt2effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[2].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt3effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						if (bt1effects == "")
							bt1effects = "Unknown Effects";
						if (bt2effects == "")
							bt2effects = "Unknown Effects";
						if (bt3effects == "")
							bt3effects = "Unknown Effects";
						#endregion

						bt1 = Helpers.AddButton($"{windowEvent.Options[0].ButtonText}", new Rect(62, 145, 120, 30), new UnityAction(windowEvent.Options[0].Select), window);
						bt2 = Helpers.AddButton($"{windowEvent.Options[1].ButtonText}", new Rect(188, 145, 120, 30), new UnityAction(windowEvent.Options[1].Select), window);
						Button bt3 = Helpers.AddButton($"{windowEvent.Options[2].ButtonText}", new Rect(62, 180, 120, 30), new UnityAction(windowEvent.Options[2].Select), window);

						//Replace {NAME} with name of object
						option1description = windowEvent.Options[0].Description.Replace("{NAME}", descriptionName);
						option2description = windowEvent.Options[1].Description.Replace("{NAME}", descriptionName);
						string option3description = windowEvent.Options[2].Description.Replace("{NAME}", descriptionName);
						for (int i = 0; i < effectvalues.Count; i++)
						{
							string key = "{" + i + "}";
							option1description = option1description.Replace(key, effectvalues[i]);
							option2description = option2description.Replace(key, effectvalues[i]);
							option3description = option3description.Replace(key, effectvalues[i]);
						}
						//Add the tooltips
						Helpers.AddTooltip($"{windowEvent.Options[0].Title}", $"{option1description}\n\n<b>Effects:</b>\n{bt1effects}", bt1.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[1].Title}", $"{option2description}\n\n<b>Effects:</b>\n{bt2effects}", bt2.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[2].Title}", $"{option3description}\n\n<b>Effects:</b>\n{bt3effects}", bt3.gameObject);

						window.MinSize = new Vector2(200, 40);
						break;
					case 4:
						#region effects
						bt1effects = "";
						bt2effects = "";
						bt3effects = "";
						string bt4effects = "";
						foreach (EventEffect effect in windowEvent.Options[0].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt2effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[2].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt3effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[3].Effects)
						{
							//effect.GenerateValue();
							if (effect.IsVisible)
								bt4effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						if (bt1effects == "")
							bt1effects = "Unknown Effects";
						if (bt2effects == "")
							bt2effects = "Unknown Effects";
						if (bt3effects == "")
							bt3effects = "Unknown Effects";
						if (bt4effects == "")
							bt4effects = "Unknown Effects";
						#endregion

						bt1 = Helpers.AddButton($"{windowEvent.Options[0].ButtonText}", new Rect(62, 145, 120, 30), new UnityAction(windowEvent.Options[0].Select), window);
						bt2 = Helpers.AddButton($"{windowEvent.Options[1].ButtonText}", new Rect(188, 145, 120, 30), new UnityAction(windowEvent.Options[1].Select), window);
						bt3 = Helpers.AddButton($"{windowEvent.Options[2].ButtonText}", new Rect(62, 180, 120, 30), new UnityAction(windowEvent.Options[2].Select), window);
						Button bt4 = Helpers.AddButton($"{windowEvent.Options[3].ButtonText}", new Rect(188, 180, 120, 30), new UnityAction(windowEvent.Options[3].Select), window);

						//Replace {NAME} with name of object
						option1description = windowEvent.Options[0].Description.Replace("{NAME}", descriptionName);
						option2description = windowEvent.Options[1].Description.Replace("{NAME}", descriptionName);
						option3description = windowEvent.Options[2].Description.Replace("{NAME}", descriptionName);
						string option4description = windowEvent.Options[3].Description.Replace("{NAME}", descriptionName);
						for (int i = 0; i < effectvalues.Count; i++)
						{
							string key = "{" + i + "}";
							option1description = option1description.Replace(key, effectvalues[i]);
							option2description = option2description.Replace(key, effectvalues[i]);
							option3description = option3description.Replace(key, effectvalues[i]);
							option4description = option4description.Replace(key, effectvalues[i]);
						}
						//Add the tooltips
						Helpers.AddTooltip($"{windowEvent.Options[0].Title}", $"{option1description}\n\n<b>Effects:</b>\n{bt1effects}", bt1.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[1].Title}", $"{option2description}\n\n<b>Effects:</b>\n{bt2effects}", bt2.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[2].Title}", $"{option3description}\n\n<b>Effects:</b>\n{bt3effects}", bt3.gameObject);
						Helpers.AddTooltip($"{windowEvent.Options[3].Title}", $"{option4description}\n\n<b>Effects:</b>\n{bt4effects}", bt4.gameObject);

						window.MinSize = new Vector2(200, 40);
						break;
						
				}
			}
		}
}
