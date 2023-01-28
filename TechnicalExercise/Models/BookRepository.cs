using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace TechnicalExercise.Models
{
    public class BookRepository
    {
        private readonly string _connectionString;

        public BookRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TechnicalExercise.Models.BookModel> GetBooks()
        {
            using var connection = new MySqlConnection(_connectionString);
            var books = new List<TechnicalExercise.Models.BookModel>();

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                connection.Open();

                using var command = new MySqlCommand("SELECT * FROM books_library", connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    books.Add(new TechnicalExercise.Models.BookModel
                    {
                        BookId = reader.GetString("id"),
                        BookName = reader.GetString("name"),
                        Reserved = reader.GetBoolean("reserved"),
                    });
                }
                return books;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
                connection.Close();
                Console.WriteLine("Done.");
               
                return books;
            }
        }

        public BookModel GetBookById(string bookId, List<BookModel> books)
        {
            foreach (var book in books)
            {
                if (book.BookId == bookId)return book;
            }
            return null;
        }

        public void UpdateBook(BookModel book)
        {

        }

        public List<TechnicalExercise.Models.BookModel> SearchBooks(string query)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM books WHERE title LIKE @query OR author LIKE @query", connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");
                    using (var reader = command.ExecuteReader())
                    {
                        var books = new List<TechnicalExercise.Models.BookModel>();
                        while (reader.Read())
                        {
                            books.Add(new TechnicalExercise.Models.BookModel
                            {
                                BookId = reader.GetString("id"),
                                BookName = reader.GetString("title"),
                                Reserved = reader.GetBoolean("reserved"),
                                BookingNumber = reader.GetString("booking_number")
                            });
                        }
                        return books;
                    }
                }
            }
        }

        public string ReserveBook(BookModel book)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("UPDATE books SET reserved = true, customer_name = @customerName, booking_number = @bookingNumber WHERE book_id = @bookId", connection))
                {
                    var bookingNumber = Guid.NewGuid().ToString();
                    command.Parameters.AddWithValue("@bookId", book.BookId);
                    command.Parameters.AddWithValue("@customerName", book.CustomerName);
                    command.Parameters.AddWithValue("@bookingNumber", book.BookingNumber);
                    command.ExecuteNonQuery();
                    return bookingNumber;
                }
            }
        }
    }
}
