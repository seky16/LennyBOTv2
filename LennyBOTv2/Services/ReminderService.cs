using System;
using System.Collections.Generic;
using Discord;
using LennyBOTv2.Models.Reminders;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace LennyBOTv2.Services
{
    public class ReminderService
    {
        private const string TypePrefix = "datetimeV2.";

        public void CreateReminder(Discord.WebSocket.SocketUserMessage message, string input)
        {
            var dtNow = message.Timestamp.UtcDateTime;
            var results = DateTimeRecognizer.RecognizeDateTime(input, Culture.EnglishOthers);

            if (results.Count == 0)
            {
                throw new FormatException("Couldn't recognize DateTime from input");
            }

            var result = results[0];

            if (!result.TypeName.StartsWith(TypePrefix))
                throw new NotSupportedException($"'{result.TypeName}' not supported");

            var values = ((IList<Dictionary<string, string>>)result.Resolution["values"])[0];

            DateTime dt;
            switch (result.TypeName.Substring(TypePrefix.Length))
            {
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATE: // today, 21.1.2020
                    dt = DateTime.Parse(values["value"]) + dtNow.TimeOfDay;
                    break;
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_TIME: // 10:00
                    dt = dtNow.Date + TimeSpan.Parse(values["value"]);
                    break;
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATETIME: // now, in 2 hours, 21.1.2020 10:00
                    dt = DateTime.Parse(values["value"]);
                    break;
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_TIMEPERIOD: // evening
                    dt = dtNow.Date + TimeSpan.Parse(values["start"]);
                    break;
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DURATION: // 2 hours
                    dt = dtNow.Date + dtNow.TimeOfDay + TimeSpan.FromSeconds(Convert.ToDouble(values["value"]));
                    break;
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATETIMEPERIOD: // between 21.1.2020 1:00 and 22.2.2020 0:00
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATEPERIOD: // from 21.1.2020 to 22.2.2020, between 21.1.2020 and 22.2.2020, next week, before tomorrow
                    throw new NotSupportedException($"'{result.TypeName}' not supported");
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_TIMEZONE:
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATETIMEPOINT:
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_DATETIMEALT:
                case Microsoft.Recognizers.Text.DateTime.Constants.SYS_DATETIME_SET:
                default:
                    throw new NotImplementedException($"'{result.TypeName}' not implemented");
            }

            if (dtNow >= dt)
                throw new InvalidOperationException($"'{dt}' is not in the future");

            var text = (input.Substring(0, result.Start).TrimEnd() + input.Substring(result.End + 1)).Trim();

            using var db = LennyServiceProvider.OpenDB();
            var reminders = db.GetCollection<ReminderModel>();
            var reminder = new ReminderModel(dtNow, dt, text, message.Author.Mention, message.GetJumpUrl());
            reminders.Insert(reminder);
        }
    }
}
