#region Using

using System.Collections.Generic;
using System.Timers;

#endregion


namespace DeathmicChatbot
{
    internal static class UserMerger
    {
        private static readonly Dictionary<string, string> MergeRequests =
            new Dictionary<string, string>();

        public static void MergeUsers(MessageQueue messageQueue,
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
                DoMerge(userToMergeAway, userToMergeInto);
                messageQueue.PrivateNoticeEnqueue(requestingUser,
                                                  string.Format(
                                                      "User {0} is now a part of user {1}.",
                                                      userToMergeAway,
                                                      userToMergeInto));
            }
        }

        private static void DoMerge(string userToMergeAway,
                                    string userToMergeInto)
        {
            // TODO: Copy all join/part events to other user, delete old data
            // TODO: Enter new user into list of aliases of old user
        }
    }
}