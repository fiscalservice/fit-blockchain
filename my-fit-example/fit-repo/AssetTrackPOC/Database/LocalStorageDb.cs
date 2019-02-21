//using System; TODO - use after POC for storing locally private keys and deviceAddress
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using AssetTrackPOC.Interface;
//using SQLite;
//using Xamarin.Forms;

//namespace AssetTrackPOC.Database
//{
//    public class LocalStorageDb
//    {


//        static LocalStorageDb database;


//        public LocalStorageDb(string dbPath)
//        {
//            database = new SQLiteAsyncConnection(dbPath);
//            database.CreateTableAsync<TodoItem>().Wait();
//        }


//        public static LocalStorageDb Database
//        {
//            get
//            {
//                if (database == null)
//                {
//                    database = new LocalStorageDb(DependencyService.Get<SqlInterface>().GetLocalFilePath("TodoSQLite.db3"));
//                }
//                return database;
//            }
//        }

//        public static implicit operator LocalStorageDb(SQLiteAsyncConnection v)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<List<TodoItem>> GetItemsAsync()
//        {
//            return database.Table<TodoItem>().ToListAsync();
//        }

//        public Task<List<TodoItem>> GetItemsNotDoneAsync()
//        {
//            return database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
//        }

//        public Task<TodoItem> GetItemAsync(int id)
//        {
//            return database.Table<TodoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
//        }

//        public Task<int> SaveItemAsync(TodoItem item)
//        {
//            if (item.ID != 0)
//            {
//                return database.UpdateAsync(item);
//            }
//            else
//            {
//                return database.InsertAsync(item);
//            }
//        }

//        public Task<int> DeleteItemAsync(TodoItem item)
//        {
//            return database.DeleteAsync(item);
//        }
//    }
//}
