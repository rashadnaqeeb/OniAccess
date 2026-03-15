using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Extracts speech text from ICodexWidget data objects.
	/// Uses Traverse for private fields on panel widgets.
	/// All methods read the widget's data model directly — no UI hierarchy walking.
	/// </summary>
	internal static class WidgetTextExtractor {
		/// <summary>
		/// Appends a semicolon-separated item list to sb. Each item is
		/// "name, formatted". Uses a bool tracker instead of loop index
		/// so that skipped items don't leave a leading separator.
		/// </summary>
		private static void AppendItemList(StringBuilder sb, int count,
				System.Func<int, Tag> getTag, System.Func<int, string> getFormatted,
				System.Func<int, bool> skip = null) {
			bool first = true;
			for (int i = 0; i < count; i++) {
				if (skip != null && skip(i)) continue;
				if (!first) sb.Append("; ");
				first = false;
				sb.Append(getTag(i).ProperName());
				sb.Append(", ");
				sb.Append(getFormatted(i));
			}
		}

		/// <summary>
		/// Returns speech text for a widget, or null if the widget should be skipped.
		/// </summary>
		internal static string GetText(ICodexWidget widget, string currentEntryId = null) {
			string raw = GetRawText(widget, currentEntryId);
			return Widgets.WidgetOps.CleanTooltipEntry(raw);
		}

		/// <summary>
		/// Returns one content line per newline-separated segment in the widget text.
		/// Element descriptions, critter descriptions, etc. pack multiple properties
		/// into a single CodexText separated by newlines. Splitting them gives each
		/// property its own cursor position.
		/// </summary>
		internal static List<string> GetTextLines(ICodexWidget widget, string currentEntryId = null) {
			string raw = GetRawText(widget, currentEntryId);
			if (string.IsNullOrEmpty(raw)) return null;

			string[] parts = raw.Split('\n');
			if (parts.Length <= 1) {
				string cleaned = Widgets.WidgetOps.CleanTooltipEntry(raw);
				if (string.IsNullOrEmpty(cleaned)) return null;
				return new List<string> { cleaned };
			}

			var result = new List<string>();
			foreach (string part in parts) {
				string cleaned = Widgets.WidgetOps.CleanTooltipEntry(part);
				if (!string.IsNullOrEmpty(cleaned))
					result.Add(cleaned);
			}
			return result.Count > 0 ? result : null;
		}

		private static string GetRawText(ICodexWidget widget, string currentEntryId) {
			if (widget is CodexText ct)
				return GetCodexTextSpeech(ct);
			if (widget is CodexTextWithTooltip ctwt)
				return GetCodexTextWithTooltipSpeech(ctwt);
			if (widget is CodexLabelWithLargeIcon clli)
				return clli.label?.text;
			if (widget is CodexLabelWithIcon cli)
				return cli.label?.text;
			if (widget is CodexIndentedLabelWithIcon cili)
				return cili.label?.text;
			if (widget is CodexRecipePanel crp)
				return GetRecipeSpeech(crp, currentEntryId);
			if (widget is CodexConversionPanel ccp)
				return GetConversionSpeech(ccp, currentEntryId);
			if (widget is CodexTemperatureTransitionPanel cttp)
				return GetTemperatureTransitionSpeech(cttp);
			if (widget is CodexConfigurableConsumerRecipePanel ccrp)
				return GetConsumerRecipeSpeech(ccrp);
			if (widget is CodexCollapsibleHeader cch)
				return GetCollapsibleHeaderSpeech(cch);
			if (widget is CodexVideo cv)
				return GetVideoSpeech(cv);
			if (widget is CodexContentLockedIndicator)
				return (string)STRINGS.ONIACCESS.CODEX.LOCKED_CONTENT;
			// Skip visual-only widgets: CodexImage, CodexDividerLine,
			// CodexSpacer, CodexLargeSpacer, CodexCritterLifecycleWidget
			return null;
		}

		/// <summary>
		/// Whether the widget is a section heading (for Ctrl+Up/Down jumping).
		/// </summary>
		internal static bool IsSectionHeading(ICodexWidget widget) {
			if (widget is CodexText ct)
				return ct.style == CodexTextStyle.Title || ct.style == CodexTextStyle.Subtitle;
			if (widget is CodexCollapsibleHeader)
				return true;
			return false;
		}

		/// <summary>
		/// Get navigable links from a widget.
		/// Returns (entryID, displayText) pairs for valid codex entries.
		/// </summary>
		internal static List<(string id, string text)> GetLinks(ICodexWidget widget) {
			var links = new List<(string id, string text)>();

			if (widget is CodexText ct && ct.style == CodexTextStyle.Body)
				links.AddRange(CodexHelper.ExtractTextLinks(ct.text));

			if (widget is CodexLabelWithLargeIcon clli && !string.IsNullOrEmpty(clli.linkID))
				links.Add((clli.linkID, clli.label?.text ?? clli.linkID));

			if (widget is CodexRecipePanel crp)
				links.AddRange(GetRecipeLinks(crp));

			if (widget is CodexConversionPanel ccp)
				links.AddRange(GetConversionLinks(ccp));

			// Filter to entries that actually exist in the codex
			links.RemoveAll(l => !CodexCache.entries.ContainsKey(l.id));

			return links;
		}

		// ========================================
		// TEXT WIDGETS
		// ========================================

		private static string GetCodexTextSpeech(CodexText ct) {
			return ct.text;
		}

		private static string GetCodexTextWithTooltipSpeech(CodexTextWithTooltip ctwt) {
			return Widgets.WidgetOps.AppendTooltip(ctwt.text, ctwt.tooltip);
		}

		// ========================================
		// COLLAPSIBLE HEADER
		// ========================================

		private static string GetCollapsibleHeaderSpeech(CodexCollapsibleHeader header) {
			return Traverse.Create(header).Field<string>("label").Value;
		}

		// ========================================
		// VIDEO
		// ========================================

		private static string GetVideoSpeech(CodexVideo video) {
			var sb = new StringBuilder();
			sb.Append((string)STRINGS.ONIACCESS.HANDLERS.VIDEO);
			if (video.overlayTexts != null && video.overlayTexts.Count > 0) {
				sb.Append(". ");
				sb.Append(string.Join(". ", video.overlayTexts));
			}
			return sb.ToString();
		}

		// ========================================
		// RECIPE PANEL
		// ========================================

		private static string GetRecipeSpeech(CodexRecipePanel panel, string currentEntryId) {
			var t = Traverse.Create(panel);
			var complexRecipe = t.Field<ComplexRecipe>("complexRecipe").Value;
			var simpleRecipe = t.Field<Recipe>("recipe").Value;
			bool useFabTitle = t.Field<bool>("useFabricatorForTitle").Value;

			if (complexRecipe != null)
				return BuildComplexRecipeSpeech(complexRecipe, useFabTitle, currentEntryId);
			if (simpleRecipe != null)
				return BuildSimpleRecipeSpeech(simpleRecipe);
			return null;
		}

		private static string BuildComplexRecipeSpeech(ComplexRecipe recipe, bool useFabTitle, string currentEntryId) {
			var sb = new StringBuilder();

			// Title
			if (useFabTitle && recipe.fabricators.Count > 0) {
				var fab = Assets.GetPrefab(recipe.fabricators[0].Name.ToTag());
				if (fab != null) sb.Append(fab.GetProperName());
			} else if (recipe.results.Length > 0) {
				sb.Append(recipe.results[0].material.ProperName());
			}

			// Ingredients
			sb.Append(". ");
			sb.Append((string)STRINGS.ONIACCESS.CODEX.REQUIRES);
			sb.Append(' ');
			AppendItemList(sb, recipe.ingredients.Length,
				i => recipe.ingredients[i].material,
				i => GameUtil.GetFormattedByTag(recipe.ingredients[i].material, recipe.ingredients[i].amount));

			// Results
			sb.Append(". ");
			sb.Append((string)STRINGS.ONIACCESS.CODEX.PRODUCES);
			sb.Append(' ');
			AppendItemList(sb, recipe.results.Length,
				i => recipe.results[i].material,
				i => GameUtil.GetFormattedByTag(recipe.results[i].material, recipe.results[i].amount));

			// Fabricator + time
			if (recipe.fabricators.Count > 0) {
				var fab = Assets.GetPrefab(recipe.fabricators[0].Name.ToTag());
				if (fab != null) {
					bool isSameArticle = currentEntryId != null &&
						recipe.fabricators[0].Name.ToUpper() == currentEntryId;
					sb.Append(". ");
					if (!isSameArticle && !useFabTitle) {
						sb.Append((string)STRINGS.ONIACCESS.CODEX.MADE_IN);
						sb.Append(' ');
						sb.Append(fab.GetProperName());
						sb.Append(", ");
					}
					sb.Append((string)STRINGS.ONIACCESS.CODEX.TIME);
					sb.Append(' ');
					sb.Append(GameUtil.GetFormattedTime(recipe.time));
				}
			}

			return sb.ToString();
		}

		private static string BuildSimpleRecipeSpeech(Recipe recipe) {
			var sb = new StringBuilder();
			sb.Append(recipe.Result.ProperName());

			sb.Append(". ");
			AppendItemList(sb, recipe.Ingredients.Count,
				i => recipe.Ingredients[i].tag,
				i => GameUtil.GetFormattedByTag(recipe.Ingredients[i].tag, recipe.Ingredients[i].amount));

			return sb.ToString();
		}

		private static List<(string id, string text)> GetRecipeLinks(CodexRecipePanel panel) {
			var links = new List<(string, string)>();
			var t = Traverse.Create(panel);
			var complexRecipe = t.Field<ComplexRecipe>("complexRecipe").Value;
			var simpleRecipe = t.Field<Recipe>("recipe").Value;

			if (complexRecipe != null) {
				foreach (var ing in complexRecipe.ingredients)
					AddTagLink(links, ing.material);
				foreach (var res in complexRecipe.results)
					AddTagLink(links, res.material);
				if (complexRecipe.fabricators.Count > 0) {
					var fab = Assets.GetPrefab(complexRecipe.fabricators[0].Name.ToTag());
					if (fab != null)
						AddNameLink(links, fab.GetProperName());
				}
			} else if (simpleRecipe != null) {
				foreach (var ing in simpleRecipe.Ingredients)
					AddTagLink(links, ing.tag);
				AddTagLink(links, simpleRecipe.Result);
			}

			return links;
		}

		// ========================================
		// CONVERSION PANEL
		// ========================================

		private static string GetConversionSpeech(CodexConversionPanel panel, string currentEntryId) {
			var t = Traverse.Create(panel);
			string title = t.Field<string>("title").Value;
			var ins = t.Field<ElementUsage[]>("ins").Value;
			var outs = t.Field<ElementUsage[]>("outs").Value;
			var converter = t.Field<UnityEngine.GameObject>("Converter").Value;

			string converterName = converter?.GetProperName();

			var sb = new StringBuilder();
			if (!string.IsNullOrEmpty(title)) {
				string firstInputName = (ins != null && ins.Length > 0 && ins[0].tag != Tag.Invalid)
					? ins[0].tag.ProperName() : null;
				if (title != firstInputName && title != converterName)
					sb.Append(title);
			}

			if (ins != null && ins.Length > 0) {
				if (sb.Length > 0) sb.Append(". ");
				AppendItemList(sb, ins.Length,
					i => ins[i].tag,
					i => {
						var timeSlice = ins[i].continuous ? GameUtil.TimeSlice.PerCycle : GameUtil.TimeSlice.None;
						return ins[i].customFormating != null
							? ins[i].customFormating(ins[i].tag, ins[i].amount, ins[i].continuous)
							: GameUtil.GetFormattedByTag(ins[i].tag, ins[i].amount, timeSlice);
					},
					skip: i => ins[i].tag == Tag.Invalid);
			}

			if (outs != null && outs.Length > 0) {
				if (sb.Length > 0) sb.Append(". ");
				sb.Append((string)STRINGS.ONIACCESS.CODEX.PRODUCES);
				sb.Append(' ');
				AppendItemList(sb, outs.Length,
					i => outs[i].tag,
					i => {
						var timeSlice = outs[i].continuous ? GameUtil.TimeSlice.PerCycle : GameUtil.TimeSlice.None;
						return outs[i].customFormating != null
							? outs[i].customFormating(outs[i].tag, outs[i].amount, outs[i].continuous)
							: GameUtil.GetFormattedByTag(outs[i].tag, outs[i].amount, timeSlice);
					},
					skip: i => outs[i].tag == Tag.Invalid);
			}

			if (converterName != null && !IsConverterSameArticle(converter, currentEntryId)) {
				sb.Append(". ");
				sb.Append(converterName);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Whether the converter GameObject belongs to the article currently being viewed.
		/// Matches both standalone entries (buildings) and SubEntries (critter morphs
		/// whose parent entry is the active article).
		/// </summary>
		private static bool IsConverterSameArticle(UnityEngine.GameObject converter, string currentEntryId) {
			if (currentEntryId == null) return false;
			var kpid = converter.GetComponent<KPrefabID>();
			if (kpid == null) return false;
			string prefabId = kpid.PrefabTag.Name;
			if (prefabId.ToUpper() == currentEntryId) return true;
			if (CodexCache.subEntries.TryGetValue(prefabId, out var sub))
				return sub.parentEntryID.ToUpper() == currentEntryId;
			return false;
		}

		private static List<(string id, string text)> GetConversionLinks(CodexConversionPanel panel) {
			var links = new List<(string, string)>();
			var t = Traverse.Create(panel);
			var ins = t.Field<ElementUsage[]>("ins").Value;
			var outs = t.Field<ElementUsage[]>("outs").Value;
			var converter = t.Field<UnityEngine.GameObject>("Converter").Value;

			if (ins != null)
				foreach (var eu in ins)
					if (eu.tag != Tag.Invalid) AddTagLink(links, eu.tag);
			if (outs != null)
				foreach (var eu in outs)
					if (eu.tag != Tag.Invalid) AddTagLink(links, eu.tag);
			if (converter != null)
				AddNameLink(links, converter.GetProperName());

			return links;
		}

		// ========================================
		// TEMPERATURE TRANSITION PANEL
		// ========================================

		private static string GetTemperatureTransitionSpeech(CodexTemperatureTransitionPanel panel) {
			var t = Traverse.Create(panel);
			var source = t.Field<Element>("sourceElement").Value;
			var type = t.Field<CodexTemperatureTransitionPanel.TransitionType>("transitionType").Value;
			if (source == null) return null;

			var sb = new StringBuilder();
			sb.Append(source.name);

			switch (type) {
				case CodexTemperatureTransitionPanel.TransitionType.HEAT:
					sb.Append(", ");
					sb.Append(GameUtil.GetFormattedTemperature(source.highTemp));
					AppendTransitionResult(sb, source.highTempTransition, source.highTempTransitionOreID, source.highTempTransitionOreMassConversion);
					break;
				case CodexTemperatureTransitionPanel.TransitionType.COOL:
					sb.Append(", ");
					sb.Append(GameUtil.GetFormattedTemperature(source.lowTemp));
					AppendTransitionResult(sb, source.lowTempTransition, source.lowTempTransitionOreID, source.lowTempTransitionOreMassConversion);
					break;
				case CodexTemperatureTransitionPanel.TransitionType.SUBLIMATE:
				case CodexTemperatureTransitionPanel.TransitionType.OFFGASS: {
						string label = type == CodexTemperatureTransitionPanel.TransitionType.SUBLIMATE
							? STRINGS.CODEX.FORMAT_STRINGS.SUBLIMATION_NAME
							: STRINGS.CODEX.FORMAT_STRINGS.OFFGASS_NAME;
						sb.Append(", ");
						sb.Append(label);
						var result = ElementLoader.FindElementByHash(source.sublimateId);
						if (result != null) {
							sb.Append(". ");
							sb.Append((string)STRINGS.ONIACCESS.CODEX.PRODUCES);
							sb.Append(' ');
							sb.Append(result.name);
						}
						break;
					}
			}

			return sb.ToString();
		}

		private static void AppendTransitionResult(StringBuilder sb, Element primary, SimHashes secondaryHash, float secondaryMassConversion) {
			if (primary == null) return;
			sb.Append(". ");
			sb.Append((string)STRINGS.ONIACCESS.CODEX.PRODUCES);
			sb.Append(' ');
			sb.Append(primary.name);

			var secondary = ElementLoader.FindElementByHash(secondaryHash);
			if (secondary != null) {
				sb.Append(", ");
				sb.Append(secondary.name);
			}
		}

		// ========================================
		// CONFIGURABLE CONSUMER RECIPE PANEL
		// ========================================

		private static string GetConsumerRecipeSpeech(CodexConfigurableConsumerRecipePanel panel) {
			var data = Traverse.Create(panel).Field<IConfigurableConsumerOption>("data").Value;
			if (data == null) return null;

			var sb = new StringBuilder();
			sb.Append(data.GetName());

			string desc = data.GetDescription();
			if (!string.IsNullOrEmpty(desc)) {
				sb.Append(". ");
				sb.Append(desc);
			}

			var ingredients = data.GetIngredients();
			if (ingredients != null && ingredients.Length > 0) {
				sb.Append(". ");
				AppendItemList(sb, ingredients.Length,
					i => ingredients[i].GetIDSets()[0],
					i => GameUtil.GetFormattedByTag(ingredients[i].GetIDSets()[0], ingredients[i].GetAmount()),
					skip: i => ingredients[i].GetIDSets().Length == 0);
			}

			return sb.ToString();
		}

		// ========================================
		// ELEMENT CATEGORY LIST
		// ========================================

		/// <summary>
		/// Get the items for a CodexElementCategoryList: header label + individual elements.
		/// Returns (text, isHeader) pairs for flattening into the content cursor.
		/// </summary>
		internal static List<(string text, bool isHeader)> GetElementCategoryItems(CodexElementCategoryList widget) {
			var items = new List<(string, bool)>();

			// Header text from the parent CodexCollapsibleHeader
			string headerLabel = Traverse.Create(widget).Field<string>("label").Value;
			if (!string.IsNullOrEmpty(headerLabel))
				items.Add((headerLabel, true));

			// Element items from Assets.GetPrefabsWithTag at speech time
			var prefabs = Assets.GetPrefabsWithTag(widget.categoryTag);
			foreach (var prefab in prefabs) {
				items.Add((prefab.GetProperName(), false));
			}

			return items;
		}

		// ========================================
		// GROUPED CONVERTER EFFECTS
		// ========================================

		/// <summary>
		/// For buildings with multiple ElementConverters, returns grouped per-converter
		/// speech items that pair each input with its outputs and temperature info.
		/// Returns null if not applicable (not a building, single converter, etc.).
		/// The int is the count of individual output items being replaced.
		/// </summary>
		internal static (List<string> items, int replacementCount)? GetGroupedConverterItems(string entryId) {
			var buildingDef = Assets.GetBuildingDef(entryId);
			if (buildingDef == null) return null;

			var converters = buildingDef.BuildingComplete.GetComponents<ElementConverter>();
			if (converters == null || converters.Length <= 1) return null;

			var groupedItems = new List<string>();
			int outputCount = 0;

			foreach (var converter in converters) {
				if (!converter.showDescriptors) continue;

				// Count active outputs for replacement tracking
				foreach (var output in converter.outputElements) {
					if (output.IsActive) outputCount++;
				}

				var sb = new StringBuilder();

				// Inputs
				bool firstInput = true;
				foreach (var input in converter.consumedElements) {
					if (!input.IsActive) continue;
					if (!firstInput) sb.Append(". ");
					firstInput = false;
					sb.Append(input.Name);
					sb.Append(", ");
					sb.Append(GameUtil.GetFormattedMass(input.MassConsumptionRate, GameUtil.TimeSlice.PerSecond));
				}

				// Outputs
				bool firstOutput = true;
				foreach (var output in converter.outputElements) {
					if (!output.IsActive) continue;
					if (firstOutput) {
						sb.Append(". ");
						sb.Append((string)STRINGS.ONIACCESS.CODEX.PRODUCES);
						sb.Append(' ');
						firstOutput = false;
					} else {
						sb.Append(". ");
					}
					sb.Append(output.Name);
					sb.Append(", ");
					sb.Append(GameUtil.GetFormattedMass(output.massGenerationRate, GameUtil.TimeSlice.PerSecond));
					sb.Append(", ");
					if (output.useEntityTemperature)
						sb.Append((string)STRINGS.ONIACCESS.CODEX.AT_BUILDING_TEMPERATURE);
					else if (output.minOutputTemperature > 0f)
						sb.Append(string.Format((string)STRINGS.ONIACCESS.CODEX.AT_LEAST_TEMPERATURE,
							GameUtil.GetFormattedTemperature(output.minOutputTemperature)));
					else
						sb.Append((string)STRINGS.ONIACCESS.CODEX.AT_INPUT_TEMPERATURE);
				}

				if (sb.Length > 0)
					groupedItems.Add(sb.ToString());
			}

			if (groupedItems.Count == 0) return null;
			return (groupedItems, outputCount);
		}

		// ========================================
		// LINK HELPERS
		// ========================================

		private static void AddTagLink(List<(string id, string text)> links, Tag tag) {
			string name = tag.ProperName();
			string id = CodexCache.FormatLinkID(name);
			links.Add((id, name));
		}

		private static void AddNameLink(List<(string id, string text)> links, string name) {
			string id = CodexCache.FormatLinkID(name);
			links.Add((id, name));
		}
	}
}
