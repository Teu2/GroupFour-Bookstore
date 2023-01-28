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
                        Reserved = reader.GetBoolean("reserved_booking"),
                    });
                }
                return books;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
                connection.Close(); // closing the connection
                Console.WriteLine("Done.");
               
                return books;
            }
        }

        public BookModel GetBookById(string bookId, List<BookModel> books)
        {
            foreach (var book in books) // finds the book with the corrosponding book id
            {
                if (book.BookId == bookId) return book;
            }
            return null;
        }

        public void ReserveBook(BookModel book)
        {
            using var connection = new MySqlConnection(_connectionString); // establish connection

            Console.WriteLine($"book name: {book.BookName}");
            Console.WriteLine($"book id: {book.BookId}");
            Console.WriteLine($"customer name: {book.CustomerName}");
            Console.WriteLine($"customer email: {book.CustomerEmail}");
            Console.WriteLine($"booking number: {book.BookingNumber}");

            try
            {
                connection.Open(); // open connection

                // update the book in the database to be reserved with customer name and customer email - we can use the same values to unreserve the book
                using var command = new MySqlCommand("UPDATE books_library SET reserved_booking = true, customer_name = @customerName, customer_email = @customerEmail, booking_number = @bookingNumber WHERE id = @bookId", connection);
                
                // updating the field values for the reserved book
                command.Parameters.AddWithValue("@bookId", book.BookId);
                command.Parameters.AddWithValue("@customerName", book.CustomerName);
                command.Parameters.AddWithValue("@customerEmail", book.CustomerEmail);
                command.Parameters.AddWithValue("@bookingNumber", book.BookingNumber);

                command.ExecuteNonQuery(); // executes sql statement
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                connection.Close(); // close connection
            }
        }
    }
}
