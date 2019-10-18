using ClusterVRSDK.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor
{
    public class VenueUploadWindow : EditorWindow
    {
        [MenuItem("clusterSDK/VenueUpload")]
        public static void Open()
        {
            var window = GetWindow<VenueUploadWindow>();
            window.titleContent = new GUIContent("cluster UploadVenueWindow");
        }

        void OnEnable()
        {
            var tokenAuth = new TokenAuthWidget();
            tokenAuth.AddView(rootVisualElement);
            rootVisualElement.Add(UiUtils.Separator());

            VisualElement loggedInUiContainer = null;
            ReactiveBinder.Bind(tokenAuth.reactiveUserInfo, userInfo =>
            {
                if (loggedInUiContainer != null)
                {
                    rootVisualElement.Remove(loggedInUiContainer);
                }

                if (userInfo.HasValue)
                {
                    loggedInUiContainer = CreateVenueUi(userInfo.Value);
                    rootVisualElement.Add(loggedInUiContainer);
                }
                else
                {
                    loggedInUiContainer = null;
                }
            });
        }

        VisualElement CreateVenueUi(UserInfo userInfo)
        {
            var selectVenue = new SelectVenueView(userInfo);

            var container = new VisualElement();
            var editAndUploadContainer = new VisualElement();
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            {
                selectVenue.AddView(scrollView);
                scrollView.Add(UiUtils.Separator());
                scrollView.Add(editAndUploadContainer);
            }
            container.Add(scrollView);
            container.Add(UiUtils.Separator());

            var previewVenueView = new PreviewVenueView(selectVenue.reactiveCurrentVenue);
            previewVenueView.AddView(container);

            ReactiveBinder.Bind(selectVenue.reactiveCurrentVenue, currentVenue =>
            {
                editAndUploadContainer.Clear();
                if (currentVenue != null)
                {
                    new EditAndUploadVenueView(userInfo, currentVenue, () =>
                    {
                        selectVenue.RefetchVenueWithoutChangingSelection();
                    }).AddView(editAndUploadContainer);
                }
            });

            return container;
        }
    }
}
