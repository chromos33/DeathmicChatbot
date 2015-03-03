#region Using

using System.Collections.Generic;
using System.Timers;
using DeathmicChatbot.Interfaces;

#endregion


namespace DeathmicChatbot
{
    internal static class UserMerger
    {
        private static readonly Dictionary<string, string> MergeRequests =
            new Dictionary<string, string>();

        public static void MergeUsers(IModel model,
                                      MessageQueue messageQueue,
                                      string requestingUser,
                                      string userToMergeAway,
                                      string userToMergeInto)
        {
            if (!MergeRequests.ContainsKey(userToMergeAway))
            {
                MergeRequests.Add(userToMergeAway, userToMergeInto);
                messageQueue.PrivateNoticeEnqueue(requestingUser,
                                                  string.Format(
                                                      "You are trying to merge user {0} into user {1}. All of user {0}'s records will stop to exist and instead be mixed into {1}'s record. This CANNOT BE UNDONE. To confirm you wish to do this, re-enter the command within the next minute.",
                                                      userToMergeAway,
                                                      userToMergeInto));
                var timer = new Timer(60000);
                timer.Elapsed += (sender, args) =>
                                 {
                                     ((Timer) sender).Stop();
                                     MergeRequests.Remove(userToMergeAway);
                                 };
                timer.Start();
            }
            else
            {
                DoMerge(model, userToMergeAway, userToMergeInto);
                messageQueue.PrivateNoticeEnqueue(requestingUser,
                                                  string.Format(
                                                      "User {0} is now a part of user {1}.",
                                                      userToMergeAway,
                                                      userToMergeInto));
            }
        }

        private static void DoMerge(IModel model,
                                    string sUserToMergeAway,
                                    string sUserToMergeInto)
        {
            var userToMergeAway = new User(model, sUserToMergeAway);
            var userToMergeInto = new User(model, sUserToMergeInto);

            userToMergeInto.Aliases.AddRange(userToMergeAway.Aliases);
            userToMergeInto.Aliases.Add(userToMergeAway.Nick);

            // TODO: Copy all join/part events to other user, delete old data

            userToMergeAway.Delete();
        }
    }
}