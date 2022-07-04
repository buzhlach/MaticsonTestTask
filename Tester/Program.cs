using System;
using System.Configuration;

namespace Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggerTools.Logger logger = new LoggerTools.Logger();

            logger.Log(LoggerTools.MessageType.Info, "Программа запущена");

            logger.ChangeSourceToFile("1.txt");
            logger.Log(LoggerTools.MessageType.Debug, "Проверка записи в файл");

            int[] x = { 1, 2, 3 };

            logger.ChangeSourceToConsole();
            logger.Log(LoggerTools.MessageType.Warning, "В массиве x 3 элемента");

            logger.ChangeSourceToDb(ConfigurationManager.ConnectionStrings["MSSQLConnection"].ToString());

            for (int i = 0; i < x.Length; i++)
            {
                logger.Log(LoggerTools.MessageType.Debug, $"i = {i} x = {x[i]}");
            }

            try
            {
                int y = x[4];
            }
            catch (Exception ex)
            {
                logger.ChangeSourceToConsole();
                logger.Log(LoggerTools.MessageType.Error, ex.ToString());

                logger.ChangeSourceToFile("1.txt");
                logger.Log(LoggerTools.MessageType.Error, ex.ToString());

                logger.ChangeSourceToDb(ConfigurationManager.ConnectionStrings["MSSQLConnection"].ToString());
                logger.Log(LoggerTools.MessageType.Error, ex.Message);
            }

            Console.ReadKey();
        }
    }
}
