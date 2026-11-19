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
					float val = (float)GameSettings.Instance.MyCompany.Money;

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
										if (product.Bugss < bugs)
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
										if (product.Bugss < bugs)
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
										if (product.Bugss > bugs)
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
										if (product.Bugss > bugs)
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
										if (product.Bugss <= bugs)
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
										if (product.Bugss <= bugs)
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
										if (product.Bugss >= bugs)
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
										if (product.Bugss >= bugs)
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
										if (product.Bugss != bugs)
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
										if (product.Bugss != bugs)
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
										if (product.Bugss == bugs)
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
										if (product.Bugss == bugs)
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
