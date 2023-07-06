using System.Text.Json;
using System.Text.Json.Serialization;

namespace TryInventories.Models
{
    public record ProfileResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("first_name")]
        string FirstName,
        [property: JsonPropertyName("last_name")]
        string LastName,
        [property: JsonPropertyName("last_login")]
        DateTime LastLogin,
        [property: JsonPropertyName("timezone")]
        string Timezone,
        [property: JsonPropertyName("subscribed_bandwidth_usage_notifications")]
        bool SubscribedBandwidthUsageNotifications,
        [property: JsonPropertyName("subscribed_subscription_notifications")]
        bool SubscribedSubscriptionNotifications,
        [property: JsonPropertyName("subscribed_proxy_usage_statistics")]
        bool SubscribedProxyUsageStatistics,
        [property: JsonPropertyName("subscribed_usage_warnings")]
        bool SubscribedUsageWarnings,
        [property: JsonPropertyName("subscribed_guides_and_tips")]
        bool SubscribedGuidesAndTips,
        [property: JsonPropertyName("subscribed_survey_emails")]
        bool SubscribedSurveyEmails,
        [property: JsonPropertyName("tracking_id")]
        string TrackingId,
        [property: JsonPropertyName("helpscout_beacon_signature")]
        string HelpscoutBeaconSignature,
        [property: JsonPropertyName("announce_kit_user_token")]
        string AnnounceKitUserToken)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
