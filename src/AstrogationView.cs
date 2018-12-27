using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KSP.Localization;

namespace Astrogator {

	using static DebugTools;
	using static KerbalTools;
	using static ViewTools;

	/// <summary>
	/// A DialogGUI* object that displays our app's data.
	/// Intended for embedding in a MultiOptionDialog.
	/// </summary>
	public class AstrogationView : DialogGUIVerticalLayout {

		/// <summary>
		/// Construct a view for the given model.
		/// </summary>
		/// <param name="m">Model object for which to make a view</param>
		/// <param name="reset">Function to call when the view needs to be re-initiated</param>
		/// <param name="close">Function to call when the user clicks a close button</param>
		public AstrogationView(AstrogationModel m, ResetCallback reset, UnityAction close)
			: base(
				FlightGlobals.ActiveVessel != null ? mainWindowMinWidthWithVessel : mainWindowMinWidthWithoutVessel,
				mainWindowMinHeight,
				mainWindowSpacing,
				mainWindowPadding,
				TextAnchor.UpperCenter
			)
		{
			model         = m;
			resetCallback = reset;
			closeCallback = close;

			int width = FlightGlobals.ActiveVessel != null ? RowWidthWithVessel : RowWidthWithoutVessel;

			if (Settings.Instance.ShowSettings) {
				AddChild(new SettingsView(resetCallback, width));
			} else if (!ErrorCondition) {
				createHeaders();
				createRows();
				AddChild(new DialogGUIHorizontalLayout(
					width, 10,
					0, wrenchPadding,
					TextAnchor.UpperRight,
					new DialogGUILabel(getMessage, notificationStyle, true, true)
				));
			}
		}

		private AstrogationModel model  { get; set; }
		private PopupDialog      dialog { get; set; }

		/// <summary>
		/// Type of function pointer used to request a re-creation of the UI.
		/// This is needed because the DialogGUI* functions don't allow us to
		/// make dynamic chnages to a UI beyond changing a label's text.
		/// </summary>
		public delegate void ResetCallback(bool resetModel = false);

		private ResetCallback resetCallback { get; set; }
		private UnityAction   closeCallback { get; set; }

		private void toggleSettingsVisible()
		{
			Settings.Instance.ShowSettings = !Settings.Instance.ShowSettings;
			resetCallback();
		}

		/// <summary>
		/// UI object representing the top row of the table
		/// </summary>
		private DialogGUIHorizontalLayout ColumnHeaders { get; set; }

		private string columnSortIndicator(ColumnDefinition col)
		{
			return col.sortKey != Settings.Instance.TransferSort ? ""
					: Settings.Instance.DescendingSort ? " v"
					: " ^";
		}

		private void createHeaders()
		{
			ColumnHeaders = new DialogGUIHorizontalLayout();
			for (int i = 0; i < Columns.Length; ++i) {
				ColumnDefinition col = Columns[i];
				// Skip columns that require an active vessel if we don't have one
				if (col.vesselSpecific && FlightGlobals.ActiveVessel == null) {
					continue;
				}
				if (col.requiresPatchedConics && (
					!patchedConicsUnlocked()
						|| !vesselControllable(FlightGlobals.ActiveVessel)
						|| model.origin == null
						|| Landed(model.origin)
				)) {
					continue;
				}
				float width = 0;
				for (int span = 0; span < col.headerColSpan; ++span) {
					width += Columns[i + span].width;
				}
				if (width > 0) {
					// Add in the spacing gaps that got left out from colspanning
					width += (col.headerColSpan - 1) * spacing;
					if (col.header != "") {
						ColumnHeaders.AddChild(headerButton(
							col.header + columnSortIndicator(col),
							col.headerStyle, Localizer.Format("astrogator_columnHeaderTooltip"), width, rowHeight, () => {
								SortClicked(col.sortKey);
							}
						));
					} else {
						ColumnHeaders.AddChild(new DialogGUISpace(width));
					}
				}
			}
			AddChild(ColumnHeaders);
		}

		private void SortClicked(SortEnum which)
		{
			if (Settings.Instance.TransferSort == which) {
				Settings.Instance.DescendingSort = !Settings.Instance.DescendingSort;
			} else {
				Settings.Instance.TransferSort = which;
				Settings.Instance.DescendingSort = false;
			}
			resetCallback();
		}

		private void createRows()
		{
			List<TransferModel> transfers = SortTransfers(
				model,
				Settings.Instance.TransferSort,
				Settings.Instance.DescendingSort
			);
			for (int i = 0; i < transfers.Count; ++i) {
				AddChild(new TransferView(transfers[i]));
			}
		}

		private bool ErrorCondition {
			get {
				return model == null
					|| model.origin == null
					|| model.transfers.Count == 0
					|| model.ErrorCondition;
			}
		}

		private string getMessage()
		{
			if (model.ActiveEjectionBurn != null
					&& Settings.Instance.TranslationAdjust
					&& FlightGlobals.ActiveVessel != null
					&& !FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS]) {
				return Localizer.Format("astrogator_translationControlsNotification");
			} else {
				return "";
			}
		}

		private UISkinDef skinToUse {
			get {
				if (!ErrorCondition) {
					return AstrogatorSkin;
				} else {
					return AstrogatorErrorSkin;
				}
			}
		}

		private UIStyle settingsToggleStyle {
			get {
				if (Settings.Instance.ShowSettings) {
					return backStyle;
				} else {
					return settingsStyle;
				}
			}
		}

		private string settingsToggleTooltip {
			get {
				if (Settings.Instance.ShowSettings) {
					return "astrogator_backButtonTooltip";
				} else {
					return "astrogator_settingsButtonTooltip";
				}
			}
		}

		private bool           needUIScaleOffsetUpdate = false;
		private static float   prevUIScale             = 1f;
		private static Vector2 uiScaleOffset           = Vector2.zero;

		private static Rect geometry {
			get {
				Vector2 pos = Settings.Instance.MainWindowPosition;
				return new Rect(
					pos.x / GameSettings.UI_SCALE,
					pos.y / GameSettings.UI_SCALE,
					FlightGlobals.ActiveVessel != null ? mainWindowMinWidthWithVessel : mainWindowMinWidthWithoutVessel,
					mainWindowMinHeight);
			}
			set {
				Settings.Instance.MainWindowPosition = new Vector2(
					value.x * GameSettings.UI_SCALE,
					value.y * GameSettings.UI_SCALE
				);
			}
		}

		private Rect currentGeometry {
			get {
				Vector3 rt = dialog.GetComponent<RectTransform>().position;
				return new Rect(
					rt.x / GameSettings.UI_SCALE / Screen.width  + 0.5f,
					rt.y / GameSettings.UI_SCALE / Screen.height + 0.5f,
					FlightGlobals.ActiveVessel != null ? mainWindowMinWidthWithVessel : mainWindowMinWidthWithoutVessel,
					mainWindowMinHeight);
			}
		}

		private void replaceScratchWindowWithRealView()
		{
			// Update the offsets and note the applicable UI Scale
			prevUIScale = GameSettings.UI_SCALE;
			uiScaleOffset = new Vector2(
				currentGeometry.x - geometry.x,
				currentGeometry.y - geometry.y
			);

			// Get rid of the scratch window
			Dismiss();

			// Open a new window using the new offset
			Show();
		}

		/// <summary>
		/// Launch a PopupDialog containing the view.
		/// Use Dismiss() to get rid of it.
		/// </summary>
		public PopupDialog Show()
		{
			if (dialog == null) {
				if (Math.Abs(prevUIScale - GameSettings.UI_SCALE) > 0.05) {
					// New UI Scale setting, so we can't open the real window yet.
					// Instead, we open a scratch window to see how much
					// distortion is applied at this level of scaling.
					dialog = PopupDialog.SpawnPopupDialog(
						mainWindowAnchorMin,
						mainWindowAnchorMax,
						new MultiOptionDialog("", "", "",
							AstrogatorSkin,
							geometry,
							new DialogGUIHorizontalLayout() {
								OnUpdate = () => {
									if (needUIScaleOffsetUpdate) {
										needUIScaleOffsetUpdate = false;
										replaceScratchWindowWithRealView();
									}
								}
							}
						),
						false,
						AstrogatorSkin,
						false
					);
					needUIScaleOffsetUpdate = true;

				} else {

					// Calculate where the new window should go based on the
					// difference between where we said the scratch window should
					// go and where it actually went.
					Rect offsetGeometry = geometry;
					offsetGeometry.x -= uiScaleOffset.x;
					offsetGeometry.y -= uiScaleOffset.y;

					dialog = PopupDialog.SpawnPopupDialog(
						mainWindowAnchorMin,
						mainWindowAnchorMax,
						new MultiOptionDialog(
							Localizer.Format("astrogator_mainTitle"),
							ModelDescription(model),
							Localizer.Format("astrogator_mainTitle") + " " + versionString,
							skinToUse,
							offsetGeometry,
							this
						),
						false,
						skinToUse,
						false
					);

					// Add the close button in the upper right corner after the PopupDialog has been created.
					AddFloatingButton(
						dialog.transform,
						-mainWindowPadding.right - mainWindowSpacing, -mainWindowPadding.top,
						closeStyle,
						"astrogator_closeButtonTooltip",
						closeCallback
					);

					// Add the settings button next to the close button.
					// If the settings are visible it's a back '<' icon, otherwise a wrench+screwdriver.
					AddFloatingButton(
						dialog.transform,
						-mainWindowPadding.right - 3 * mainWindowSpacing - buttonIconWidth,
						-mainWindowPadding.top,
						settingsToggleStyle,
						settingsToggleTooltip,
						toggleSettingsVisible
					);
				}
			}
			return dialog;
		}

		/// <summary>
		/// Close the popup.
		/// </summary>
		public void Dismiss()
		{
			if (dialog != null) {
				geometry = new Rect(
					currentGeometry.x, currentGeometry.y,
					FlightGlobals.ActiveVessel ? mainWindowMinWidthWithVessel : mainWindowMinWidthWithoutVessel,
					mainWindowMinHeight
				);
				dialog.Dismiss();
				dialog = null;
			}
		}
	}

}
