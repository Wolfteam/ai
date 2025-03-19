using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.BedrockRuntime;

const string inputExample = """
                            "input":
                            {
                            	"EligibilityDetails": "- The person must be located in Ecuador or Argentina\n- Must be less than 18 years old\n- Must have enough money to sustain itself\n- Must be willing to relocate\n- Must has a bank account in the XYZ Bank and in the WWW Bank\n- Can be graduated from any university except: ABC University and CBA University"
                            }
                            """;

const string outputExample = """
                             "output":
                             {
                             	"requirements": [
                             		{
                             			"name": "location",
                             			"value": ["Ecuador", "Argentina"],
                             			"op": "In"
                             		},
                             		{
                             			"name": "age",
                             			"value": 18,
                             			"op": "LessThan"
                             		},
                             		{
                             			"name": "has_enough_money",
                             			"value": true,
                             			"op": "Equal"
                             		},
                             		{
                             			"name": "has_required_bank_accounts",
                             			"value": ["XYZ", "WWW"],
                             			"op": "All"
                                    },
                                    {
                             			"name": "must_be_willing_to_relocate",
                             			"value": true,
                             			"op": "Equal"
                             		},
                             		{
                             			"name": "not_graduated_from",
                             			"value": ["ABC", "CBA"],
                             			"op": "NotIn"
                                    }
                             	]
                             }
                             """;
const string prompt = $"""
                       You are a [JSON parser] designed to assist users in processing and interpreting JSON data efficiently. 
                       Your role is to analyze and extract meaningful information from JSON inputs provided by the user. 
                       Your task is to parse the JSON data, interpret its contents based on the given context, and present 
                       the results in a clear, structured format suitable for [JSON].

                       For this task, the JSON input will contain one string value. 
                       This value represents a list of requirements written in natural language, with each requirement 
                       separated by a newline (e.g., '- The person must be located in South America\n'). 
                       
                       Your context is to treat each newline-separated entry as an individual requirement that may have sub
                       requirements, extract these requirements, and return them in a clean, organized manner. 

                       Steps to follow:

                       Parse the provided JSON input.
                       Extract the string value containing the list of requirements.
                       Split the string into individual requirements based on newline separators.
                       Present the requirements in JSON.
                       Each requirement has the following structure:
                         1. 'name' (a string identifying the requirement, it must be in lowercase, use underscores if needed) 
                         2. 'value' (the extracted value, which can be a number, boolean, string or an array) 
                         3. 'op' (an operator value like 'GreaterThan', 'LessThan', 'NotEqual', 'Equal', 'In', 'All', 'NotIn').
                       
                       Ensure the output is concise and retains the original meaning of each requirement.
                       
                       Example JSON input for reference:

                       {inputExample}
                       
                       Example JSON output for reference:
                       
                       {outputExample}

                       Now, process the user’s JSON input and provide the output accordingly.
                       """;

string? path;
do
{
    Console.WriteLine("Enter the path of the json file");
    path = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
    {
        break;
    }

    path = null;
} while (string.IsNullOrWhiteSpace(path));

var file = new FileStream(path, FileMode.Open);
var data = await JsonSerializer.DeserializeAsync<Data>(file);
string userData = JsonSerializer.Serialize(data);

var client = new AmazonBedrockRuntimeClient();
var coherePrompt = new CoherePrompt(prompt + Environment.NewLine + "Here's the user's json:\n" + userData);
var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(coherePrompt)));
var requestModel = new Amazon.BedrockRuntime.Model.InvokeModelRequest
{
    ModelId = "cohere.command-text-v14",
    ContentType = "application/json",
    Accept = "application/json",
    Body = stream
};
var response = await client.InvokeModelAsync(requestModel);
var cohereResponse = JsonSerializer.Deserialize<CohereResponse>(response.Body);

string? responseText = cohereResponse?.Generations.FirstOrDefault()?.Text.Trim();
if (string.IsNullOrWhiteSpace(responseText))
{
    Console.WriteLine("Could not generate the requirements");
}
else
{
    Console.WriteLine(responseText);
}

internal record Data(string EligibilityDetails);

internal class CoherePrompt(string prompt)
{
	[JsonPropertyName("prompt")]
	public string Prompt { get; init; } = prompt;

	[JsonPropertyName("max_tokens")]
	public int MaxTokens { get; init; } = 1000;

	[JsonPropertyName("temperature")]
	public double Temperature { get; init; } = 0.9;

	[JsonPropertyName("p")]
	public double TopP { get; init; } = 1;
}

internal class CohereResponse
{
	[JsonPropertyName("generations")]
	public List<Generation> Generations { get; init; } = [];
}

internal class Generation(string text)
{
	[JsonPropertyName("text")]
	public string Text { get; init; } = text;
}