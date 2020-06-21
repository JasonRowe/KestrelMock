namespace KestrelMock.Domain
{
    public class InputMappings
    {
        public PathMapping PathMapping { get; set; } = new PathMapping();
        public PathStartsWithMapping PathStartsWithMapping { get; set; } = new PathStartsWithMapping();
        public BodyCheckMapping BodyCheckMapping { get; set; } = new BodyCheckMapping();
        public PathMatchesRegexMapping PathMatchesRegexMapping { get; set; } = new PathMatchesRegexMapping();
    }

}
