
namespace Mailer.Model
{
    public class EmailMessage
    {
        public virtual string Body { get; set; }
        public virtual string To { get; set; }
        public virtual string Subject { get; set; }
        public virtual string CC { get; set; }
        public virtual string BCC { get; set; }

    }
}
