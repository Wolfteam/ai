using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.BedrockRuntime;

const string inputExampleForRequirements = """
                                           {
                                           	 "Data": "- The person must be located in Ecuador or Argentina\n- Must be less than 18 years old\n- Must have enough money to sustain itself\n- Must be willing to relocate\n- Must has a bank account in the XYZ Bank and in the WWW Bank\n- Can be graduated from any university except: ABC University and CBA University"
                                           }
                                           """;
const string outputExampleForRequirements = """
                                            {
                                               "most_important_one": "Must reside in Ecuador or Argentina",
                                               "values": [
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
const string promptForRequirements = $"""
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
                                      Pick the most relevant one, and set it to 'most_important_one', this field must be in natural language.
                                      Present the requirements in JSON.
                                      Each value of the 'values' array must have the following structure:
                                        1. 'name' (a string identifying the requirement, it must be in lowercase, use underscores if needed).
                                        2. 'value' (the extracted value, which can be a number, boolean).
                                        3. 'op' (an operator which must be one of the following: 'GreaterThan', 'LessThan', 'NotEqual', 'Equal', 'In', 'All', 'NotIn').
                                            - The All, In, NotIn operators can only be used when the value is an array .

                                      Example JSON input for reference:
                                      {inputExampleForRequirements}

                                      Example JSON output for reference:
                                      {outputExampleForRequirements}

                                      Now, process the user’s JSON input and provide the output accordingly with no additional text or explanation.
                                      """;


const string inputExampleForBenefits = """
                                       {
                                       	 "Data": "- You will get a bonus of 100$ to spend\n- You will start paying after 3 months"
                                       }
                                       """;
const string outputExampleForBenefits = """
                                        {
                                            "most_important_one": "100$ Bonus",
                                        	"values": [
                                        		{
                                        			"name": "bonus",
                                        			"value": 100
                                        		},
                                        		{
                                        			"name": "free_months",
                                        			"value": 3
                                        		}
                                        	]
                                        }
                                        """;
const string promptForBenefits = $"""
                                  You are a [JSON parser] designed to assist users in processing and interpreting JSON that contains patient benefits data efficiently.
                                  Your role is to analyze and extract meaningful information from JSON inputs provided by the user.
                                  Your task is to parse the JSON data, interpret its contents based on the given context, and present
                                  the results in a clear, structured format suitable for [JSON].

                                  For this task, the JSON input will contain one string value.
                                  This value represents a list of benefits written in natural language, with each benefit
                                  separated by a newline (e.g., '- You will get a bonus of 100$ to spend\n').

                                  Your context is to treat each newline-separated entry as an individual benefit, 
                                  extract these benefits, and return them in a clean, organized manner.

                                  Steps to follow:

                                  Parse the provided JSON input.
                                  Extract the string value containing the list of benefits.
                                  Split the string into individual benefits based on newline separators, if you think that the benefit is not relevant (like contact information) to the patient, discard it.
                                  Pick the most relevant one, and set it to 'most_important_one', this field must be in natural language.
                                  Present the benefits in JSON.
                                  Each value has the following structure:
                                    1. 'name' (a string identifying the benefit, it must be in lowercase, use underscores if needed).
                                    2. 'value' (the extracted value, which can be a number or a boolean).

                                  Example JSON input for reference:
                                  {inputExampleForBenefits}

                                  Example JSON output for reference:
                                  {outputExampleForBenefits}

                                  Now, process the user’s JSON input and provide the output accordingly with no additional text or explanation.
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

var detail = new Detail
{
    Renewal = data!.AddRenewalDetails,
    Income = data.IncomeReq ? data.IncomeDetails! : "Not required"
};
var result = new Result
{
    ProgramName = data.ProgramName,
    ProgramType = data.AssistanceType,
    CoverageEligibilities = data.CoverageEligibilities,
    Funding = new Funding
    {
        CurrentFundingLevel = data.FundLevelType ?? "Data Not Available"
    },
    Details =
    [
        detail
    ]
};

string? requirementsJson = await AskCohere(data.EligibilityDetails, promptForRequirements);
if (!string.IsNullOrWhiteSpace(requirementsJson))
{
    PromptJsonResults promptResult = JsonSerializer.Deserialize<PromptJsonResults>(requirementsJson)!;
    detail.Eligibility = promptResult.MostImportantOne;
    result.Requirements = promptResult.Values;
}

string? benefitsJson = await AskCohere(data.ProgramDetails, promptForBenefits);
if (!string.IsNullOrWhiteSpace(benefitsJson))
{
    PromptJsonResults promptResult = JsonSerializer.Deserialize<PromptJsonResults>(benefitsJson)!;
    detail.Program = promptResult.MostImportantOne;
    result.Benefits = promptResult.Values;
}

if (!string.IsNullOrWhiteSpace(data.EnrollmentURL))
{
    result.Forms.Add(new Form
    {
        Name = "Enrollment Form",
        Link = data.EnrollmentURL
    });
}

string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions
{
    WriteIndented = true
});
Console.WriteLine(jsonResult);

return;

static async Task<string?> AskCohere(string data, string prompt)
{
    string userData = JsonSerializer.Serialize(new
    {
        Data = data
    }, new JsonSerializerOptions
    {
        WriteIndented = true
    });

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

    return cohereResponse?.Generations.FirstOrDefault()?.Text.Trim();
}


// @formatter:off
internal class Data
{
    public string ProgramName { get; set; } = string.Empty;

    public List<string> CoverageEligibilities { get; set; } = [];

    public string AssistanceType { get; set; }

    public string EligibilityDetails { get; set; } = string.Empty;

    public string ProgramDetails { get; set; } = string.Empty;

    public string EnrollmentURL { get; set; } = string.Empty;

    public string? FundLevelType { get; set; }

    public bool IncomeReq { get; set; }

    public string? IncomeDetails { get; set; }

    public string AddRenewalDetails { get; set; } = string.Empty;
}

internal class CoherePrompt(string prompt)
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; init; } = prompt;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; init; } = 1000;

    [JsonPropertyName("temperature")]
    public double Temperature { get; init; } = 0.2;

    [JsonPropertyName("p")]
    public double TopP { get; init; } = 0.75;
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

public class PromptJsonResults
{
    [JsonPropertyName("most_important_one")]
    public string MostImportantOne { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<PromptJsonResultItem> Values { get; set; } = [];
}

public class PromptJsonResultItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public dynamic Value { get; set; }

    [JsonPropertyName("op")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Op { get; set; }
}

public class Detail
{
    [JsonPropertyName("eligibility")]
    public string Eligibility { get; set; }

    [JsonPropertyName("program")]
    public string Program { get; set; }

    [JsonPropertyName("renewal")]
    public string Renewal { get; set; }

    [JsonPropertyName("income")]
    public string Income { get; set; }
}

public class Form
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }
}

public class Funding
{
    [JsonPropertyName("evergreen")]
    public bool Evergreen { get; set; } = true;

    [JsonPropertyName("current_funding_level")]
    public string CurrentFundingLevel { get; set; }
}

public class Result
{
    [JsonPropertyName("program_name")]
    public string ProgramName { get; set; }

    [JsonPropertyName("coverage_eligibilities")]
    public List<string> CoverageEligibilities { get; set; } = [];

    [JsonPropertyName("program_type")]
    public string ProgramType { get; set; }

    [JsonPropertyName("requirements")]
    public List<PromptJsonResultItem> Requirements { get; set; } = [];

    [JsonPropertyName("benefits")]
    public List<PromptJsonResultItem> Benefits { get; set; } = [];

    [JsonPropertyName("forms")]
    public List<Form> Forms { get; set; } = [];

    [JsonPropertyName("funding")]
    public Funding Funding { get; set; }

    [JsonPropertyName("details")]
    public List<Detail> Details { get; set; } = [];
}