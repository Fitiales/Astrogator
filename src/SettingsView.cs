using System;
using UnityEngine;
using KSP.Localization;

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
					Localizer.Format("astrogator_manualLink"),
					linkStyle, Localizer.Format("astrogator_manualLinkTooltip"), RowWidth, rowHeight,
					() => { Application.OpenURL(docsURL); }
				));

				AddChild(LabelWithStyleAndSize(
					Localizer.Format("astrogator_settingsSectionHeader"),
					midHdrStyle,
					mainWindowMinWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.GeneratePlaneChangeBurns,
					Localizer.Format("astrogator_planeChangeBurnsSetting"),
					(bool b) => { Settings.Instance.GeneratePlaneChangeBurns = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AddPlaneChangeDeltaV,
					Localizer.Format("astrogator_addChangeBurnsSetting"),
					(bool b) => {
						Settings.Instance.AddPlaneChangeDeltaV = b;
						// Only need to reload if we don't already have the plane change values
						if (b) {
							resetCallback(true);
						}
					},
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.DeleteExistingManeuvers,
					Localizer.Format("astrogator_autoDeleteNodesSetting"),
					(bool b) => { Settings.Instance.DeleteExistingManeuvers = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.ShowTrackedAsteroids,
					Localizer.Format("astrogator_asteroidsSetting"),
					(bool b) => { Settings.Instance.ShowTrackedAsteroids = b; resetCallback(true); },
					mainWindowInternalWidth
				));

				AddChild(LabelWithStyleAndSize(
					Localizer.Format("astrogator_maneuverCreationHeader"),
					midHdrStyle,
					mainWindowMinWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AutoTargetDestination,
					Localizer.Format("astrogator_autoTargetDestSetting"),
					(bool b) => { Settings.Instance.AutoTargetDestination = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AutoFocusDestination,
					Localizer.Format("astrogator_autoFocusDestSetting"),
					(bool b) => { Settings.Instance.AutoFocusDestination = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AutoEditEjectionNode,
					Localizer.Format("astrogator_autoEditEjecSetting"),
					(bool b) => { Settings.Instance.AutoEditEjectionNode = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AutoEditPlaneChangeNode,
					Localizer.Format("astrogator_autoEditPlaneChgSetting"),
					(bool b) => { Settings.Instance.AutoEditPlaneChangeNode = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.AutoSetSAS,
					Localizer.Format("astrogator_autoSetSASSetting"),
					(bool b) => { Settings.Instance.AutoSetSAS = b; },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.TranslationAdjust,
					Localizer.Format("astrogator_adjustNodesSetting"),
					(bool b) => { Settings.Instance.TranslationAdjust = b; },
					mainWindowInternalWidth
				));

				AddChild(LabelWithStyleAndSize(
					Localizer.Format("astrogator_unitsHeader"),
					midHdrStyle,
					mainWindowMinWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.DisplayUnits == DisplayUnitsEnum.Metric,
					Localizer.Format("astrogator_metricSetting"),
					(bool b) => { if (b) Settings.Instance.DisplayUnits = DisplayUnitsEnum.Metric; resetCallback(false); },
					mainWindowInternalWidth
				));

				AddChild(new WrappingToggle(
					() => Settings.Instance.DisplayUnits == DisplayUnitsEnum.UnitedStatesCustomary,
					Localizer.Format("astrogator_imperialSetting"),
					(bool b) => { if (b) Settings.Instance.DisplayUnits = DisplayUnitsEnum.UnitedStatesCustomary; resetCallback(false); },
					mainWindowInternalWidth
				));

			} catch (Exception ex) {
				DbgExc("Problem constructing settings view", ex);
			}
		}

		private AstrogationView.ResetCallback resetCallback { get; set; }

	}

}
