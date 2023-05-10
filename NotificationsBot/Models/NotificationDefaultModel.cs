namespace NotificationsBot.Models
{
    public class NotificationDefaultModel
    {
        public string Title { get; set; }

        public string AppName { get; set; }

        public string Description { get; set; }

        public string NotificationUrl { get; set; }
    }

    public class Order
    {
        public string OrderId { get; set; }

        public string OrderName { get; set; }
    }
}

