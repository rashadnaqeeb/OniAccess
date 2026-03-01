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
			string raw;
			if (widget is CodexText ct)
				raw = GetCodexTextSpeech(ct);
			else if (widget is CodexTextWithTooltip ctwt)
				raw = GetCodexTextWithTooltipSpeech(ctwt);
			else if (widget is CodexLabelWithLargeIcon clli)
				raw = clli.label?.text;
			else if (widget is CodexLabelWithIcon cli)
				raw = cli.label?.text;
			else if (widget is CodexIndentedLabelWithIcon cili)
				raw = cili.label?.text;
			else if (widget is CodexRecipePanel crp)
				raw = GetRecipeSpeech(crp, currentEntryId);
			else if (widget is CodexConversionPanel ccp)
				raw = GetConversionSpeech(ccp);
			else if (widget is CodexTemperatureTransitionPanel cttp)
				raw = GetTemperatureTransitionSpeech(cttp);
			else if (widget is CodexConfigurableConsumerRecipePanel ccrp)
				raw = GetConsumerRecipeSpeech(ccrp);
			else if (widget is CodexCollapsibleHeader cch)
				raw = GetCollapsibleHeaderSpeech(cch);
			else if (widget is CodexVideo cv)
				raw = GetVideoSpeech(cv);
			else if (widget is CodexContentLockedIndicator)
				raw = (string)STRINGS.ONIACCESS.CODEX.LOCKED_CONTENT;
			else
				// Skip visual-only widgets: CodexImage, CodexDividerLine,
				// CodexSpacer, CodexLargeSpacer, CodexCritterLifecycleWidget
				return null;

			return Widgets.WidgetOps.CleanTooltipEntry(raw);
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
					if (!isSameArticle) {
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

		private static string GetConversionSpeech(CodexConversionPanel panel) {
			var t = Traverse.Create(panel);
			string title = t.Field<string>("title").Value;
			var ins = t.Field<ElementUsage[]>("ins").Value;
			var outs = t.Field<ElementUsage[]>("outs").Value;
			var converter = t.Field<UnityEngine.GameObject>("Converter").Value;

			var sb = new StringBuilder();
			if (!string.IsNullOrEmpty(title))
				sb.Append(title);

			if (ins != null && ins.Length > 0) {
				sb.Append(". ");
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
				sb.Append(". ");
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

			if (converter != null) {
				sb.Append(". ");
				sb.Append(converter.GetProperName());
			}

			return sb.ToString();
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
