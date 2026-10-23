namespace FinalProjectMVC.ViewModels
{
    public class FooterVM
    {
        public List<CategoryVM> Categories { get; set; } = new List<CategoryVM>();
        public int CurrentYear => DateTime.Now.Year;
    }
}
