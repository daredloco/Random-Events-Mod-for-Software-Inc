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
				if (CompanyEffects.TryApply(this))
					return EffectValue;

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

				if (Effect == "software_offsales")
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
					float debttopay = (float)-GameSettings.Instance.MyCompany.Money;
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
					product.AddBugs(int.Parse(EffectValue));
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
				else if (Effect == "quit_employee")
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
							actor.employee.Salary += actor.employee.Salary * (float.Parse(EffectValue) / 100);
						}
					}
				}
				else if (Effect == "call_event")
				{
					//TODO: Check if its world or random event
					var event2call = EventsHandlerV2.RandomEvents.FirstOrDefault(x => x.Title == EffectValue);
					if(event2call != null)
					{ 
						event2call.Fire(false);
					}
				}
				return EffectValue;
			}
		}
}
