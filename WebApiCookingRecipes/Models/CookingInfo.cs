namespace WebApiTestApp.Models
{
    public class CookingInfo
    {
        public IEnumerable<Ingredient> extendedIngredients { get; set; }
        public string instructions { get; set; }

    }
}
