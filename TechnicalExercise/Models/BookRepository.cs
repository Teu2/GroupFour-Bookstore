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
            _connectionString = connectionString; // initialise conection string
        }

        public List<TechnicalExercise.Models.BookModel> GetBooks() // retrieves all books to be displayed in a table
        {
            using var connection = new MySqlConnection(_connectionString); // establishes connection
            var books = new List<TechnicalExercise.Models.BookModel>();

            try
            {
                // opening the connection
                Console.WriteLine("Connecting to MySQL...");
                connection.Open();

                using var command = new MySqlCommand("SELECT * FROM books_library", connection); // select all books from the database to be displayed
                using var reader = command.ExecuteReader(); // returns mysql data reader object containing values from the databse

                while (reader.Read())
                {
                    books.Add(new TechnicalExercise.Models.BookModel // adding each book from the MySQL database to the list of books
                    {
                        BookId = reader.GetString("id"),
                        BookName = reader.GetString("name"),
                        Reserved = reader.GetBoolean("reserved_booking"),
                    });
                }
                return books; // returns the list of books
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                connection.Close(); // closing the connection

                return books; // returns empty list of books
            }
        }

        public string GetBookId(Dictionary<string, string> formValues)
        {
            string bookId = String.Empty;

            foreach (var item in formValues)
            {
                // extracting bookId from index.cshtml 
                if (item.Value != null)
                {
                    if (item.Key.StartsWith("customerName_"))
                    {
                        bookId = item.Key.Replace("customerName_", "");
                    }
                    else if (item.Key.StartsWith("customerEmail_"))
                    {
                        bookId = item.Key.Replace("customerEmail_", "");
                    }
                }
            }

            return bookId;
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

            // debugging
            Console.WriteLine($"book name: {book.BookName}");
            Console.WriteLine($"book id: {book.BookId}");
            Console.WriteLine($"customer name: {book.CustomerName}");
            Console.WriteLine($"customer email: {book.CustomerEmail}");
            Console.WriteLine($"booking number: {book.BookingNumber}");

            try
            {
                connection.Open(); // open connection

                // update the book in the database to be reserved with customer name and customer email - we can use the same values to unreserve the book if required
                using var command = new MySqlCommand("UPDATE books_library SET reserved_booking = true, customer_name = @customerName, customer_email = @customerEmail, booking_number = @bookingNumber WHERE id = @bookId", connection);
                
                // updating the field values for the reserved book safely
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

        public void UnReserveBook(BookModel book)
        {
            using var connection = new MySqlConnection(_connectionString); // establish connection

            try
            {
                connection.Open(); // open connection

                // update the book in the database to unreserve the book
                using var command = new MySqlCommand("UPDATE books_library SET reserved_booking = false WHERE id = @bookId, customer_name = @customerName, customer_email = @customerEmail, booking_number = @bookingNumber", connection);

                // updating the field values for the reserved book safely
                command.Parameters.AddWithValue("@bookId", book.BookId);
                command.Parameters.AddWithValue("@customerName", null);
                command.Parameters.AddWithValue("@customerEmail", null);
                command.Parameters.AddWithValue("@bookingNumber", 0);

                command.ExecuteNonQuery(); // executes sql statement
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connection.Close(); // close connection
            }
        }
    }
}
