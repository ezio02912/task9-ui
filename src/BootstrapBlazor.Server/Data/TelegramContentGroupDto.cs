
namespace BootstrapBlazor.Server.Data;
public class TelegramContentGroupDto
{

        public int Id { get; set; }
        public string? ChatId { get; set; }
        public string? ChatTitle { get; set; }
        public string? Caption { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Message { get; set; }
        public TelegramPostContentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public static Func<IEnumerable<TelegramContentGroupDto>, string, SortOrder, IEnumerable<TelegramContentGroupDto>> GetNameSortFunc() => Utility.GetSortFunc<TelegramContentGroupDto>();

}
