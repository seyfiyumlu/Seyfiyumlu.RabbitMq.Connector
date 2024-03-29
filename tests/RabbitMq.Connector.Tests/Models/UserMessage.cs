using System.Text.Json.Serialization;

namespace RabbitMq.Connector.Tests.Models;

public class UserMessage
{

    [JsonPropertyName("MessageTypeId")]
    public int MsgTypeID { get; set; }

    [JsonPropertyName("ChatId")]
    public long ChatId { get; set; }

    [JsonPropertyName("Message")]
    public string? MessageText { get; set; }

    public int ReplyMsgID { get; set; }

    [JsonPropertyName("Created_at")]
    public DateTime? CreateDate { get; set; }

    [JsonPropertyName("Sent_at")]
    public DateTime? SentDate { get; set; }


}