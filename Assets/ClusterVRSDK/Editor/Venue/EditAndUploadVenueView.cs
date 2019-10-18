using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor.Venue
{
    public class EditAndUploadVenueView
    {
        readonly EditVenueView editVenueView;
        readonly UploadVenueView uploadVenueView;

        public EditAndUploadVenueView(UserInfo userInfo, Core.Editor.Venue.Json.Venue venue, Action venueChangeCallback)
        {
            Assert.IsNotNull(venue);

            editVenueView = new EditVenueView(userInfo, venue, venueChangeCallback);
            uploadVenueView = new UploadVenueView(userInfo, venue);
        }

        public void AddView(VisualElement parent)
        {
            parent.Add(new IMGUIContainer(() =>
            {
                Process();
                DrawUI();
            }));
        }

        void Process()
        {
            editVenueView.Process();
            uploadVenueView.Process();
        }

        enum Tab
        {
            Edit,
            Upload
        }

        Tab currentTab;

        void DrawUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                currentTab = (Tab) GUILayout.Toolbar((int) currentTab, new[] {"会場の設定", "アップロード"});
                GUILayout.FlexibleSpace();
            }

            if (currentTab == Tab.Edit)
            {
                editVenueView.DrawUI();
            }
            else
            {
                uploadVenueView.DrawUI();
            }
        }
    }
}
