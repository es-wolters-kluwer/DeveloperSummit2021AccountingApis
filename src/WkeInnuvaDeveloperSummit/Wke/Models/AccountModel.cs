namespace WkeInnuvaDeveloperSummit.Wke
{
    public class AccountModel
    {
        public string Id
        {
            get
            {
                return this.IsTemplate ? Guid.NewGuid().ToString() : this.CorrelationId;
            }
        }
        public string CorrelationId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public DateTime Version { get; set; }
        public long SequenceVersion { get; set; }
        public bool IsTemplate => this.State == "Template";
    }
}
