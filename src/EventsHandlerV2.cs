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
		internal static List<Event> RandomEvents = new List<Event>();
		internal static List<Event> WorldEvents = new List<Event>();

		public static bool hasEventRunning = false;
		public static bool debugMode = false;
		public static bool skipfastforward = true;
		public static byte eventMode = 1; //1=only random, 2=only real, 3=both
		public static bool ignoreWorldChance = true; //Should the chance in world events be ignored?
		public static uint cooldown = 48; //Cooldown in hours
		internal static bool iscoolingdown = false;
		int cooldownCounter = 0;

		internal static Actor founder;

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
					DevConsole.Console.LogWarning("Couldn't find file inside the local folder, trying the steam folder! => " + ex.Message);
				
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
	}
}
