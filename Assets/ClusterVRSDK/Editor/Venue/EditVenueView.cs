using System;
using System.Linq;
using ClusterVRSDK.Core.Editor.Venue;
using ClusterVRSDK.Core.Editor.Venue.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ClusterVRSDK.Editor.Venue
{
    public class EditVenueView
    {
        readonly UserInfo userInfo;
        readonly Core.Editor.Venue.Json.Venue venue;

        readonly Action venueChangeCallback;

        readonly DrawThumbnailView drawThumbnailView;

        EditVenue editVenue;

        public EditVenueView(UserInfo userInfo, Core.Editor.Venue.Json.Venue venue, Action venueChangeCallback)
        {
            Assert.IsNotNull(venue);

            this.userInfo = userInfo;
            this.venue = venue;

            this.venueChangeCallback = venueChangeCallback;

            editVenue = new EditVenue();
            drawThumbnailView = new DrawThumbnailView();
        }

        bool executeSaveVenue;
        bool savingVenueThumbnail;

        string errorMessage;

        public void Process()
        {
            if (executeSaveVenue)
            {
                executeSaveVenue = false;
                savingVenueThumbnail = true;

                var patchVenuePayload = new PatchVenuePayload
                {
                    description = editVenue.Description,
                    name = editVenue.Name,
                    thumbnailUrls = venue.ThumbnailUrls.ToList()
                };

                var patchVenueService =
                    new PatchVenueSettingService(
                        userInfo.VerifiedToken,
                        venue.VenueId,
                        patchVenuePayload,
                        editVenue.ThumbnailPath,
                        venue =>
                        {
                            editVenue = null;
                            savingVenueThumbnail = false;
                            venueChangeCallback();
                        },
                        exception =>
                        {
                            errorMessage = $"会場情報の保存に失敗しました。{exception.Message}";
                            savingVenueThumbnail = false;
                        });
                patchVenueService.Run();
                errorMessage = null;
            }
        }

        public void DrawUI()
        {
            if (editVenue == null)
            {
                return;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("名前");
                editVenue.Name = EditorGUILayout.TextField(editVenue.Name ?? venue.Name);
            }

            EditorGUILayout.LabelField("説明");
            var textAreaOption = new[] {GUILayout.MinHeight(64)};
            editVenue.Description = EditorGUILayout.TextArea(editVenue.Description ?? venue.Description, textAreaOption);

            if (string.IsNullOrEmpty(editVenue.ThumbnailPath) && venue.ThumbnailUrls.Any())
            {
                drawThumbnailView.OverwriteDownloadUrl(venue.ThumbnailUrls.First(x => x != null));
            }

            drawThumbnailView.DrawUI(savingVenueThumbnail);

            if (GUILayout.Button("サムネイル画像を選択..."))
            {
                editVenue.ThumbnailPath =
                    EditorUtility.OpenFilePanelWithFilters(
                        "画像を選択",
                        "",
                        new[] {"Image files", "png,jpg,jpeg", "All files", "*"}
                    );
                drawThumbnailView.OverwriteFilePath(editVenue.ThumbnailPath);
            }

            EditorGUILayout.Space();


            if (!savingVenueThumbnail)
            {
                executeSaveVenue = GUILayout.Button("保存");
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
    }
}
