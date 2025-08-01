using HarmonyLib;
using JsonSubTypes;
using Model;
using Model.Definition.Data;
using RollingStock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NS15
{
	namespace ArticulatedSteamLocomotive
	{
		public class CouplerReferences
		{
			public TransformReference CouplerParentF;
			public TransformReference CouplerParentR;
			internal Transform parentF;
			internal Transform parentR;

			public float OffsetF;
			public float OffsetR;

		}
		public class ArticulatedSteamLocomotiveDefinition : SteamLocomotiveDefinition
		{
			public override string Kind => "ArticulatedSteamLocomotive";

			public CouplerReferences Couplers = new CouplerReferences();
		}

		[HarmonyPatch(typeof(Car))]
		public static class CarPatches
		{
			[HarmonyPatch("CouplerPivot"), HarmonyPrefix]
			public static bool CouplerPivotPrefix(Car __instance, ref Vector3 __result, Car.End end, float extra = 0f)
			{
				if (__instance is SteamLocomotive sl && sl.LocoDefinition is ArticulatedSteamLocomotiveDefinition asldef)
				{
					CouplerReferences couplers = asldef.Couplers;
					if ((end == Car.End.F && couplers.parentF == null) || (end == Car.End.R && couplers.parentR == null)) { return true; }

					Vector3 pos = end == Car.End.F ? new Vector3(0f, -couplers.OffsetF, __instance.couplerHeight - couplers.parentF.localPosition.y) : new Vector3(0f, -couplers.OffsetR, __instance.couplerHeight - couplers.parentR.localPosition.y);
					pos.y -= end == Car.End.F ? extra : -extra;
					Transform parent = end == Car.End.F ? couplers.parentF : couplers.parentR;

					__result = parent.rotation * pos + parent.position;
					// Debug.Log($"returning {__result}");
					return false;
				}
				return true;
			}

			[HarmonyPatch("SetupCouplers"), HarmonyPostfix]
			public static void SetupCouplersPostfix(Car __instance, Coupler couplerPrefab)
			{
				if (__instance is SteamLocomotive sl && sl.LocoDefinition is ArticulatedSteamLocomotiveDefinition asldef) // does asldef reference the original one in __instance.LocoDefinition?
				{
					CouplerReferences couplers = asldef.Couplers;
					MethodInfo mi_WantsEndGear = AccessTools.Method(typeof(SteamLocomotive), "WantsEndGear");
					if ((bool)mi_WantsEndGear.Invoke(sl,[Car.End.F]))
					{
						if (couplers.CouplerParentF != null)
						{
							couplers.parentF = __instance.Resolve(couplers.CouplerParentF);
						}
					}
					if ((bool)mi_WantsEndGear.Invoke(sl, [Car.End.R]))
					{
						if (couplers.CouplerParentR != null)
						{
							couplers.parentR = __instance.Resolve(couplers.CouplerParentR);
						}
					}
				}
			}
		}



		[HarmonyPatch(typeof(JsonSubtypes))]
		public static class JsonSubtypesPatches
		{
			[HarmonyPatch("GetSubTypeMapping"), HarmonyPostfix]
			public static void GetSubTypeMappingPostfix(Type type, JsonSubtypes __instance, ref NullableDictionary<object, Type> __result)
			{
				__result.Add("ArticulatedSteamLocomotive", typeof(ArticulatedSteamLocomotiveDefinition));
			}
		}

		public static class TrainControllerPatches
		{
			[HarmonyPatch("CreateCarRaw"), HarmonyTranspiler]
			public static IEnumerable<CodeInstruction> CreateCarRawTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				List<CodeInstruction> codes = instructions.ToList();

				MethodInfo archetypeGetter = AccessTools.Property(typeof(CarDefinition), "Archetype").GetGetMethod();
				MethodInfo kindGetter = AccessTools.Property(typeof(CarDefinition), "Kind").GetGetMethod();
				MethodInfo addCarKind = AccessTools.Method(typeof(TrainControllerPatches), nameof(AddCarKind));
				var kindLocal = generator.DeclareLocal(typeof(string));

				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo methodInfo && methodInfo == archetypeGetter)
					{
						yield return new CodeInstruction(OpCodes.Callvirt, kindGetter);
						yield return new CodeInstruction(OpCodes.Stloc_S, kindLocal.LocalIndex);
						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Ldloc_S, kindLocal.LocalIndex);
						yield return new CodeInstruction(OpCodes.Call, addCarKind);
						yield return new CodeInstruction(OpCodes.Stloc_S, 5);
						i += 18;
						continue;
					}
					yield return codes[i];
				}

			}

			public static Car AddCarKind(GameObject gameObject, string definitionKind)
			{
				Type type = CarJsonSubTypes[definitionKind] ?? typeof(Car);
				Debug.Log($"Trying to get car typed {type} with definitionKind {definitionKind}");
				Car addedCar = (Car)gameObject.AddComponent(type);
				// Debug.Log($"Internal function AddCarKind returning {addedCar.GetType()} {addedCar}");
				return addedCar;
			}

			public static Dictionary<string, Type> CarJsonSubTypes = new Dictionary<string, Type>()
			{
				// There was originally something to be done here
				{"SteamLocomotive", typeof(SteamLocomotive)},
				{"DieselLocomotive", typeof(DieselLocomotive)},
				{"BaseLocomotive", typeof(BaseLocomotive)},
				{"ArticulatedSteamLocomotive", typeof(SteamLocomotive)},
			};

			[HarmonyPatch("CreateCarRaw"), HarmonyPostfix]
			public static void CreateCarRawPostfix(ref Car __result)
			{
				Debug.Log($"CreateCarRaw returning {__result.GetType()} {__result}");
			}
		}
	}


}
