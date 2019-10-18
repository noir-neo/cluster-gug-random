using System;
using ClusterVRSDK.Core.Editor;
using ClusterVRSDK.Core.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor
{
    public class TokenAuthWidget
    {
        [Obsolete("Use reactiveUserInfo")]
        public string VerifiedToken => reactiveUserInfo.Val.HasValue ? reactiveUserInfo.Val.Value.VerifiedToken : "";
        [Obsolete("Use reactiveUserInfo")]
        public string Username =>  reactiveUserInfo.Val.HasValue ? reactiveUserInfo.Val.Value.Username : "";
        [Obsolete("Use reactiveUserInfo")]
        public bool IsLoggedIn => reactiveUserInfo.Val.HasValue;

        public readonly Reactive<UserInfo?> reactiveUserInfo = new Reactive<UserInfo?>();
        readonly Reactive<string> reactiveMessage = new Reactive<string>();

        bool isLoggingIn;

        public void AddView(VisualElement parent)
        {
            parent.Add(new IMGUIContainer(() => EditorGUILayout.LabelField("APIアクセストークン", EditorStyles.boldLabel)));

            var accessToken = new TextField();
            accessToken.RegisterValueChangedCallback(ev =>
            {
                ValidateAndLogin(ev.newValue);
            });
            parent.Add(accessToken);

            parent.Add(
                new Button(() => Application.OpenURL(Constants.WebBaseUrl + "/app/my/tokens"))
                {
                    text = "トークンを入手"
                });

            var messageLabel = new Label();
            parent.Add(messageLabel);
            ReactiveBinder.Bind(this.reactiveMessage, msg => { messageLabel.text = msg; });

            // TODO: 他のwindowでloginしたときにも自動で同期する
            if (!string.IsNullOrEmpty(EditorPrefsUtils.SavedAccessToken))
            {
                accessToken.value = EditorPrefsUtils.SavedAccessToken;
            }

            // 初期状態 or 既存のトークンをvalidateして何かのメッセージを出すのに必要
            ValidateAndLogin(EditorPrefsUtils.SavedAccessToken);
        }

        void ValidateAndLogin(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                reactiveUserInfo.Val = null;
                reactiveMessage.Val = "アクセストークンが必要です";
                return;
            }

            if (token.Length != 64)
            {
                reactiveUserInfo.Val = null;
                reactiveMessage.Val = "不正なアクセストークンです";
                return;
            }

            // Call auth API
            if (isLoggingIn)
            {
                return;
            }
            isLoggingIn = true;
            var _ = APIServiceClient.GetMyUser.CallWithCallback(Empty.Value, token, user =>
            {
                isLoggingIn = false;

                reactiveUserInfo.Val = new UserInfo(user.Username, token);
                reactiveMessage.Val = "Logged in as " + "\"" + user.Username + "\"";

                EditorPrefsUtils.SavedAccessToken = token;
            }, exc =>
            {
                isLoggingIn = false;
            }, 3);
        }
    }
}
