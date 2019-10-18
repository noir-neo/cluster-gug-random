using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterVRSDK.Core.Editor.Venue;
using ClusterVRSDK.Core.Editor.Venue.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor.Venue
{
    public class SelectVenueView
    {
        public readonly Reactive<Core.Editor.Venue.Json.Venue> reactiveCurrentVenue = new Reactive<Core.Editor.Venue.Json.Venue>();

        readonly UserInfo userInfo;
        readonly Dictionary<GroupID, Venues> allVenues = new Dictionary<GroupID, Venues>();

        VisualElement selector;

        public SelectVenueView(UserInfo userInfo)
        {
            this.userInfo = userInfo;
        }

        public void AddView(VisualElement parent)
        {
            selector = new VisualElement();
            var venueInfo = new VisualElement();
            parent.Add(selector);
            parent.Add(venueInfo);

            ReactiveBinder.Bind(reactiveCurrentVenue, venue =>
            {
                venueInfo.Clear();
                if (venue == null)
                {
                    return;
                }

                var thumbnailView = new DrawThumbnailView();
                venueInfo.Add(new IMGUIContainer(() =>
                {
                    EditorGUILayout.LabelField("説明");
                    EditorGUILayout.HelpBox(venue.Description, MessageType.None);

                    if (venue.ThumbnailUrls.Any())
                    {
                        thumbnailView.OverwriteDownloadUrl(venue.ThumbnailUrls.First(x => x != null));
                    }
                    thumbnailView.DrawUI(false);
                }));
            });

            RefreshVenueSelector();
        }

        public void RefetchVenueWithoutChangingSelection()
        {
            var currentVenue = reactiveCurrentVenue.Val;
            if (currentVenue != null)
            {
                RefreshVenueSelector(currentVenue.Group.Id, currentVenue.VenueId);
            }
            else
            {
                RefreshVenueSelector();
            }
        }

        async Task RefreshVenueSelector(GroupID groupIdToSelect = null, VenueID venueIdToSelect = null)
        {
            selector.Clear();
            selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox("会場情報を取得しています...", MessageType.None)));

            VisualElement venuePicker = null;
            void RecreateVenuePicker(GroupID groupId)
            {
                if (venuePicker != null)
                {
                    selector.Remove(venuePicker);
                }

                venuePicker = CreateVenuePicker(groupId, allVenues[groupId], venueIdToSelect);
                selector.Add(venuePicker);
            }

            try
            {
                var groups = await APIServiceClient.GetGroups.Call(Empty.Value, userInfo.VerifiedToken, 3);
                foreach (var group in groups.List)
                {
                    allVenues[group.Id] = await APIServiceClient.GetGroupVenues.Call(group.Id, userInfo.VerifiedToken, 3);
                }

                selector.Clear();
                if (groups.List.Count == 0)
                {
                    selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox("clusterにてチーム登録をお願いいたします", MessageType.Warning)));
                }
                else
                {
                    var teamMenu = new PopupField<Group>("所属チーム", groups.List, 0, group => group.Name, group => group.Name);
                    teamMenu.RegisterValueChangedCallback(ev => RecreateVenuePicker(ev.newValue.Id));
                    selector.Add(teamMenu);

                    var groupToSelect = groups.List.Find(group => group.Id == groupIdToSelect) ?? groups.List[0];
                    teamMenu.SetValueWithoutNotify(groupToSelect);

                    RecreateVenuePicker(groupToSelect.Id);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                selector.Clear();
                selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox($"会場情報の取得に失敗しました {e.Message}", MessageType.Error)));
            }
        }

        VisualElement CreateVenuePicker(GroupID groupId, Venues venues, VenueID venueIdToSelect = null)
        {
            var container = new VisualElement()
            {
                style = {flexDirection = FlexDirection.Row}
            };

            if (venues.List.Count != 0)
            {
                var dupedVenueNames = new HashSet<string>(
                    venues.List.GroupBy(venue => venue.Name, venue => venue.VenueId, (name, ids) => ids.Count() >= 2 ? name : "").Where(name => name != ""));
                string GetUniqueVenueName(Core.Editor.Venue.Json.Venue venue)
                {
                    return dupedVenueNames.Contains(venue.Name) ? $"{venue.Name} ({venue.VenueId.Value})" : venue.Name;
                }

                var venueMenu = new PopupField<Core.Editor.Venue.Json.Venue>("会場一覧", venues.List, 0, GetUniqueVenueName, GetUniqueVenueName);
                venueMenu.style.flexGrow = 1f;
                venueMenu.RegisterValueChangedCallback(ev => { reactiveCurrentVenue.Val = ev.newValue; });
                container.Add(venueMenu);

                var venueToSelect = venues.List.Find(venue => venue.VenueId == venueIdToSelect) ?? venues.List[0];
                venueMenu.SetValueWithoutNotify(venueToSelect);

                reactiveCurrentVenue.Val = venueToSelect;
            }
            else
            {
                reactiveCurrentVenue.Val = null;
            }

            container.Add(new Button(() => CreateNewVenue(groupId))
            {
                text = "新規会場追加"
            });
            return container;
        }

        void CreateNewVenue(GroupID groupId)
        {
            var newVenuePayload = new PostNewVenuePayload
            {
                description = "説明未設定",
                name = "NewVenue",
                groupId = groupId.Value,
            };

            var postVenueService =
                new PostRegisterNewVenueService(
                    userInfo.VerifiedToken,
                    newVenuePayload,
                    venue =>
                    {
                        RefreshVenueSelector();
                        reactiveCurrentVenue.Val = venue;
                    },
                    exception =>
                    {
                        Debug.LogException(exception);
                        selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox($"新規会場の登録ができませんでした。{exception.Message}", MessageType.Error)));
                    });
            postVenueService.Run();
        }
    }
}
