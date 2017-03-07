using System;
using UnityEngine;

namespace Astrogator {

	using static DebugTools;
	using static ViewTools;

	/// <summary>
	/// A GUI object allowing the user to edit the settings.
	/// </summary>
	public class SettingsView : DialogGUIVerticalLayout {

		private const string docsURL = "https://github.com/HebaruSan/Astrogator/blob/master/README.md#settings";

		/// <summary>
		/// Construct a GUI object that allows the user to edit the settings.
		/// </summary>
		public SettingsView(AstrogationView.ResetCallback reset)
			: base(
				mainWindowMinWidth, 10,
				settingsSpacing,    settingsPadding,
				TextAnchor.UpperLeft
			)
		{
			resetCallback = reset;

			try {

				AddChild(headerButton(
					"Click for online manual",
					linkStyle, "The meaning of each setting is explained in the README.md file", RowWidth, rowHeight,
					() => { Application.OpenURL(docsURL); }
				));

				AddChild(LabelWithStyleAndSize(
					"Settings:",
					midHdrStyle,
					mainWindowMinWidth, rowHeight
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.GeneratePlaneChangeBurns,
					"Generate plane change burns",
					(bool b) => { Settings.Instance.GeneratePlaneChangeBurns = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AddPlaneChangeDeltaV,
					"Add plane change burns to Δv column",
					(bool b) => {
						Settings.Instance.AddPlaneChangeDeltaV = b;
						// Only need to reload if we don't already have the plane change values
						if (b) {
							resetCallback(true);
						}
					}
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.DeleteExistingManeuvers,
					"Auto-delete user-created maneuver nodes",
					(bool b) => { Settings.Instance.DeleteExistingManeuvers = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.ShowTrackedAsteroids,
					"Calculate transfers to tracked asteroids",
					(bool b) => { Settings.Instance.ShowTrackedAsteroids = b; resetCallback(true); }
				));

				AddChild(LabelWithStyleAndSize(
					"Maneuver creation:",
					midHdrStyle,
					mainWindowMinWidth, rowHeight
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AutoTargetDestination,
					"Automatically target destination",
					(bool b) => { Settings.Instance.AutoTargetDestination = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AutoFocusDestination,
					"Automatically focus destination",
					(bool b) => { Settings.Instance.AutoFocusDestination = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AutoEditEjectionNode,
					"Automatically edit ejection node",
					(bool b) => { Settings.Instance.AutoEditEjectionNode = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AutoEditPlaneChangeNode,
					"Automatically edit plane change node",
					(bool b) => { Settings.Instance.AutoEditPlaneChangeNode = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.AutoSetSAS,
					"Automatically set SAS to maneuver mode",
					(bool b) => { Settings.Instance.AutoSetSAS = b; }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.TranslationAdjust,
					"Adjust nodes with translation controls when RCS is off",
					(bool b) => { Settings.Instance.TranslationAdjust = b; }
				));

				AddChild(LabelWithStyleAndSize(
					"Units:",
					midHdrStyle,
					mainWindowMinWidth, rowHeight
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.DisplayUnits == DisplayUnitsEnum.Metric,
					"Système International d'Unités (Metric)",
					(bool b) => { if (b) Settings.Instance.DisplayUnits = DisplayUnitsEnum.Metric; resetCallback(false); }
				));

				AddChild(new DialogGUIToggle(
					() => Settings.Instance.DisplayUnits == DisplayUnitsEnum.UnitedStatesCustomary,
					"United States Customary (Imperial)",
					(bool b) => { if (b) Settings.Instance.DisplayUnits = DisplayUnitsEnum.UnitedStatesCustomary; resetCallback(false); }
				));

			} catch (Exception ex) {
				DbgExc("Problem constructing settings view", ex);
			}
		}

		private AstrogationView.ResetCallback resetCallback { get; set; }

	}

}
