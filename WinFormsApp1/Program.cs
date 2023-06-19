using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotExample
{
    class Program
    {
        static TelegramBotClient botClient;
        static ManualResetEvent resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("5395956067:AAG9cHMmf4_u8sGtRm8mRBNvMZUGW3ufitU"); // Замените на свой токен бота
            Console.WriteLine("Бот запущен...");
            Task.Run(() => ReceiveMessages());
            resetEvent.WaitOne();
        }

        static async Task ReceiveMessages()
        {
            int? offset = null;
            int nutsCount = 0;
            bool isPlaying = false;
            bool isPlayingAgainstComputer = false;

            while (true)
            {
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                try
                {
                    var updates = await botClient.GetUpdatesAsync(offset);

                    foreach (var update in updates)
                    {
                        if (update.Type == UpdateType.Message)
                        {
                            var message = update.Message;

                            if (message.Type == MessageType.Text)
                            {
                                if (message.Text == "/start")
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать в игру \"Орех\"! Хотите поиграть? (Введите \"игра\")");
                                    isPlaying = true;
                                }
                                else if (message.Text.ToLower() == "игра" && isPlaying)
                                {
                                    isPlayingAgainstComputer = true;
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Отлично! Вы сыграете против компьютера. Пожалуйста, введите количество орехов для начала игры (от 20 до 50):");
                                }
                                else if (int.TryParse(message.Text, out int count) && count >= 20 && count <= 50 && isPlaying)
                                {
                                    isPlaying = true;
                                    nutsCount = count;
                                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Отлично! Давайте начнем с {nutsCount} орехов.");
                                    if (isPlayingAgainstComputer)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Ваш ход. Пожалуйста, возьмите от 1 до {Math.Min(3, nutsCount)} орехов. (Осталось {nutsCount} орехов)");
                                    }
                                    offset = update.Id + 1;
                                }
                                else if (int.TryParse(message.Text, out int take) && take >= 1 && take <= Math.Min(3, nutsCount) && isPlaying)
                                {
                                    nutsCount -= take;
                                    if (nutsCount <= 0)
                                    {
                                        if (isPlayingAgainstComputer)
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Не повезло, вы Проиграли!");
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Поздравляем, {message.From.FirstName} {message.From.LastName}! Вы выиграли!");
                                        }
                                        isPlaying = false;
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Вы взяли {take} орехов. (Осталось {nutsCount} орехов)");
                                        if (isPlayingAgainstComputer)
                                        {
                                            int computerTake = (nutsCount - 1) % 4;
                                            if (computerTake == 0)
                                            {
                                                computerTake = new Random().Next(1, Math.Min(3, nutsCount));
                                            }
                                            nutsCount -= computerTake;
                                            if (nutsCount <= 0)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Компьютер взял {computerTake} орехов. Осталось 0 орехов. Вы проиграли.");
                                                isPlaying = false;
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat.Id, $"Компьютер взял {computerTake} орехов. (Осталось {nutsCount} орехов). Ваш ход. Пожалуйста, возьмите от 1 до {Math.Min(3, nutsCount)} орехов.");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Неизвестная команда. Пожалуйста, введите корректную команду.");
                                }
                            }
                        }
                    }

                    offset = updates[updates.Length - 1].Id + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                }
            }
        }
    }
}