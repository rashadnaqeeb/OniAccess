using System.Collections.Generic;

using STRINGS;

namespace OniAccess.Handlers.Screens.Starmap {
	internal static class StarmapHelper {

		// ========================================
		// DESTINATION QUERIES
		// ========================================

		internal static List<SpaceDestination> GetAllDestinations() {
			return SpacecraftManager.instance.destinations;
		}

		internal static SpacecraftManager.DestinationAnalysisState GetAnalysisState(
				SpaceDestination dest) {
			return SpacecraftManager.instance.GetDestinationAnalysisState(dest);
		}

		internal static bool IsAnalyzed(SpaceDestination dest) {
			return GetAnalysisState(dest) ==
				SpacecraftManager.DestinationAnalysisState.Complete;
		}

		/// <summary>
		/// Returns sorted list of non-empty distance tiers.
		/// Each entry is the OneBasedDistance value.
		/// </summary>
		internal static List<int> GetPopulatedDistanceTiers() {
			var tiers = new HashSet<int>();
			foreach (var dest in GetAllDestinations())
				tiers.Add(dest.OneBasedDistance);
			var sorted = new List<int>(tiers);
			sorted.Sort();
			return sorted;
		}

		internal static List<SpaceDestination> GetDestinationsAtTier(int oneBasedDistance) {
			var result = new List<SpaceDestination>();
			foreach (var dest in GetAllDestinations()) {
				if (dest.OneBasedDistance == oneBasedDistance)
					result.Add(dest);
			}
			return result;
		}

		// ========================================
		// DESTINATION LABELS
		// ========================================

		internal static string GetTierLabel(int oneBasedDistance) {
			return DisplayDistance(oneBasedDistance * 10000f);
		}

		internal static string GetDestinationLabel(SpaceDestination dest) {
			var state = GetAnalysisState(dest);
			if (state == SpacecraftManager.DestinationAnalysisState.Complete)
				return dest.GetDestinationType().Name;
			if (state == SpacecraftManager.DestinationAnalysisState.Discovered) {
				float score = SpacecraftManager.instance.GetDestinationAnalysisScore(dest.id);
				float pct = score / (float)TUNING.ROCKETRY.DESTINATION_ANALYSIS.COMPLETE * 100f;
				return string.Format(UI.STARMAP.ANALYSIS_AMOUNT,
					GameUtil.GetFormattedPercent(pct));
			}
			return (string)UI.STARMAP.UNKNOWN_DESTINATION;
		}

		// ========================================
		// ROCKET QUERIES
		// ========================================

		internal static List<Spacecraft> GetSpacecraft() {
			return SpacecraftManager.instance.GetSpacecraft();
		}

		internal static string GetStatusText(Spacecraft rocket) {
			return StarmapScreen.GetTextForState(rocket.state, rocket).first;
		}

		// ========================================
		// ROCKET LIST LABELS
		// ========================================

		internal static string BuildRocketListLabel(Spacecraft rocket) {
			string name = rocket.GetRocketName();
			string status = Speech.TextFilter.FilterForSpeech(GetStatusText(rocket));

			if (rocket.state == Spacecraft.MissionState.Grounded)
				return $"{name}, {status}";

			// In-flight: show time remaining and progress
			float duration = rocket.GetDuration();
			float timeLeft = rocket.GetTimeLeft();
			float pct = duration == 0f ? 0f : (1f - timeLeft / duration) * 100f;

			string timePrefix = rocket.controlStationBuffTimeRemaining > 0f
				? UI.STARMAP.ROCKETSTATUS.BOOSTED_TIME_MODIFIER.text
				: "";
			string cycles = GameUtil.GetFormattedCycles(timeLeft);
			string progress = GameUtil.GetFormattedPercent(pct);

			return $"{name}, {status}, {timePrefix}{cycles} {STRINGS.ONIACCESS.STARMAP.REMAINING}, {progress} {STRINGS.ONIACCESS.STARMAP.COMPLETE}";
		}

		// ========================================
		// ROCKET DETAIL DATA
		// ========================================

		internal struct DetailItem {
			public string Label;
			public DetailItemKind Kind;
		}

		internal enum DetailItemKind {
			Info,
			ChecklistReady,
			ChecklistWarning,
			ChecklistFailure,
		}

		internal static List<DetailItem> BuildRocketDetails(Spacecraft rocket) {
			var items = new List<DetailItem>();
			var lcm = rocket.launchConditions;
			var cmd = lcm.GetComponent<CommandModule>();

			// Mission status
			string status = Speech.TextFilter.FilterForSpeech(GetStatusText(rocket));
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATUS.STATUS}: {status}",
				Kind = DetailItemKind.Info
			});

			// In-flight timing
			if (rocket.state != Spacecraft.MissionState.Grounded) {
				float duration = rocket.GetDuration();
				float timeLeft = rocket.GetTimeLeft();
				float pct = duration == 0f ? 0f : (1f - timeLeft / duration) * 100f;
				string timePrefix = rocket.controlStationBuffTimeRemaining > 0f
					? UI.STARMAP.ROCKETSTATUS.BOOSTED_TIME_MODIFIER.text : "";
				items.Add(new DetailItem {
					Label = $"{UI.STARMAP.ROCKETSTATUS.TIMEREMAINING}: {timePrefix}{GameUtil.GetFormattedCycles(timeLeft)}, {GameUtil.GetFormattedPercent(pct)} {STRINGS.ONIACCESS.STARMAP.COMPLETE}",
					Kind = DetailItemKind.Info
				});
			}

			// Launch checklist
			foreach (var condition in lcm.GetLaunchConditionList()) {
				var evalStatus = condition.EvaluateCondition();
				string msg = condition.GetStatusMessage(evalStatus);
				string tooltip = condition.GetStatusTooltip(evalStatus);
				var kind = evalStatus == ProcessCondition.Status.Ready
					? DetailItemKind.ChecklistReady
					: evalStatus == ProcessCondition.Status.Warning
						? DetailItemKind.ChecklistWarning
						: DetailItemKind.ChecklistFailure;
				string label = msg;
				if (evalStatus != ProcessCondition.Status.Ready
						&& !string.IsNullOrEmpty(tooltip))
					label = $"{msg}: {tooltip}";
				items.Add(new DetailItem { Label = label, Kind = kind });
			}

			// Range
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.TOTAL_OXIDIZABLE_FUEL}: {GameUtil.GetFormattedMass(cmd.rocketStats.GetTotalOxidizableFuel())}",
				Kind = DetailItemKind.Info
			});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.ENGINE_EFFICIENCY}: {GameUtil.GetFormattedEngineEfficiency(cmd.rocketStats.GetEngineEfficiency())}",
				Kind = DetailItemKind.Info
			});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.OXIDIZER_EFFICIENCY}: {GameUtil.GetFormattedPercent(cmd.rocketStats.GetAverageOxidizerEfficiency())}",
				Kind = DetailItemKind.Info
			});
			float booster = cmd.rocketStats.GetBoosterThrust() * 1000f;
			if (booster != 0f)
				items.Add(new DetailItem {
					Label = $"{UI.STARMAP.ROCKETSTATS.SOLID_BOOSTER}: {GameUtil.GetFormattedDistance(booster)}",
					Kind = DetailItemKind.Info
				});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.TOTAL_THRUST}: {GameUtil.GetFormattedDistance(cmd.rocketStats.GetTotalThrust() * 1000f)}",
				Kind = DetailItemKind.Info
			});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.TOTAL_RANGE}: {GameUtil.GetFormattedDistance(cmd.rocketStats.GetRocketMaxDistance() * 1000f)}",
				Kind = DetailItemKind.Info
			});

			// Mass
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.DRY_MASS}: {GameUtil.GetFormattedMass(cmd.rocketStats.GetDryMass(), GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}",
				Kind = DetailItemKind.Info
			});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATS.WET_MASS}: {GameUtil.GetFormattedMass(cmd.rocketStats.GetWetMass(), GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}",
				Kind = DetailItemKind.Info
			});
			items.Add(new DetailItem {
				Label = $"{UI.STARMAP.ROCKETSTATUS.TOTAL}: {GameUtil.GetFormattedMass(cmd.rocketStats.GetTotalMass(), GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}",
				Kind = DetailItemKind.Info
			});

			// Fuel tanks
			var network = AttachableBuilding.GetAttachedNetwork(
				cmd.GetComponent<AttachableBuilding>());
			Tag engineFuelTag = cmd.rocketStats.GetEngineFuelTag();
			foreach (var module in network) {
				var tank = module.GetComponent<IFuelTank>();
				if (!tank.IsNullOrDestroyed()) {
					var storage = (tank as UnityEngine.MonoBehaviour)
						.GetComponent<Storage>();
					if (storage != null) {
						float mass = storage.GetMassAvailable(engineFuelTag);
						var elem = ElementLoader.FindElementByHash(
							ElementLoader.GetElementID(engineFuelTag));
						string elemName = elem != null ? elem.name : engineFuelTag.Name;
						items.Add(new DetailItem {
							Label = $"{module.GetProperName()} ({elemName}): {GameUtil.GetFormattedMass(mass, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}",
							Kind = DetailItemKind.Info
						});
					}
				}
			}

			// Oxidizer tanks
			foreach (var module in network) {
				var oxTank = module.GetComponent<OxidizerTank>();
				if (oxTank != null) {
					foreach (var kvp in oxTank.GetOxidizersAvailable()) {
						if (kvp.Value > 0f) {
							items.Add(new DetailItem {
								Label = $"{module.GetProperName()} ({kvp.Key.Name}): {GameUtil.GetFormattedMass(kvp.Value, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}",
								Kind = DetailItemKind.Info
							});
						}
					}
				}
			}

			// Storage bays
			foreach (var module in network) {
				var bay = module.GetComponent<CargoBay>();
				if (bay != null) {
					var storage = module.GetComponent<Storage>();
					if (storage != null) {
						float used = storage.MassStored();
						float cap = storage.capacityKg;
						items.Add(new DetailItem {
							Label = $"{module.GetProperName()}: {GameUtil.GetFormattedMass(used)} / {GameUtil.GetFormattedMass(cap)}",
							Kind = DetailItemKind.Info
						});
					}
				}
			}

			// Passengers
			var minionStorage = lcm.GetComponent<MinionStorage>();
			if (minionStorage != null) {
				int count = minionStorage.GetStoredMinionInfo().Count;
				items.Add(new DetailItem {
					Label = $"{UI.STARMAP.LISTTITLES.PASSENGERS}: {count}",
					Kind = DetailItemKind.Info
				});
			}

			// Module list
			foreach (var module in network) {
				items.Add(new DetailItem {
					Label = module.GetProperName(),
					Kind = DetailItemKind.Info
				});
			}

			return items;
		}

		// ========================================
		// DESTINATION DETAIL DATA
		// ========================================

		internal static List<string> BuildDestinationDetails(
				SpaceDestination dest, Spacecraft activeRocket) {
			var items = new List<string>();
			var destType = dest.GetDestinationType();
			bool analyzed = IsAnalyzed(dest);

			// Name/type
			if (analyzed) {
				items.Add(destType.Name);
				items.Add(destType.typeName);
			} else {
				items.Add((string)UI.STARMAP.UNKNOWN_DESTINATION);
				items.Add((string)UI.STARMAP.UNKNOWN_TYPE);
			}

			// Distance
			items.Add($"{DisplayDistance(dest.OneBasedDistance * 10000f)}");

			// Locked-in status
			if (activeRocket != null
					&& activeRocket.state != Spacecraft.MissionState.Grounded) {
				var rocketDest = SpacecraftManager.instance.GetSpacecraftDestination(
					activeRocket.launchConditions);
				if (rocketDest != null && rocketDest.id == dest.id)
					items.Add((string)UI.STARMAP.ROCKETSTATUS.LOCKEDIN);
			}

			// Description
			if (analyzed && !string.IsNullOrEmpty(destType.description))
				items.Add(destType.description);

			// Analysis progress
			float score = SpacecraftManager.instance.GetDestinationAnalysisScore(dest.id);
			float analysisPct = score
				/ (float)TUNING.ROCKETRY.DESTINATION_ANALYSIS.COMPLETE * 100f;
			if (!analyzed) {
				items.Add(string.Format(UI.STARMAP.ANALYSIS_AMOUNT,
					GameUtil.GetFormattedPercent(analysisPct)));
			} else {
				items.Add((string)UI.STARMAP.ANALYSIS_COMPLETE);
			}

			// Analysis target indicator
			int currentTarget = SpacecraftManager.instance
				.GetStarmapAnalysisDestinationID();
			if (currentTarget == dest.id)
				items.Add(STRINGS.ONIACCESS.STARMAP.ANALYZING_THIS);

			if (!analyzed) {
				// Add analyze action item at end
				AddAnalyzeAction(items, dest);
				return items;
			}

			// Research opportunities
			foreach (var opp in dest.researchOpportunities) {
				string prefix = opp.completed
					? STRINGS.ONIACCESS.STARMAP.RESEARCH_COMPLETE_PREFIX
					: STRINGS.ONIACCESS.STARMAP.RESEARCH_INCOMPLETE_PREFIX;
				string rare = opp.discoveredRareResource != SimHashes.Void
					? $" {STRINGS.ONIACCESS.STARMAP.RARE_RESOURCE} " : " ";
				items.Add($"{prefix}{rare}{opp.description}, {opp.dataValue} {STRINGS.ONIACCESS.STARMAP.DATA_POINTS}");
			}

			// Mass
			items.Add($"{UI.STARMAP.CURRENT_MASS}: {GameUtil.GetFormattedMass(dest.CurrentMass, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}");
			items.Add($"{UI.STARMAP.MAXIMUM_MASS}: {GameUtil.GetFormattedMass(destType.maxiumMass, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}");
			items.Add($"{UI.STARMAP.MINIMUM_MASS}: {GameUtil.GetFormattedMass(destType.minimumMass, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)}");
			items.Add($"{UI.STARMAP.REPLENISH_RATE}: {GameUtil.GetFormattedMass(destType.replishmentPerCycle, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Kilogram)}");

			// Element composition
			float totalMass = dest.GetTotalMass();
			foreach (var kvp in dest.recoverableElements) {
				var element = ElementLoader.FindElementByHash(kvp.Key);
				float pct = totalMass > 0f
					? dest.GetResourceValue(kvp.Key, kvp.Value) / totalMass * 100f
					: 0f;
				string pctStr = pct <= 1f
					? (string)UI.STARMAP.COMPOSITION_SMALL_AMOUNT
					: GameUtil.GetFormattedPercent(pct);
				string compat = GetCargoCompatibility(element, activeRocket);
				items.Add($"{element.name}: {pctStr}{compat}");
			}

			// Undiscovered elements (rare resources not yet found)
			foreach (var opp in dest.researchOpportunities) {
				if (!opp.completed && opp.discoveredRareResource != SimHashes.Void)
					items.Add($"{UI.STARMAP.COMPOSITION_UNDISCOVERED}: {UI.STARMAP.COMPOSITION_UNDISCOVERED_AMOUNT}");
			}

			// Recoverable entities
			foreach (var kvp in dest.GetRecoverableEntities()) {
				var prefab = Assets.GetPrefab(kvp.Key);
				string name = prefab.GetProperName();
				string compat = GetEntityCompatibility(activeRocket);
				items.Add($"{name}{compat}");
			}

			// Artifact drop rates
			var dropTable = destType.artifactDropTable;
			if (dropTable != null) {
				foreach (var rate in dropTable.rates) {
					string tierName = Strings.Get(rate.first.name_key);
					float dropPct = rate.second / dropTable.totalWeight * 100f;
					items.Add($"{tierName}: {GameUtil.GetFormattedPercent(dropPct)}");
				}
			}

			// Analyze action
			AddAnalyzeAction(items, dest);

			return items;
		}

		internal static string GetAnalyzeActionLabel(SpaceDestination dest) {
			if (IsAnalyzed(dest))
				return (string)UI.STARMAP.ANALYSIS_COMPLETE;
			if (dest.id == SpacecraftManager.instance.GetStarmapAnalysisDestinationID())
				return (string)UI.STARMAP.SUSPEND_DESTINATION_ANALYSIS;
			return (string)UI.STARMAP.ANALYZE_DESTINATION;
		}

		private static void AddAnalyzeAction(List<string> items, SpaceDestination dest) {
			items.Add(GetAnalyzeActionLabel(dest));
		}

		// ========================================
		// LAUNCH
		// ========================================

		internal static string TryLaunch(Spacecraft rocket) {
			if (rocket == null)
				return STRINGS.ONIACCESS.STARMAP.NO_ROCKET_SELECTED;
			if (rocket.state != Spacecraft.MissionState.Grounded)
				return STRINGS.ONIACCESS.STARMAP.ROCKET_NOT_GROUNDED;

			var lcm = rocket.launchConditions;
			var dest = SpacecraftManager.instance.GetSpacecraftDestination(lcm);

			if (dest == null)
				return (string)UI.STARMAP.DESTINATIONSELECTION.NOTSELECTED;

			// Check all conditions, report all failures
			var failures = new List<string>();
			foreach (var condition in lcm.GetLaunchConditionList()) {
				var status = condition.EvaluateCondition();
				if (status != ProcessCondition.Status.Ready) {
					string msg = condition.GetStatusMessage(status);
					string tooltip = condition.GetStatusTooltip(status);
					if (!string.IsNullOrEmpty(tooltip))
						failures.Add($"{msg}: {tooltip}");
					else
						failures.Add(msg);
				}
			}

			if (failures.Count > 0)
				return string.Join(", ", failures.ToArray());

			lcm.Launch(dest);
			PlaySound("HUD_Click");
			return string.Format(STRINGS.ONIACCESS.STARMAP.LAUNCHED,
				rocket.GetRocketName());
		}

		// ========================================
		// CARGO COMPATIBILITY HELPERS
		// ========================================

		private static string GetCargoCompatibility(Element element, Spacecraft rocket) {
			if (rocket == null || rocket.state != Spacecraft.MissionState.Grounded)
				return "";
			var cmd = rocket.launchConditions.GetComponent<CommandModule>();
			var network = AttachableBuilding.GetAttachedNetwork(
				cmd.GetComponent<AttachableBuilding>());

			bool hasMatchingBay = false;
			string bayName = "";

			if (element.IsGas) {
				bayName = Assets.GetPrefab("GasCargoBay".ToTag()).GetProperName();
				foreach (var m in network)
					if (m.GetComponent<CargoBay>() != null
							&& m.GetComponent<CargoBay>().storageType
								== CargoBay.CargoType.Gasses) {
						hasMatchingBay = true;
						break;
					}
			} else if (element.IsLiquid) {
				bayName = Assets.GetPrefab("LiquidCargoBay".ToTag()).GetProperName();
				foreach (var m in network)
					if (m.GetComponent<CargoBay>() != null
							&& m.GetComponent<CargoBay>().storageType
								== CargoBay.CargoType.Liquids) {
						hasMatchingBay = true;
						break;
					}
			} else if (element.IsSolid) {
				bayName = Assets.GetPrefab("CargoBay".ToTag()).GetProperName();
				foreach (var m in network)
					if (m.GetComponent<CargoBay>() != null
							&& m.GetComponent<CargoBay>().storageType
								== CargoBay.CargoType.Solids) {
						hasMatchingBay = true;
						break;
					}
			}

			if (string.IsNullOrEmpty(bayName))
				return "";
			return hasMatchingBay
				? $" ({STRINGS.ONIACCESS.STARMAP.CAN_CARRY})"
				: $" ({STRINGS.ONIACCESS.STARMAP.NEEDS_BAY}: {bayName})";
		}

		private static string GetEntityCompatibility(Spacecraft rocket) {
			if (rocket == null || rocket.state != Spacecraft.MissionState.Grounded)
				return "";
			var cmd = rocket.launchConditions.GetComponent<CommandModule>();
			var network = AttachableBuilding.GetAttachedNetwork(
				cmd.GetComponent<AttachableBuilding>());

			bool hasBay = false;
			foreach (var m in network) {
				if (m.GetComponent<CargoBay>() != null
						&& m.GetComponent<CargoBay>().storageType
							== CargoBay.CargoType.Entities) {
					hasBay = true;
					break;
				}
			}

			string bayName = DlcManager.IsPureVanilla()
				? Assets.GetPrefab("SpecialCargoBay".ToTag()).GetProperName()
				: Assets.GetPrefab("SpecialCargoBayCluster".ToTag()).GetProperName();
			return hasBay
				? $" ({STRINGS.ONIACCESS.STARMAP.CAN_CARRY})"
				: $" ({STRINGS.ONIACCESS.STARMAP.NEEDS_BAY}: {bayName})";
		}

		// ========================================
		// FORMATTING
		// ========================================

		private static string DisplayDistance(float distance) {
			return GameUtil.GetFormattedDistance(distance);
		}

		internal static void PlaySound(string clipName) {
			BaseScreenHandler.PlaySound(clipName);
		}
	}
}
