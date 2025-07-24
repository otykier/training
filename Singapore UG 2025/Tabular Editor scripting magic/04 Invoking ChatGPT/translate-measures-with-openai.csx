// TRANSLATE MEASURES WITH OpenAI
// ==============================
// Ensure your model has one or more cultures (translations) defined. Then, select one or more measures
// and run this script.
// You will need an OpenAI API key, which can be hardcoded into the script below, or added to the
// TE_OpenAI_APIKey environment variable.
//
// https://blog.tabulareditor.com/2023/03/24/translate-your-measures-using-tabular-editor-and-gpt3/
// Credit: David Bojsen

#r "System.Net.Http"
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

// You need to signin to https://platform.openai.com/ and create an API key for your profile then paste that key 
// into the apiKey constant below
var apiKey = Environment.GetEnvironmentVariable("TE_OpenAI_APIKey");
if(string.IsNullOrEmpty(apiKey)) {
    Info("Please specify your OpenAI API key in an environment variable named: 'TE_OpenAI_APIKey'");
    return;
}
if(Selected.Measures.Count == 0) {
    Info("Select at least 1 measure in the TOM Explorer");
    return;
}
if(Model.Cultures.Count == 0) {
    Info("Make sure your model contains at least 1 culture");
    return;
}
var culture = SelectObject(Model.Cultures);
if(culture == null) return;
const string uri = "https://api.openai.com/v1/chat/completions";

const string model = "gpt-4-turbo";
var systemPrompt = "In the context of a Power BI semantic model, the user will provide a JSON array of measure names to be translated to " + culture.Name + ". Please respond only with a JSON array containing the translated names.";

const string template = "{{\"model\": \"{0}\",\"messages\": [{{\"role\": \"system\",\"content\":[{{\"type\":\"text\",\"text\":\"{1}\"}}]}},{{\"role\": \"user\",\"content\":[{{\"type\":\"text\",\"text\":\"{2}\"}}]}}],\"temperature\": 1,\"max_tokens\": 4000}}";

using (var client = new HttpClient()) {
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

    var measures = Selected.Measures.Concat(Selected.Tables.SelectMany(t => t.Measures)).ToList();
    var userPrompt = JsonConvert.SerializeObject(measures.Select(m => m.Name).ToList());
    
    var body = string.Format(template, model, systemPrompt.Replace("\\", "\\\\").Replace("\"", "\\\""), userPrompt.Replace("\\", "\\\\").Replace("\"", "\\\""));

    var res = client.PostAsync(uri, new StringContent(body, Encoding.UTF8,"application/json"));
    res.Result.EnsureSuccessStatusCode();
    var result = res.Result.Content.ReadAsStringAsync().Result;

    var obj = JObject.Parse(result);
    var content = obj["choices"][0]["message"]["content"];
    var contentJson = content.ToString();

    var translatedNames = JsonConvert.DeserializeObject<List<string>>(contentJson);

    for(int i = 0; i < translatedNames.Count; i++) {
        measures[i].TranslatedNames[culture] = translatedNames[i];
    }
}