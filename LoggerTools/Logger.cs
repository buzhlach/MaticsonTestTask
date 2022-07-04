using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;

namespace LoggerTools
{
    public class Logger
    {

        /// <summary>
        /// Текстовый источник записи.
        /// </summary>
        private Stream _textSource = null;

        /// <summary>
        /// Источник записи базы данных.
        /// </summary>
        private SqlConnection _dbSource = null;

        /// <summary>
        /// Конструктор без параметров, в качестве потока - консоль.
        /// </summary>
        public Logger()
        {
            _textSource = Console.OpenStandardOutput();
        }

        /// <summary>
        /// Конструктор с текстовым потоком.
        /// </summary>
        /// <param name="textSource">Текстовый поток для записи.</param>
        public Logger(Stream textSource)
        {
            _textSource = textSource;
        }

        /// <summary>
        /// Конструктор с подключением к БД.
        /// </summary>
        /// <param name="dbSource">Подключение к БД.</param>
        public Logger(SqlConnection dbSource)
        {
            _dbSource = dbSource;
        }

        /// <summary>
        /// Сменить источник записи на консоль.
        /// </summary>
        public void ChangeSourceToConsole()
        {
            _dbSource?.Close();
            _textSource?.Close();

            _dbSource=null;

            _textSource = Console.OpenStandardOutput();
        }

        /// <summary>
        /// Сменить источник записи на файл.
        /// </summary>
        /// <param name="filePath"></param>
        public void ChangeSourceToFile(string filePath)
        {
            if(filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            _dbSource?.Close();
            _textSource?.Close();

            _dbSource = null;

            _textSource = new FileStream(filePath, FileMode.Append);
        }

        /// <summary>
        /// Сменить источник записи на базу данных.
        /// </summary>
        /// <param name="connectionString"></param>
        public void ChangeSourceToDb(string connectionString)
        {
            _dbSource?.Close();
            _textSource?.Close();

            _textSource=null;

            _dbSource = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Логирование.
        /// </summary>
        /// <param name="messageType">Тип лога.</param>
        /// <param name="text">Текст лога.</param>
        public void Log(MessageType messageType,string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (_textSource == null)
            {
                DbLog(messageType, text);
            }
            else
            {
                TextLog(messageType, text);
            }
        }


        /// <summary>
        /// Логирование в базу данных.
        /// </summary>
        /// <param name="messageType">Тип лога.</param>
        /// <param name="text">Текст лога.</param>
        /// <exception cref="Exception"></exception>
        private void DbLog(MessageType messageType, string text)
        {
            string sql = "INSERT INTO dbo.logs(log_id,message_type,time,text) VALUES(@param1,@param2,@param3,@param4)";
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _dbSource;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@param1",Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@param2", messageType.ToString());
                cmd.Parameters.AddWithValue("@param3", DateTime.Now);
                cmd.Parameters.AddWithValue("@param4", text);

                try
                {
                    _dbSource.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new Exception("Ошибка при записи в базу данных.", ex);
                }
                finally
                {
                    _dbSource.Close();
                }
            }
        }

        /// <summary>
        /// Логирование в текстовый источник.
        /// </summary>
        /// <param name="messageType">Тип лога.</param>
        /// <param name="text">Текст лога.</param>
        private void TextLog(MessageType messageType, string text)
        {

            try
            {
                using (StreamWriter sw = new StreamWriter(_textSource))
                {
                    sw.WriteLine($"{messageType.ToString()}: {DateTime.Now.ToString()}  {text}");
                }
            }
            catch (IOException ex)
            {
                throw new IOException("Поток не подходит для StreamWriter.", ex);
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
