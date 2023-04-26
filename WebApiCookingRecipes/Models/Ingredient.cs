namespace WebApiTestApp.Models
{
    public class Ingredient
    {
        public string original { get; set; }

        public Ingredient()
        {
            
        }

        public Ingredient(string original)
        {
            this.original = original;
        }
    }
}
