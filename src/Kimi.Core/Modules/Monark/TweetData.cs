using Discord.API;
using Kimi.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618
#pragma warning disable IDE1006

namespace Kimi.Core.Modules.Monark
{
    
    internal class TweetData
    {
        internal static async Task<string> GetTweet()
        {
            var text = KimiData.TweetData.Select(x => x.text).ToArray();
            var rng = new Random();

            var n = rng.Next(0, text.Count());

            // p1 = TweetData[n].text.ToString().Split(',', '.');
            string[] p1 = Regex.Replace(text[n], @"http[^\s]+", "").Split(',', '.');
            n = rng.Next(text.Count());
            string[] p2 = Regex.Replace(text[n], @"http[^\s]+", "").Split(',', '.');
            return await Task.FromResult($"{p1[rng.Next(0, p1.Length)].TrimEnd(' ')} {p2[rng.Next(0, p2.Length)].TrimEnd(' ')}");
        }
    }

    public class Annotation
    {
        public int start { get; set; }
        public int end { get; set; }
        public double probability { get; set; }
        public string type { get; set; }
        public string normalized_text { get; set; }
    }

    public class Attachments
    {
        public List<string> media_keys { get; set; }
        public List<string> poll_ids { get; set; }
    }

    public class Bulk
    {
        public List<Datum> data { get; set; }
        public Includes includes { get; set; }
        public List<Error> errors { get; set; }
        public Meta meta { get; set; }
        public Twarc __twarc { get; set; }
    }

    public class Cashtag
    {
        public int start { get; set; }
        public int end { get; set; }
        public string tag { get; set; }
    }

    public class ContextAnnotation
    {
        public Domain domain { get; set; }
        public Entity entity { get; set; }
    }

    public class Datum
    {
        public List<ReferencedTweet> referenced_tweets { get; set; }
        public string text { get; set; }
        public string lang { get; set; }
        public bool possibly_sensitive { get; set; }
        public PublicMetrics public_metrics { get; set; }
        public string author_id { get; set; }
        public string in_reply_to_user_id { get; set; }
        public string reply_settings { get; set; }
        public string id { get; set; }
        public List<string> edit_history_tweet_ids { get; set; }
        public string conversation_id { get; set; }
        public EditControls edit_controls { get; set; }
        public string source { get; set; }
        public List<ContextAnnotation> context_annotations { get; set; }
        public Entities entities { get; set; }
        public DateTime created_at { get; set; }
        public Attachments attachments { get; set; }
    }

    public class Description
    {
        public List<Url> urls { get; set; }
        public List<Mention> mentions { get; set; }
        public List<Hashtag> hashtags { get; set; }
        public List<Cashtag> cashtags { get; set; }
    }

    public class Domain
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class EditControls
    {
        public int edits_remaining { get; set; }
        public bool is_edit_eligible { get; set; }
        public DateTime editable_until { get; set; }
    }

    public class Entities
    {
        public List<Mention> mentions { get; set; }
        public List<Annotation> annotations { get; set; }
        public List<Url> urls { get; set; }
        public List<Hashtag> hashtags { get; set; }
        public Description description { get; set; }
        public Url url { get; set; }
    }

    public class Entity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Error
    {
        public string value { get; set; }
        public string detail { get; set; }
        public string title { get; set; }
        public string resource_type { get; set; }
        public string parameter { get; set; }
        public string resource_id { get; set; }
        public string type { get; set; }
        public string section { get; set; }
    }

    public class Geo
    {
        public string place_id { get; set; }
    }

    public class Hashtag
    {
        public int start { get; set; }
        public int end { get; set; }
        public string tag { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Includes
    {
        public List<User> users { get; set; }
        public List<Tweet> tweets { get; set; }
        public List<Medium> media { get; set; }
        public List<Poll> polls { get; set; }
    }

    public class Medium
    {
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public string media_key { get; set; }
        public int? duration_ms { get; set; }
        public string preview_image_url { get; set; }
        public PublicMetrics public_metrics { get; set; }
        public string alt_text { get; set; }
    }

    public class Mention
    {
        public int start { get; set; }
        public int end { get; set; }
        public string username { get; set; }
        public string id { get; set; }
    }

    public class Meta
    {
        public int result_count { get; set; }
        public string newest_id { get; set; }
        public string oldest_id { get; set; }
        public string next_token { get; set; }
        public string previous_token { get; set; }
    }

    public class Option
    {
        public int position { get; set; }
        public string label { get; set; }
        public int votes { get; set; }
    }

    public class Poll
    {
        public List<Option> options { get; set; }
        public DateTime end_datetime { get; set; }
        public int duration_minutes { get; set; }
        public string voting_status { get; set; }
        public string id { get; set; }
    }

    public class PublicMetrics
    {
        public int retweet_count { get; set; }
        public int reply_count { get; set; }
        public int like_count { get; set; }
        public int quote_count { get; set; }
        public int followers_count { get; set; }
        public int following_count { get; set; }
        public int tweet_count { get; set; }
        public int listed_count { get; set; }
        public int view_count { get; set; }
    }

    public class ReferencedTweet
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Root
    {
        public List<Bulk> bulk { get; set; }
    }

    public class Twarc
    {
        public string url { get; set; }
        public string version { get; set; }
        public DateTime retrieved_at { get; set; }
    }

    public class Tweet
    {
        public List<ReferencedTweet> referenced_tweets { get; set; }
        public string text { get; set; }
        public string lang { get; set; }
        public bool possibly_sensitive { get; set; }
        public PublicMetrics public_metrics { get; set; }
        public string author_id { get; set; }
        public string in_reply_to_user_id { get; set; }
        public string reply_settings { get; set; }
        public string id { get; set; }
        public List<string> edit_history_tweet_ids { get; set; }
        public string conversation_id { get; set; }
        public EditControls edit_controls { get; set; }
        public string source { get; set; }
        public List<ContextAnnotation> context_annotations { get; set; }
        public Entities entities { get; set; }
        public DateTime created_at { get; set; }
        public Attachments attachments { get; set; }
        public Geo geo { get; set; }
        public Withheld withheld { get; set; }
    }

    public class Url
    {
        public int start { get; set; }
        public int end { get; set; }
        public string url { get; set; }
        public string expanded_url { get; set; }
        public string display_url { get; set; }
        public string media_key { get; set; }
        public List<Image> images { get; set; }
        public int? status { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string unwound_url { get; set; }
    }

    public class Url3
    {
        public List<Url> urls { get; set; }
    }

    public class User
    {
        public bool @protected { get; set; }
        public PublicMetrics public_metrics { get; set; }
        public string location { get; set; }
        public DateTime created_at { get; set; }
        public Entities entities { get; set; }
        public string username { get; set; }
        public bool verified { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string pinned_tweet_id { get; set; }
        public string profile_image_url { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public Withheld withheld { get; set; }
    }

    public class Withheld
    {
        public List<string> country_codes { get; set; }
        public string scope { get; set; }
        public bool copyright { get; set; }
    }
}
