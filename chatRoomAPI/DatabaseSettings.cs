namespace chatRoomAPI
{
    public class DatabaseSettings
    {
        public string Server { get; private set; }
        public string InitialCatalog { get; private set; }
        public string UserId { get; private set; }
        public string Password { get; private set; }

        public string connectionString
        {
            get
            {
                return $"Server={Server};Database={InitialCatalog};User Id={UserId};Password={Password};";
            }
        }

        public DatabaseSettings(IConfiguration configuration)
        {
            Server = configuration["DatabaseSettings:Server"];
            InitialCatalog = configuration["DatabaseSettings:Initial Catalog"];
            UserId = configuration["DatabaseSettings:UserId"];
            Password = configuration["DatabaseSettings:Password"];
        }
    }
}
