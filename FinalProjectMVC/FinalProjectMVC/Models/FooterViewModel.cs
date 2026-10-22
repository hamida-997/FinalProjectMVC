namespace FinalProjectMVC.Models
{
    public class FooterViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public int CurrentYear => DateTime.Now.Year;
    }
}
