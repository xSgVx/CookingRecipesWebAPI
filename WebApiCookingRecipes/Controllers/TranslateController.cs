using Microsoft.AspNetCore.Mvc;
using DeepL;
using WebApiTestApp.Models;
using System.Reflection.Metadata.Ecma335;
using System.Web;
using static System.Net.Mime.MediaTypeNames;
using DeepL.Model;
using Flurl;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net;
using Flurl.Util;

namespace WebApiTestApp.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TranslateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DeeplConfig _deeplConfig;
        private readonly RecipesApiConfig _recipesApiConfig;
        private readonly Translator _translator;

        public TranslateController( IConfiguration configuration)
        {
            _configuration = configuration;

            _deeplConfig = _configuration.GetSection("DeeplConfig")
                                         .Get<DeeplConfig>();

            _recipesApiConfig = _configuration.GetSection("RecipesApiConfig")
                                              .Get<RecipesApiConfig>();

            _translator = new Translator(_deeplConfig.AuthKey);

        }


        [HttpGet]
        [Route("RuEnTranslate/{text}")]
        public async Task<ActionResult> RuToEnText(string text)
        {
            var translatedText = await _translator.TranslateTextAsync(
                                                    text,
                                                    "ru",
                                                    "en");
            return Content(translatedText.Text);
        }


        [HttpGet]
        [ActionName("EnRu")]
        [Route("EnRuTranslate/{text}")]
        public async Task<ActionResult> EnToRuText(string text)
        {
            var translatedText = await _translator.TranslateTextAsync(
                                                    text,
                                                    "en",
                                                    "ru");
            return Content(translatedText.Text);
        }

        private async Task<CookingInfo> Translate(CookingInfo cookInfo)
        {
            var ruCookInfo = new CookingInfo();
            ruCookInfo.instructions = await TranslateString(cookInfo.instructions);

            var ingredients = new List<Ingredient>();
            foreach (var ingr in cookInfo.extendedIngredients)
            {
                var translatedIngr = await TranslateString(ingr.original);
                var ruIngr = new Ingredient(translatedIngr);
                ingredients.Add(ruIngr);
            }
            ruCookInfo.extendedIngredients = ingredients;

            return ruCookInfo;
        }

        private async Task<string> TranslateString(string text)
        {
            var translatedText = await _translator.TranslateTextAsync(
                                        text,
                                        "en",
                                        "ru");

            return translatedText.Text;
        }


        [HttpGet]
        [Route("GetRecipes/{ingridients}")]
        public async Task<IActionResult> GetJsonRecipes(string ingridients)
        {
            var url = _recipesApiConfig.Url
                            .AppendPathSegments("recipes", "findByIngredients")
                            .SetQueryParam("ingredients", new[] { ingridients })
                            .SetQueryParam("number", 5);

            var client = new RestClient(new RestClientOptions(url.ToUri()));
            var request = new RestRequest(url, Method.Get)
                                .AddHeader(_recipesApiConfig.ApiKeyName, _recipesApiConfig.ApiKeyValue)
                                .AddHeader(_recipesApiConfig.HostName, _recipesApiConfig.HostValue);


            var response = await client.GetAsync(request);

            IEnumerable<Recipe> recipes = JsonSerializer.Deserialize<Recipe[]>(response.Content);

            return Ok(recipes);
        }

        [HttpGet]
        [Route("GetRecipeInfo/{id}")]
        public async Task<IActionResult> GetRecipeInfo(int id)
        {
            var url = _recipesApiConfig.Url
                            .AppendPathSegment("recipes")
                            .AppendPathSegment(id)
                            .AppendPathSegment("information");

            var client = new RestClient(new RestClientOptions(url.ToUri()));
            var request = new RestRequest(url, Method.Get)
                                .AddHeader(_recipesApiConfig.ApiKeyName, _recipesApiConfig.ApiKeyValue)
                                .AddHeader(_recipesApiConfig.HostName, _recipesApiConfig.HostValue);


            var response = await client.GetAsync(request);

            var cookInfo = JsonSerializer.Deserialize<CookingInfo>(response.Content);
            var ruCook = await Translate(cookInfo);

            return Ok(ruCook);
        }




    }
}