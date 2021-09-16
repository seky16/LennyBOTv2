using System;

namespace LennyBOTv2.Models.Reminders
{
    public record ReminderModel(
        DateTime CreatedUtc,
        DateTime ReminderDateTimeUtc,
        string Text,
        string Mention,
        string JumpUrl
    );
}
