using DynamicCSharp;
using MadGoat_SSAA;
using OrbCreationExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tyd;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RandomEvents
{
	public class EventsHandlerV2 : ModBehaviour
	{
		public static EventsHandlerV2 Instance;
		List<Event> RandomEvents = new List<Event>();
		List<Event> WorldEvents = new List<Event>();

		public static bool hasEventRunning = false;
		public static bool debugMode = false;
		public static bool skipfastforward = true;
		public static byte eventMode = 1; //1=only random, 2=only real, 3=both
		public static bool ignoreWorldChance = true; //Should the chance in world events be ignored?
		public static uint cooldown = 48; //Cooldown in hours
		static bool iscoolingdown = false;
		int cooldownCounter = 0;

		static Actor founder;

		void AutoEvent(object sender, EventArgs args)
		{
			if (eventMode == 2)
				return;

			if (skipfastforward)
				if (TimeOfDay.Instance.IsSkipping)
					return;

			if (hasEventRunning)
				return;

			Utils.ShuffleList(RandomEvents);
			if (debugMode)
				DevConsole.Console.Log("Running AutoEvent");
			if (RandomEvents.Count < 1)
				return;

			if (iscoolingdown)
			{
				cooldownCounter++;
				if (debugMode)
					DevConsole.Console.Log($"Is cooling down for {cooldown - cooldownCounter} hours...");
				if (cooldownCounter >= cooldown)
				{
					cooldownCounter = 0;
					iscoolingdown = false;
				}
				else
				{
					return;
				}
			}
			int eventindex = UnityEngine.Random.Range(0, RandomEvents.Count - 1);
			int rnd = SafeRandom.Rnd.Next(1, 101);
			if (debugMode)
				DevConsole.Console.Log($"AutoEvent has {rnd} and chance is {RandomEvents[eventindex].Chance}");
			if (rnd < RandomEvents[eventindex].Chance)
			{
				RandomEvents[eventindex].Fire();
			}
		}

		void AutoWorldEvent(object sender, EventArgs args)
		{
			if (eventMode == 1)
				return;
			List<Event> events = EventsOfTheMonth();
			if (events.Count < 1)
				return;
			if(ignoreWorldChance)
				events[UnityEngine.Random.Range(0, events.Count - 1)].Fire();
			else
			{
				int rnd = SafeRandom.Rnd.Next(1, 101);
				Event eve = events[UnityEngine.Random.Range(0, events.Count - 1)];
				if (rnd < eve.Chance)
					eve.Fire();
			}
		}

		List<Event> EventsOfTheMonth()
		{
			int year = TimeOfDay.Instance.GetDate(true).Year;
			int month = TimeOfDay.Instance.GetDate(true).Month;
			return WorldEvents.FindAll(x => x.Date.Year == year && x.Date.Month == month);
		}

		public void Start()
		{
			#region Settings
			skipfastforward = LoadSetting("SkipFastForward", true);
			eventMode = LoadSetting<byte>("EventMode", 1);
			cooldown = LoadSetting<uint>("Cooldown", 24);
			#endregion
		}

		public override void OnActivate()
		{
			Instance = this;
			RandomEvents.Clear();
			WorldEvents.Clear();
			RandomEvents = LoadTyd("random_events");
			WorldEvents = LoadTyd("world_events");

			#region Console
			DevConsole.Command<int> cmd = new DevConsole.Command<int>("CALL_RANDOM_EVENT", ConsoleCallRandom, "CALL_RANDOM_EVENT {event index}");
			DevConsole.Console.AddCommand(cmd);
			DevConsole.Command lstcmd = new DevConsole.Command("RANDOM_EVENT_LIST", ConsoleRandomList, "RANDOM_EVENT_LIST");
			DevConsole.Console.AddCommand(lstcmd);
			DevConsole.Command<int> cmdworld = new DevConsole.Command<int>("CALL_WORLD_EVENT", ConsoleCallWorld, "CALL_WORLD_EVENT {event index}");
			DevConsole.Console.AddCommand(cmdworld);
			DevConsole.Command lstcmdworld = new DevConsole.Command("WORLD_EVENT_LIST", ConsoleWorldList, "WORLD_EVENT_LIST");
			DevConsole.Console.AddCommand(lstcmdworld);
			DevConsole.Command<bool> debugcmd = new DevConsole.Command<bool>("EVENT_DEBUG", ConsoleDebug, "EVENT_DEBUG Y/N");
			DevConsole.Console.AddCommand(debugcmd);
			#endregion

			//Set the autoeventhandlers
			TimeOfDay.OnHourPassed += AutoEvent;
			TimeOfDay.OnMonthPassed += AutoWorldEvent;
		}

		public override void OnDeactivate()
		{
			RandomEvents.Clear();
			WorldEvents.Clear();

			DevConsole.Console.RemoveCommand("EVENT_DEBUG");
			DevConsole.Console.RemoveCommand("CALL_RANDOM_EVENT");
			DevConsole.Console.RemoveCommand("RANDOM_EVENT_LIST");
			DevConsole.Console.RemoveCommand("CALL_WORLD_EVENT");
			DevConsole.Console.RemoveCommand("WORLD_EVENT_LIST");

			//Unlink autoeventhandlers
			TimeOfDay.OnHourPassed -= AutoEvent;
			TimeOfDay.OnMonthPassed -= AutoWorldEvent;
		}

		#region console commands
		void ConsoleCallRandom(int id)
		{
			DevConsole.Console.Log("Calling random event " + id);
			RandomEvents[id].Fire();
		}

		void ConsoleRandomList()
		{
			DevConsole.Console.Log("Random Events: ");
			int count = 0;
			foreach (Event e in RandomEvents)
			{
				DevConsole.Console.Log(e.Title + " " + count);
				count++;
			}
		}

		void ConsoleCallWorld(int id)
		{
			DevConsole.Console.Log("Calling world event " + id);
			WorldEvents[id].Fire(true);
		}

		void ConsoleWorldList()
		{
			DevConsole.Console.Log("World Events: ");
			int count = 0;
			foreach (Event e in WorldEvents)
			{
				DevConsole.Console.Log(e.Title + " " + count);
				count++;
			}
		}

		void ConsoleDebug(bool enable)
		{
			DevConsole.Console.Log("Debug Mode now is " + enable);
			debugMode = enable;
		}
		#endregion

		public List<Event> LoadTyd(string fname)
		{
			List<Event> events = new List<Event>();

			TydFile eventsfile = null;
			try
			{
				eventsfile = TydFile.FromFile("./DLLMods/RandomEvents/" + fname + ".tyd");
			}
			catch(Exception ex)
			{
				if (debugMode)
					DevConsole.Console.LogWarning("Coudln't find file inside the local folder, trying the steam folder! => " + ex.Message);
				
				try
				{
					eventsfile = TydFile.FromFile("../../workshop/content/362620/2139500907/" + fname + ".tyd");
				}
				catch(Exception ex2)
				{
					DevConsole.Console.LogWarning(ex2.Message);
				}
			}
			
			foreach(TydNode n in eventsfile.DocumentNode)
			{
				TydDocument doc = new TydDocument(new TydNode[1] { n });
				for (int i = 0; i < doc.Count; i++)
				{
					TydCollection tydCollection = doc[i] as TydCollection;
					if (tydCollection != null)
					{
						string title = tydCollection.GetChild("Title").GetNodeValues().First();
						string desc = tydCollection.GetChild("Description").GetNodeValues().First();
						SDateTime date = new SDateTime(0, 0);
						bool isworldevent = false;
						if(fname == "world_events")
						{
							string datestr = tydCollection.GetChild("Date").GetNodeValues().First();
							date = new SDateTime(int.Parse(datestr.Split('.')[0]), int.Parse(datestr.Split('.')[1]));
							isworldevent = true;
						}
						int chance = tydCollection.GetChild("Chance").GetNodeValues().First().MakeInt();
						List<VariableCondition> conditions = new List<VariableCondition>();
						foreach(string condition in tydCollection.GetChild("Conditions").GetNodeValues())
						{
							conditions.Add(new VariableCondition(condition));
						}

						#region Options
						TydCollection optionsCollection = tydCollection.GetChild("Options") as TydCollection;
						List<EventOption> options = new List<EventOption>();
						if(optionsCollection != null)
						{
							foreach(TydCollection option in optionsCollection)
							{
								string optionbt = option.GetChild("ButtonText").GetNodeValues().First();						
								string optiontitle = option.GetChild("Title").GetNodeValues().First();
								string optiondesc = option.GetChild("Description").GetNodeValues().First();
								string optionnotification = option.GetChild("Notification").GetNodeValues().First();
								string optionnotificationsound = option.GetChild("NotificationSound").GetNodeValues().First();
								PopupManager.NotificationSound optionsound = PopupManager.NotificationSound.Neutral;
								if (optionnotificationsound == "Good" || optionnotificationsound == "good")
								{
									optionsound = PopupManager.NotificationSound.Good;
								}
								else if(optionnotificationsound == "Issue" || optionnotificationsound == "issue")
								{
									optionsound = PopupManager.NotificationSound.Issue;
								}
								else if(optionnotificationsound == "Warning" || optionnotificationsound == "warning")
								{
									optionsound = PopupManager.NotificationSound.Warning;
								}

								#region Effects
								List<EventEffect> optioneffects = new List<EventEffect>();
								TydCollection effectscollection = option.GetChild("Effects") as TydCollection;
								if(effectscollection != null)
								{
									foreach (TydCollection effect in effectscollection)
									{
										bool effectvisible = effect.GetChild("IsVisible").GetNodeValues().First().MakeBool();
										var effectvalnode = effect.GetChild("EffectValue");
										string effectval = "0";
										if (effectvalnode != null)
										{
											effectval = effectvalnode.GetNodeValues().First();
										}
										var effectminnode = effect.GetChild("EffectMinValue");
										float effectmin = 0;
										if (effectminnode != null)
										{
											effectmin = effectminnode.GetNodeValues().First().MakeFloat();
										}
										var effectmaxnode = effect.GetChild("EffectMaxValue");
										float effectmax = 0;
										if (effectmaxnode != null)
										{
											effectmax = effectmaxnode.GetNodeValues().First().MakeFloat();
										}
										string effectname = effect.GetChild("EffectName").GetNodeValues().First();
										string effectstr = effect.GetChild("Effect").GetNodeValues().First();

										optioneffects.Add(new EventEffect()
										{
											IsVisible = effectvisible,
											EffectValue = effectval,
											EffectMinValue = effectmin,
											EffectMaxValue = effectmax,
											EffectName = effectname,
											Effect = effectstr
										});
									}
								}
								#endregion

								options.Add(new EventOption()
								{
									ButtonText = optionbt,
									Description = optiondesc,
									Notification = optionnotification,
									Title = optiontitle,
									NotificationSound = optionsound,
									Effects = optioneffects
								});
							}}
						#endregion

						events.Add(new Event() {
							Title = title,
							Description = desc,
							Chance = (byte)chance,
							Conditions = conditions,
							Options = options,
							Date = date,
							IsWorldEvent = isworldevent
						});
					}
				}
			}
			return events;
		}

		#region Window
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
					Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CloseButton").gameObject);
					Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CollapseButton").gameObject);
					Destroy(window.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "ResizeButton").gameObject);
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
							effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						if (bt1effects == "")
							bt1effects = "Unknown Effects";
						#endregion

						Button bt1 = Helpers.AddButton($"{windowEvent.Options[0].ButtonText}", new Rect(120, 145, 120, 30), new UnityAction(windowEvent.Options[0].Select), window);

						//Replace {NAME} with name of object
						string option1description = windowEvent.Options[0].Description.Replace("{NAME}", descriptionName);

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
							effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							effect.GenerateValue();
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
							effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							effect.GenerateValue();
							if (effect.IsVisible)
								bt2effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[2].Effects)
						{
							effect.GenerateValue();
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
							effect.GenerateValue();
							if (effect.IsVisible)
								bt1effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[1].Effects)
						{
							effect.GenerateValue();
							if (effect.IsVisible)
								bt2effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[2].Effects)
						{
							effect.GenerateValue();
							if (effect.IsVisible)
								bt3effects += Utils.UpperFirstLetters(effect.EffectName) + ": " + effect.EffectValue + "\n";
						}
						foreach (EventEffect effect in windowEvent.Options[3].Effects)
						{
							effect.GenerateValue();
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
		#endregion

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

		public class EventEffect
		{
			public string Effect { get; set; }
			public string EffectName { get; set; }
			public float EffectMinValue { get; set; }
			public float EffectMaxValue { get; set; }
			public string EffectValue { get; set; }
			public bool IsVisible { get; set; }
			Event Parent { get; set; }

			public void SetParent(Event parent)
			{
				Parent = parent;
			}

			public void GenerateValue()
			{
				if (EffectMaxValue != 0)
				{
					EffectValue = "" + (int)UnityEngine.Random.Range(EffectMinValue, EffectMaxValue);
				}
			}

			public string Fire()
			{
				object obj = Parent.EventObject;
				SoftwareProduct product = obj as SoftwareProduct;
				SoftwareAlpha alpha = obj as SoftwareAlpha;
				SoftwareWorkItem sworkitem = obj as SoftwareWorkItem;
				DesignDocument designDocument = obj as DesignDocument;
				SupportWork supportWork = obj as SupportWork;
				Actor actor = obj as Actor;
				SoftwareAddOn addon = obj as SoftwareAddOn;
				//Employee employee = obj as Employee; //handled with actor
				StockMarket marketobj = obj as StockMarket;

				if (Effect == "company_money")
				{
					GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(EffectValue), Company.TransactionCategory.NA);
				}
				else if (Effect == "company_bill")
				{
					//Add/Remove money from the company account in category bills
					GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(EffectValue), Company.TransactionCategory.Bills);
				}
				else if (Effect == "company_deals")
				{
					//Add/Remove money from the company account in category deals
					GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(EffectValue), Company.TransactionCategory.Deals);
				}
				else if (Effect == "software_offsales")
				{
					//Adds offline sales to a software
					float price = product.Price;
					product.AddToCashflow(0, int.Parse(EffectValue), 0, int.Parse(EffectValue) * price, 0, TimeOfDay.Instance.GetDate());
				}
				else if (Effect == "software_onsales")
				{
					//Adds online sales to a software
					float price = product.Price;
					product.AddToCashflow(int.Parse(EffectValue), 0, 0, int.Parse(EffectValue) * price, 0, TimeOfDay.Instance.GetDate());
				}
				else if (Effect == "software_refunds")
				{
					//Adds refunds to the sales
					int effectVal = int.Parse(EffectValue);
					int totalsales = 0;
					for(int i = 0; i < product.GetUnitSales(false).Count; i++)
					{
						totalsales += product.GetUnitSales(false)[i];
					}
					if(totalsales < effectVal)
					{
						effectVal = totalsales;
					}
					float refundcost = product.Price * effectVal;
					product.AddToCashflow(0, 0, effectVal, -refundcost, 0, TimeOfDay.Instance.GetDate());
				}
				else if (Effect == "fans_change")
				{
					//Add/remove fans to the company
					if (product != null)
					{
						GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), product.Category);
						return EffectValue;
					}

					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
						return EffectValue;
					}
				}
				else if (Effect == "fans_change_os")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Parent.Name == "Operating System")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an OS Product for effect 'fans_change_os'!");
				}
				else if (Effect == "fans_change_oscomputer")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Computer" && prod.Category.Parent.Name == "Operating System")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an OS Product for effect 'fans_change_oscomputer'!");
				}
				else if (Effect == "fans_change_osconsole")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Console" && prod.Category.Parent.Name == "Operating System")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an OS Product for effect 'fans_change_osconsole'!");
				}
				else if (Effect == "fans_change_osphone")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Phone" && prod.Category.Parent.Name == "Operating System")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an OS Product for effect 'fans_change_osphone'!");
				}
				else if (Effect == "fans_change_office")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Default" && prod.Category.Parent.Name == "Office Software")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an Office Product for effect 'fans_change_office'!");
				}
				else if (Effect == "fans_change_2deditor")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Default" && prod.Category.Parent.Name == "2D Editor")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an 2D Editor Product for effect 'fans_change_2deditor'!");
				}
				else if (Effect == "fans_change_3deditor")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Default" && prod.Category.Parent.Name == "3D Editor")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an 3D Editor Product for effect 'fans_change_3deditor'!");
				}
				else if (Effect == "fans_change_audiotool")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Default" && prod.Category.Parent.Name == "Audio Tool")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an Audio Tool Product for effect 'fans_change_audiotool'!");
				}
				else if (Effect == "fans_change_antivirus")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Default" && prod.Category.Parent.Name == "Antivirus")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an Antivirus Product for effect 'fans_change_antivirus'!");
				}
				else if (Effect == "fans_change_game")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find a Game Product for effect 'fans_change_game'!");
				}
				else if (Effect == "fans_change_gamerpg")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "RPG" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an RPG Game Product for effect 'fans_change_gamerpg'!");
				}
				else if (Effect == "fans_change_gameadventure")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Adventure" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an Adventure Game Product for effect 'fans_change_gameadventure'!");
				}
				else if (Effect == "fans_change_gamesimulation")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Simulation" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find a Simulation Game Product for effect 'fans_change_gamesimulation'!");
				}
				else if (Effect == "fans_change_gamesports")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "Sports" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an Sports Game Product for effect 'fans_change_gamesports'!");
				}
				else if (Effect == "fans_change_gamerts")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "RTS" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an RTS Game Product for effect 'fans_change_gamerts'!");
				}
				else if (Effect == "fans_change_gamefps")
				{
					foreach (SoftwareProduct prod in GameSettings.Instance.MyCompany.Products)
					{
						if (prod.Category.Name == "FPS" && prod.Category.Parent.Name == "Game")
						{
							GameSettings.Instance.MyCompany.AddFans(int.Parse(EffectValue), prod.Category);
							return EffectValue;
						}
					}

					if (debugMode)
						DevConsole.Console.LogWarning("Couldn't find an FPS Game Product for effect 'fans_change_gamefps'!");
				}
				else if (Effect == "popularity_change_oscomputer")
				{
					MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Computer"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_osconsole")
				{
					MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Console"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_osphone")
				{
					MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Phone"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_office")
				{
					MarketSimulation.Active.SoftwareTypes["Office Software"].Categories["Default"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_2deditor")
				{
					MarketSimulation.Active.SoftwareTypes["2D Editor"].Categories["Default"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_3deditor")
				{
					MarketSimulation.Active.SoftwareTypes["3D Editor"].Categories["Default"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_audiotool")
				{
					MarketSimulation.Active.SoftwareTypes["Audio Tool"].Categories["Default"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_antivirus")
				{
					MarketSimulation.Active.SoftwareTypes["Antivirus"].Categories["Default"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gamerpg")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["RPG"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gameadventure")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["Adventure"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gamesimulation")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["Simulation"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gamesports")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["Sports"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gamerts")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["RTS"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "popularity_change_gamefps")
				{
					MarketSimulation.Active.SoftwareTypes["Game"].Categories["FPS"].Popularity += int.Parse(EffectValue);
				}
				else if (Effect == "pay_debt")
				{
					//Pay the debts of the player
					float debttopay = -GameSettings.Instance.MyCompany.Money;
					GameSettings.Instance.MyCompany.MakeTransaction(debttopay, Company.TransactionCategory.NA);
				}
				else if (Effect == "add_exp")
				{
					//Adds experience to a random role
					if (actor != null)
						switch (UnityEngine.Random.Range(0, 4))
						{
							case 0:
								actor.employee.SetSpecExperience(Employee.EmployeeRole.Lead, int.Parse(EffectValue));
								break;
							case 1:
								actor.employee.SetSpecExperience(Employee.EmployeeRole.Designer, int.Parse(EffectValue));
								break;
							case 2:
								actor.employee.SetSpecExperience(Employee.EmployeeRole.Programmer, int.Parse(EffectValue));
								break;
							case 3:
								actor.employee.SetSpecExperience(Employee.EmployeeRole.Artist, int.Parse(EffectValue));
								break;
							case 4:
								actor.employee.SetSpecExperience(Employee.EmployeeRole.Service, int.Parse(EffectValue));
								break;
						}
				}
				else if (Effect == "add_leaderexp")
				{
					if (actor != null)
					{
						actor.employee.SetSpecExperience(Employee.EmployeeRole.Lead, int.Parse(EffectValue));
					}
				}
				else if (Effect == "add_designerexp")
				{
					if (actor != null)
					{
						actor.employee.SetSpecExperience(Employee.EmployeeRole.Designer, int.Parse(EffectValue));
					}
				}
				else if (Effect == "add_programmerexp")
				{
					if (actor != null)
					{
						actor.employee.SetSpecExperience(Employee.EmployeeRole.Programmer, int.Parse(EffectValue));
					}
				}
				else if (Effect == "add_artistexp")
				{
					if (actor != null)
					{
						actor.employee.SetSpecExperience(Employee.EmployeeRole.Artist, int.Parse(EffectValue));
					}
				}
				else if (Effect == "add_serviceexp")
				{
					if (actor != null)
					{
						actor.employee.SetSpecExperience(Employee.EmployeeRole.Service, int.Parse(EffectValue));
					}
				}
				else if (Effect == "change_businessrep")
				{
					GameSettings.Instance.MyCompany.ChangeBusinessRep(int.Parse(EffectValue), "Event");
				}
				else if (Effect == "stockmarket_loss")
				{
					foreach (StockMarket market in GameSettings.Instance.StockMarkets)
					{
						market.Value = Utils.GetPercentage(market.Value, int.Parse(EffectValue), true);
					}
				}
				else if (Effect == "stockmarket_win")
				{
					foreach (StockMarket market in GameSettings.Instance.StockMarkets)
					{
						market.Value = Utils.GetPercentage(market.Value, int.Parse(EffectValue), false);
					}
				}
				else if (Effect == "stockmarket_singleloss")
				{
					if(marketobj == null)
						marketobj = GameSettings.Instance.StockMarkets[UnityEngine.Random.Range(0, GameSettings.Instance.StockMarkets.Count - 1)];
					marketobj.Value = Utils.GetPercentage(marketobj.Value, int.Parse(EffectValue), true);
				}
				else if (Effect == "stockmarket_singlewin")
				{
					if (marketobj == null)
						marketobj = GameSettings.Instance.StockMarkets[UnityEngine.Random.Range(0, GameSettings.Instance.StockMarkets.Count - 1)];
					marketobj.Value = Utils.GetPercentage(marketobj.Value, int.Parse(EffectValue), false);
				}
				else if (Effect == "change_employee_satisfaction")
				{
					if (actor != null)
					{
						actor.employee.JobSatisfaction += float.Parse(EffectValue);
					}
				}
				else if (Effect == "change_employee_stress")
				{
					if (actor != null)
					{
						actor.employee.Stress += float.Parse(EffectValue);
					}
				}
				else if (Effect == "change_bugs")
				{
					product.Bugs += int.Parse(EffectValue);
				}
				else if (Effect == "goes_home")
				{
					if (actor != null)
						actor.GoHomeNow = true;
				}
				else if (Effect == "add_marketing")
				{
					product.AddToMarketing(MarketSimulation.Active.GetMaxAwareness(product) * float.Parse(EffectValue));
				}
				else if (Effect == "fire_employee")
				{
					if(actor != null)
						actor.Fire(false);
				}
				else  if (Effect == "quit_employee")
				{
					if(actor != null)
						actor.Fire(true);
				}
				else if (Effect == "generates_bugs")
				{
					if(actor != null)
					{
						if(actor.MyWorkItem() != null)
						{
							//TODO: Do something with it
						}
					}
				}
				else if (Effect == "fixes_bugs")
				{
					if(actor != null)
					{
						if(actor.MyWorkItem() != null)
						{
							//TODO: Do somethign with it
						}
						
					}
				}
				else if (Effect == "change_salary")
				{
					if (actor != null)
					{
						if(actor.employee != null)
						{
							actor.employee.Salary += actor.employee.Salary * float.Parse(EffectValue);
						}
					}
				}
				return EffectValue;
			}
		}

		public class VariableCondition
		{
			public object ConditionObject { get; set; }
			public string Main { get; set; }
			public string[] Subs { get; set; }

			public VariableCondition(string str)
			{
				List<string> subs = new List<string>();
				foreach (string splittedCondition in str.Split('&'))
				{
					if (Main == null)
					{
						Main = splittedCondition;
					}
					else
					{
						subs.Add(splittedCondition);
					}
				}
				if (Main == null)
					Main = str;
				Subs = subs.ToArray();
			}

			public bool Check()
			{
				if (debugMode)
					DevConsole.Console.Log("Checking Condition " + Main);

				//Workaround because GameSettings.Instance.Founder can be null
				founder = GameSettings.Instance.sActorManager.Actors.GetRandomWhere(x => x.employee.Founder);

				//Check main condition first and set a ConditionObjects list
				#region main condition
				#region Variable Condition
				List<object> mainlist = new List<object>();
				bool IsVariableCondition = false;
				string ConditionString = Main;
				string Variable = "";
				string VariableSign = "";
				if (Main.Contains('<'))
				{
					IsVariableCondition = true;
					VariableSign = "<";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains('>'))
				{
					IsVariableCondition = true;
					VariableSign = ">";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains("<="))
				{
					IsVariableCondition = true;
					VariableSign = "<=";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains(">="))
				{
					IsVariableCondition = true;
					VariableSign = ">=";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains("!="))
				{
					IsVariableCondition = true;
					VariableSign = "!=";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains("!"))
				{
					IsVariableCondition = true;
					VariableSign = "!";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				else if (Main.Contains("="))
				{
					IsVariableCondition = true;
					VariableSign = "=";
					Variable = Main.Split(VariableSign.ToCharArray())[1];
					ConditionString = ConditionString.Replace(VariableSign + Variable, "");
				}
				#endregion

				if (ConditionString == "has_software")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						mainlist.Add(product);
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}

				}
				else if (ConditionString == "has_workitem")
				{
					foreach (WorkItem item in GameSettings.Instance.MyCompany.WorkItems)
						mainlist.Add(item);
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_game")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{

						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gamerpg")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "RPG")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gameadventure")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "Adventure")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gamesimulation")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "Simulation")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gamesports")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "Sports")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gamerts")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "RTS")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_gamefps")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Game" && product.Category.Name == "FPS")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_os")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Operating System")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_oscomputer")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Operating System" && product.Category.Name == "Computer")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_osconsole")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Operating System" && product.Category.Name == "Console")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_osphone")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Operating System" && product.Category.Name == "Phone")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_2deditor")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "2D Editor")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_3deditor")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "3D Editor")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_audiotool")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Audio Tool")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_office")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Office Software")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_antivirus")
				{
					foreach (SoftwareProduct product in GameSettings.Instance.MyCompany.Products)
						if (product.Type.Name == "Antivirus")
							mainlist.Add(product);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_staff")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Staff)
						mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_employee")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.AItype == AI.AIType.Employee)
							if(actor.employee.Founder == false)
								mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_leader")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.GetRole() == Employee.RoleBit.Lead)
							mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_designer")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.GetRole() == Employee.RoleBit.Designer)
							mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_programmer")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.GetRole() == Employee.RoleBit.Programmer)
							mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_artist")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.GetRole() == Employee.RoleBit.Artist)
							mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_service")
				{
					foreach (Actor actor in GameSettings.Instance.sActorManager.Actors)
						if (actor.GetRole() == Employee.RoleBit.Service)
							mainlist.Add(actor);

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "is_year")
				{
					int val = TimeOfDay.Instance.GetDate(true).Year;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 1)
							return false;
					}
				}
				else if (ConditionString == "is_month")
				{
					int val = TimeOfDay.Instance.GetDate(true).Month;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 1)
							return false;
					}
				}
				else if (ConditionString == "has_fans")
				{
					uint val = GameSettings.Instance.MyCompany.Fans;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 1)
							return false;
					}
				}
				else if (ConditionString == "has_money")
				{
					float val = GameSettings.Instance.MyCompany.Money;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 1)
							return false;
					}
				}
				else if (ConditionString == "has_snow")
				{
					float val = TimeOfDay.Instance.SnowAmount;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 0.001f)
							return false;
					}
				}
				else if (ConditionString == "has_stars")
				{
					int val = GameSettings.Instance.MyCompany.BusinessStars;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (val >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (val <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (val > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (val < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (val == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (val != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (val > 0)
								return false;
						}
					}
					else
					{
						if (val < 1)
							return false;
					}
				}
				else if (ConditionString == "player_has_car")
				{
					if (IsVariableCondition && VariableSign == "!")
					{
						
						if (founder.MyCar != null)
							return false;
					}
					else
					{
						if (founder.MyCar == null)
							return false;
					}
				}
				else if (ConditionString == "has_popularityoscomputer")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Computer"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularityosconsole")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Console"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularityosphone")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Operating System"].Categories["Phone"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularityoffice")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Office Software"].Categories["Default"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularity2deditor")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["2D Editor"].Categories["Default"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularity3deditor")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["3D Editor"].Categories["Default"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularityaudiotool")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Audio Tool"].Categories["Default"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularityantivirus")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Antivirus"].Categories["Default"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygamerpg")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["RPG"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygameadventure")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["Adventure"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygamesimulation")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["Simulation"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygamesports")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["Sports"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygamerts")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["RTS"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_popularitygamefps")
				{
					float popularity = MarketSimulation.Active.SoftwareTypes["Game"].Categories["FPS"].Popularity;

					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (popularity >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (popularity <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (popularity > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (popularity < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (popularity == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (popularity != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (popularity > 0)
								return false;
						}
					}
					else
					{
						if (popularity < 1)
							return false;
					}
				}
				else if (ConditionString == "has_stockmarket")
				{
					//Checks if game has stockmarket and returns a random stock
					if (GameSettings.Instance.StockMarkets.Count < 1)
						return false;
					mainlist.AddRange(GameSettings.Instance.StockMarkets);
				}
				else if (ConditionString == "has_supportwork")
				{
					foreach(WorkItem wi in GameSettings.Instance.MyCompany.WorkItems)
					{
						SupportWork sw = wi as SupportWork;
						if (sw != null)
							mainlist.Add(sw);
					}
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_activedeal")
				{
					foreach (WorkItem wi in GameSettings.Instance.MyCompany.WorkItems)
					{
						if (wi.ActiveDeal != null)
							mainlist.Add(wi);
					}
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_softwareworkitem")
				{				
					foreach (WorkItem wi in GameSettings.Instance.MyCompany.WorkItems)
					{
						SoftwareWorkItem swi = wi as SoftwareWorkItem;
						if (swi != null)
							mainlist.Add(swi);
					}
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_softwarealpha")
				{
					foreach (WorkItem wi in GameSettings.Instance.MyCompany.WorkItems)
					{
						SoftwareWorkItem swi = wi as SoftwareWorkItem;
						if (swi != null)
						{
							SoftwareAlpha salpha = swi as SoftwareAlpha;
							if(salpha != null)
								mainlist.Add(salpha);
						}
					}
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_designdocument")
				{
					foreach (WorkItem wi in GameSettings.Instance.MyCompany.WorkItems)
					{
						SoftwareWorkItem swi = wi as SoftwareWorkItem;
						if (swi != null)
						{
							DesignDocument design = swi as DesignDocument;
							if (design != null)
								mainlist.Add(design);
						}
					}
					if (IsVariableCondition)
					{
						if (VariableSign == "<")
						{
							if (mainlist.Count >= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">")
						{
							if (mainlist.Count <= uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "<=")
						{
							if (mainlist.Count > uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == ">=")
						{
							if (mainlist.Count < uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!=")
						{
							if (mainlist.Count == uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "=")
						{
							if (mainlist.Count != uint.Parse(Variable))
								return false;
						}
						else if (VariableSign == "!")
						{
							if (mainlist.Count > 0)
								return false;
						}
					}
					else
					{
						if (mainlist.Count < 1)
							return false;
					}
				}
				else if (ConditionString == "has_rain")
				{
					if (TimeOfDay.Instance.RainFactor < 0.1f)
						return false;
				}
				else if (ConditionString == "has_multiple_founders")
				{
					var founders = GameSettings.Instance.sActorManager.Actors.Where(x => x.employee.Founder);
					if (IsVariableCondition && VariableSign == "!")
					{

						if (founders.Count() > 1)
							return false;
					}
					else
					{
						if (founders.Count() < 2)
							return false;
					}
				}

				//Set a random object as conditionobject
				if (mainlist.Count > 0)
				{
					ConditionObject = mainlist.GetRandom();
				}
				#endregion


				//Return true if no subs are there
				if (Subs.Length < 1)
					return true;

				List<object> sublist = new List<object>();

				//Check if all subcondtions are valid for one of the ConditionObject
				#region sub conditions
				foreach (string subCondition in Subs)
				{
					if (debugMode)
						DevConsole.Console.Log("Checking Sub Condition " + subCondition);
					#region variable condition
					bool IsSubVariableCondition = false;
					string SubConditionString = subCondition;
					string SubVariable = "";
					string SubVariableSign = "";

					if (subCondition.Contains('<'))
					{
						IsSubVariableCondition = true;
						SubVariableSign = "<";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains('>'))
					{
						IsSubVariableCondition = true;
						SubVariableSign = ">";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains("<="))
					{
						IsSubVariableCondition = true;
						SubVariableSign = "<=";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains(">="))
					{
						IsSubVariableCondition = true;
						SubVariableSign = ">=";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains("!="))
					{
						IsSubVariableCondition = true;
						SubVariableSign = "!=";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains("!"))
					{
						IsSubVariableCondition = true;
						SubVariableSign = "!";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					else if (subCondition.Contains("="))
					{
						IsSubVariableCondition = true;
						SubVariableSign = "=";
						SubVariable = subCondition.Split(SubVariableSign.ToCharArray())[1];
						SubConditionString = SubConditionString.Replace(SubVariableSign + SubVariable, "");
					}
					#endregion

					if (SubConditionString == "release_date")
					{
						SDateTime rdate = Utils.RemoveDateTime(TimeOfDay.Instance.GetDate(true), int.Parse(SubVariable));
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release < rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release < rdate)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release > rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release > rdate)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release <= rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release <= rdate)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release >= rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release >= rdate)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release != rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release != rdate)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release == rdate)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Release == rdate)
											sublist.Add(product);
									}
								}
							}
						}
					}
					else if (SubConditionString == "has_bugs")
					{
						if (Variable == "")
							Variable = "0";
						int bugs = int.Parse(Variable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs < bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs < bugs)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs > bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs > bugs)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs <= bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs <= bugs)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs >= bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs >= bugs)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs != bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs != bugs)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs == bugs)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										if (product.Bugs == bugs)
											sublist.Add(product);
									}
								}
							}
						}
					}
					else if (SubConditionString == "has_sequel")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								SoftwareProduct product = obj as SoftwareProduct;
								if (product != null)
								{
									if (!product.HasSequel)
										tmplist.Add(product);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								SoftwareProduct product = obj as SoftwareProduct;
								if (product != null)
								{
									if (product.HasSequel)
										tmplist.Add(product);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_sequel")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								SoftwareProduct product = obj as SoftwareProduct;
								if (product != null)
								{
									if (product.SequelTo == null)
										tmplist.Add(product);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								SoftwareProduct product = obj as SoftwareProduct;
								if (product != null)
								{
									if (product.SequelTo != null)
										tmplist.Add(product);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "has_offsales")
					{
						int sales = int.Parse(Variable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach(int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales < sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales < sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales > sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales > sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales <= sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales <= sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales >= sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales >= sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales != sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales != sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales == sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(false))
										{
											productsales += sale;
										}
										if (productsales == sales)
											sublist.Add(product);
									}
								}
							}
						}
					}
					else if (SubConditionString == "has_onsales")
					{
						int sales = int.Parse(Variable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales < sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales < sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales > sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales > sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales <= sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales <= sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales >= sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales >= sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales != sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales != sales)
											sublist.Add(product);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales == sales)
											tmplst.Add(product);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									SoftwareProduct product = obj as SoftwareProduct;
									if (product != null)
									{
										int productsales = 0;
										foreach (int sale in product.GetUnitSales(true))
										{
											productsales += sale;
										}
										if (productsales == sales)
											sublist.Add(product);
									}
								}
							}
						}
					}
					else if (SubConditionString == "is_alpha")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();
						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								if (alpha != null)
								{
									if (alpha.InBeta)
										tmplist.Add(alpha);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								if (alpha != null)
								{
									if (!alpha.InBeta)
										tmplist.Add(alpha);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_beta")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								if (alpha != null)
								{
									if (!alpha.InBeta)
										tmplist.Add(alpha);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								if (alpha != null)
								{
									if (alpha.InBeta)
										tmplist.Add(alpha);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "from_playercompany")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								DesignDocument design = obj as DesignDocument;
								if(design != null)
								{
									if (design.MyCompany != GameSettings.Instance.MyCompany)
										tmplist.Add(design);
								}
								if (alpha != null)
								{
									
									if (alpha.MyCompany != GameSettings.Instance.MyCompany)
										tmplist.Add(alpha);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								SoftwareAlpha alpha = obj as SoftwareAlpha;
								DesignDocument design = obj as DesignDocument;
								if (design != null)
								{
									if (design.MyCompany == GameSettings.Instance.MyCompany)
										tmplist.Add(design);
								}
								if (alpha != null)
								{
									if (alpha.MyCompany == GameSettings.Instance.MyCompany)
										tmplist.Add(alpha);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "sick_days")
					{
						int sickdays = int.Parse(Variable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays < sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays < sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays > sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays > sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays <= sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays <= sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays >= sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays >= sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays != sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays != sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays == sickdays)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.SickDays == sickdays)
											sublist.Add(actor);
									}
								}
							}
						}
					}
					else if (SubConditionString == "is_female")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (!actor.employee.Female)
										tmplist.Add(actor);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (actor.employee.Female)
										tmplist.Add(actor);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_working")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (!actor.IsWorking)
										tmplist.Add(actor);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (actor.IsWorking)
										tmplist.Add(actor);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_founder")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (!actor.employee.Founder)
										tmplist.Add(actor);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (actor.employee.Founder)
										tmplist.Add(actor);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_crunching")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						if (VariableSign == "!")
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (!actor.IsCrunching())
										tmplist.Add(actor);
								}
							}
						}
						else
						{
							foreach (object obj in condlist)
							{
								Actor actor = obj as Actor;
								if (actor != null)
								{
									if (actor.IsCrunching())
										tmplist.Add(actor);
								}
							}
						}
						sublist = tmplist;
					}
					else if (SubConditionString == "is_stressed")
					{
						float stress = float.Parse(SubVariable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress < stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress < stress)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress > stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress > stress)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress <= stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress <= stress)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress >= stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress >= stress)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress != stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress != stress)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress == stress)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.Stress == stress)
											sublist.Add(actor);
									}
								}
							}
						}
					}
					else if (SubConditionString == "is_satisfied")
					{
						float satisfaction = float.Parse(SubVariable);
						if (SubVariableSign == "<")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction < satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction < satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction > satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction > satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "<=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction <= satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction <= satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == ">=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction >= satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction >= satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "!=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction != satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction != satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
						else if (SubVariableSign == "=")
						{
							if (sublist.Count > 0)
							{
								List<object> tmplst = new List<object>();
								foreach (object obj in sublist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction == satisfaction)
											tmplst.Add(actor);
									}
								}
								sublist = tmplst;
							}
							else
							{
								foreach (object obj in mainlist)
								{
									Actor actor = obj as Actor;
									if (actor != null)
									{
										if (actor.employee.JobSatisfaction == satisfaction)
											sublist.Add(actor);
									}
								}
							}
						}
					}
					else if (SubConditionString == "years_in_company")
					{
						//TODO: Find out how to get how many years the Actor is inside company
						foreach(object obj in mainlist)
						{
							Actor actor = obj as Actor;
							SDateTime dt = actor.employee.Hired;
							
							//TODO: Calculate the years from TODAY to actor.employee.Hired
						}
					}
					else if (SubConditionString == "salary")
					{
						List<object> tmplst = new List<object>();
						foreach (object obj in mainlist)
						{
							Actor actor = obj as Actor;
							float salary = actor.GetMonthlySalary();
							if(SubVariableSign == "<")
							{
								if(salary < float.Parse(SubVariable))
									tmplst.Add(actor);
							}
							else if(SubVariableSign == "<=")
							{
								if (salary <= float.Parse(SubVariable))
									tmplst.Add(actor);
							}
							else if(SubVariableSign == ">")
							{
								if (salary > float.Parse(SubVariable))
									tmplst.Add(actor);
							}
							else if(SubVariableSign == ">=")
							{
								if (salary >= float.Parse(SubVariable))
									tmplst.Add(actor);
							}
							else if(SubVariableSign == "=")
							{
								if (salary == float.Parse(SubVariable))
									tmplst.Add(actor);
							}
							else if(SubVariableSign == "!=")
							{
								if (salary != float.Parse(SubVariable))
									tmplst.Add(actor);
							}
						}
						sublist = tmplst;
					}
					else if (SubConditionString == "has_addon")
					{
						List<object> condlist = mainlist;
						if (sublist.Count > 0)
							condlist = sublist;
						List<object> tmplist = new List<object>();

						foreach (object obj in condlist)
						{
							SoftwareProduct product = obj as SoftwareProduct;
							if (product != null)
							{
								if (product.Addons.Count > 0)
								{
									tmplist.Add(product);
								}
							}
						}
						
						sublist = tmplist;
					}
				}
				if (sublist.Count < 1)
					return false;

				ConditionObject = sublist.GetRandom();
				//ConditionObject = sublist[UnityEngine.Random.Range(0, sublist.Count - 1)];
				#endregion
				return true;
			}
		}
	}
}
