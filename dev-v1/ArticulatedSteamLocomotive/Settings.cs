using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using UnityEngine;

namespace NS15
{
	namespace ArticulatedSteamLocomotive
	{
		public static class Main
		{
			public static bool enabled;
			public static Settings settings;

			// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
			static bool Load(UnityModManager.ModEntry modEntry)
			{
				Harmony? harmony = null;
				settings = Settings.Load<Settings>(modEntry);
				modEntry.OnGUI = OnGUI;
				modEntry.OnSaveGUI = OnSaveGUI;
				modEntry.OnUpdate = OnUpdate;
				modEntry.OnToggle = OnToggle;

				try
				{
					harmony = new Harmony(modEntry.Info.Id);
					harmony.PatchAll(Assembly.GetExecutingAssembly());

					// Other plugin startup logic
				}
				catch (Exception ex)
				{
					modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
					harmony?.UnpatchAll(modEntry.Info.Id);
					return false;
				}

				return true;
			}

			static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
			{

			}

			static void OnGUI(UnityModManager.ModEntry modEntry)
			{
				// GUILayout.Label("Hello World!");
				settings.Draw(modEntry);
			}

			static void OnSaveGUI(UnityModManager.ModEntry modEntry)
			{
				settings.Save(modEntry);
			}

			static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
			{
				enabled = value;
				return true; // If true, the mod will switch the state. If not, the state will not change.
			}
		}
		public class Settings : UnityModManager.ModSettings, IDrawable
		{


			public override void Save(UnityModManager.ModEntry modEntry)
			{
				Save(this, modEntry);
			}

			public void OnChange()
			{
			}
		}
	}
}
