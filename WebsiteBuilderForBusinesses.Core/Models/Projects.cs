using WebsiteBuilderForBusinesses.Core.Infrastructures;

namespace WebsiteBuilderForBusinesses.Core.Models
{
    public class Projects
    {
        public Guid Id { get; }
        public string Name { get; } = string.Empty;
        public DateTime DateOpen { get; }
        public string TextHtml { get; } = string.Empty;

        public static ResultModel<Projects> Create(Guid id, string name, DateTime dateOpen, string textHtml)
        {
            if (id == Guid.Empty)
                return ResultModel<Projects>.Failure("Поле Id не должно быть пустым");
            if(name == string.Empty)
                return ResultModel<Projects>.Failure("Название не должно быть пустым");

            return ResultModel<Projects>.Success(new Projects(id, name, dateOpen, textHtml));
        }

        private Projects(Guid id, string name, DateTime dateOpen, string textHtml)
        {
            Id = id;
            Name = name;
            DateOpen = dateOpen;
            TextHtml = textHtml;
        }

    }
}
