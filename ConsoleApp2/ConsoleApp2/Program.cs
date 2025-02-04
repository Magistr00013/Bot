using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static readonly ITelegramBotClient botClient = new TelegramBotClient("8153063302:AAH3byPLUHJBkFOi0K0piCCVhUfX_kjnOkM");
    private static readonly long targetGroupId = -1002273117435; // ID группы для пересылки сообщений

    private static readonly Dictionary<long, int> userStepTracker = new Dictionary<long, int>(); // хранит состояние пользователей

    static async Task Main(string[] args)
    {
        botClient.OnMessage += Bot_OnMessage;
        botClient.OnCallbackQuery += Bot_OnCallbackQuery;
        botClient.StartReceiving();

        Console.WriteLine("Bot is running...");
        Console.ReadLine();
        botClient.StopReceiving();
    }

    private static async void Bot_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
        {
            // Пересылаем сообщение в целевую группу
            await botClient.SendTextMessageAsync(targetGroupId, e.Message.Text, replyMarkup: GetInitialKeyboard(e.Message.Chat.Id));
        }
    }

    private static async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
    {
        var chatId = e.CallbackQuery.Message.Chat.Id;

        if (!userStepTracker.ContainsKey(chatId))
        {
            userStepTracker[chatId] = 0; // начинаем с нуля
        }

        int currentStep = userStepTracker[chatId];

        // Проверяем, в какой последовательности нажимались кнопки
        if (e.CallbackQuery.Data == "button1" && currentStep == 0)
        {
            userStepTracker[chatId] = 1; // переходим к следующему шагу
            await botClient.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, GetUpdatedKeyboard(chatId, 1));
        }
        else if (e.CallbackQuery.Data == "button2" && currentStep == 1)
        {
            userStepTracker[chatId] = 2;
            await botClient.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, GetUpdatedKeyboard(chatId, 2));
        }
        else if (e.CallbackQuery.Data == "button3" && currentStep == 2)
        {
            userStepTracker[chatId] = 3;
            await botClient.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, GetUpdatedKeyboard(chatId, 3));
        }
        else if (e.CallbackQuery.Data == "button4" && currentStep == 3)
        {
        
            userStepTracker[chatId] = 4;
            await botClient.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, GetUpdatedKeyboard(chatId, 4));
        }
        else
        {
            await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Ты не можешь обогнать действие!");
        }

        await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id); // отвечает на нажатие кнопки
    }
    private static async Task NotifyUser(long chatId, string message)
    {
        await botClient.SendTextMessageAsync(chatId, message);
    }

    private static async Task UpdateKeyboard(Message message, int step)
    {
        var keyboard = GetUpdatedKeyboard(message.Chat.Id, step);
        await botClient.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, keyboard);
    }

    private static InlineKeyboardMarkup GetInitialKeyboard(long chatId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Принял", "button1") },
            new[] { InlineKeyboardButton.WithCallbackData("Выводи", "button2") },
            new[] { InlineKeyboardButton.WithCallbackData("Забрал", "button3") },
            new[] { InlineKeyboardButton.WithCallbackData("Свободен", "button4") },
        });
    }

    private static InlineKeyboardMarkup GetUpdatedKeyboard(long chatId, int step)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        for (int i = 1; i <= 4; i++)
        {
            var buttonText = step >= i ? $"Кнопка {i} (Выполнил)" : $"Кнопка {i}";
            var callbackData = $"button{i}";
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonText, callbackData) });
        }

        return new InlineKeyboardMarkup(buttons);
    }
}
