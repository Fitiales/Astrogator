using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astrogator {

	using static DebugTools;
	using static ViewTools;

	/// <summary>
	/// A DialogGUI* object that displays our app's data.
	/// Intended for embedding in a MultiOptionDialog.
	/// </summary>
	public class AstrogationView : DialogGUIVerticalLayout {
		private AstrogationModel model { get; set; }
		private PopupDialog dialog { get; set; }
		private static Rect geometry {
			get {
				Vector2 pos = Settings.Instance.MainWindowPosition;
				return new Rect(pos.x, pos.y, mainWindowMinWidth, mainWindowMinHeight);
			}
			set {
				Settings.Instance.MainWindowPosition =
					new Vector2(value.x, value.y);
			}
		}

		/// <summary>
		/// The user-facing name for this mod.
		/// Use Astrogator.Name for filenames, internal representations, CKAN, etc.
		/// </summary>
		public const string DisplayName = "Astrogator";

		/// <summary>
		/// UI object representing the top row of the table
		/// </summary>
		private static DialogGUIHorizontalLayout ColumnHeaders { get; set; }

		/// <summary>
		/// Construct a view for the given model.
		/// </summary>
		/// <param name="m">Model object for which to make a view</param>
		public AstrogationView(AstrogationModel m)
			: base(
				mainWindowMinWidth,
				mainWindowMinHeight,
				mainWindowSpacing,
				mainWindowPadding,
				TextAnchor.UpperCenter
			)
		{
			model = m;

			createHeaders();
			createRows();
		}

		private void createHeaders()
		{
			if (ColumnHeaders == null) {
				ColumnHeaders = new DialogGUIHorizontalLayout();
				for (int i = 0; i < Columns.Length; ++i) {
					ColumnDefinition col = Columns[i];
					// Skip columns that require an active vessel if we don't have one
					if (!col.vesselSpecific || model.vessel != null) {
						int width = 0;
						for (int span = 0; span < col.headerColSpan; ++span) {
							width += Columns[i + span].width;
						}
						if (width > 0) {
							ColumnHeaders.AddChild(LabelWithStyleAndSize(col.header, col.headerStyle, width, rowHeight));
						}
					}
				}
			}
			AddChild(ColumnHeaders);
		}

		private void createRows()
		{
			for (int i = 0; i < model.transfers.Count; ++i) {
				AddChild(new TransferView(model.transfers[i]));
			}
		}

		/// <summary>
		/// Launch a PopupDialog containing the view.
		/// Use Dismiss() to get rid of it.
		/// </summary>
		public PopupDialog Show()
		{
			return dialog = PopupDialog.SpawnPopupDialog(
				mainWindowAnchor,
				mainWindowAnchor,
				new MultiOptionDialog(
					String.Format("Transfers from {0}", model.OriginDescription()),
					DisplayName,
					AstrogatorSkin,
					geometry,
					this
				),
				false,
				AstrogatorSkin,
				false
			);
		}

		/// <summary>
		/// Close the popup.
		/// </summary>
		public void Dismiss()
		{
			if (dialog != null) {
				Vector3 rt = dialog.RTrf.position;
				DbgFmt("Rect transform: {0}", rt.ToString());
				geometry = new Rect(
					rt.x / Screen.width  + 0.5f,
					rt.y / Screen.height + 0.5f,
					mainWindowMinWidth,
					mainWindowMinHeight
				);
				dialog.Dismiss();
				dialog = null;
			}
		}
	}

}
