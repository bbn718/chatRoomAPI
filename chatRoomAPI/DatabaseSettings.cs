namespace chatRoomAPI
{
    public class DatabaseSettings
    {
        public string? Server { get; private set; }
        public string? InitialCatalog { get; private set; }
        public string? UserId { get; private set; }
        public string? Password { get; private set; }

        public string connectionString
        {
            get
            {
                return $"Server={Server};Initial Catalog={InitialCatalog};User Id={UserId};Password={Password};";
            }
        }

        public DatabaseSettings(IConfiguration? configuration)
        {
            Server = configuration["ConnectionStrings:Server"];
            InitialCatalog = configuration["ConnectionStrings:Initial Catalog"];
            UserId = configuration["ConnectionStrings:UserId"];
            Password = configuration["ConnectionStrings:Password"];
        }
    }
}
