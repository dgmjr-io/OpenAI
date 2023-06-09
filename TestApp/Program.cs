/* This is a C# code example that demonstrates how to use the Azure Text Analytics API to perform
extractive summarization on a given text document. The code imports necessary libraries and defines
a method `TextSummarizationExample` that takes in a `TextAnalyticsClient` object and performs the
summarization operation on a sample text document. The main method creates a `TextAnalyticsClient`
object and calls the `TextSummarizationExample` method. */
using Azure;
using System;
using Azure.AI.TextAnalytics;
using System.Threading.Tasks;
using System.Collections.Generic;
using static global::System.Environment;
using Azure.AI.OpenAI;
using System.ComponentModel;
using System.Security.Authentication.ExtendedProtection;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace Example
{
    static class Program
    {
        // This example requires environment variables named "LANGUAGE_KEY" and "LANGUAGE_ENDPOINT"
        static readonly string languageKey = GetEnvironmentVariable("AZURE_OPENAI_KEY");
        static readonly string languageEndpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

        private static readonly AzureKeyCredential credentials = new(languageKey);
        private static readonly Uri endpoint = new(languageEndpoint);

        /// <summary>
        /// This function demonstrates an example of text summarization using the Text Analytics client
        /// in C#.
        /// </summary>
        /// <param name="client">TextAnalyticsClient is a class provided by the Azure
        /// Cognitive Services Text Analytics API that allows developers to access the API's text
        /// analysis capabilities, such as sentiment analysis, key phrase extraction, and text
        /// summarization. It requires an API key and endpoint to be instantiated.</param>
        static async Task TextSummarizationExample(TextAnalyticsClient client)
        {

            string document =
                @"The extractive summarization feature uses natural language processing techniques to locate key sentences in an unstructured text document.
                These sentences collectively convey the main idea of the document. This feature is provided as an API for developers.
                They can use it to build intelligent solutions based on the relevant information extracted to support various use cases.
                In the public preview, extractive summarization supports several languages. It is based on pretrained multilingual transformer models, part of our quest for holistic representations.
                It draws its strength from transfer learning across monolingual and harness the shared nature of languages to produce models of improved quality and efficiency.";

            // Prepare analyze operation input. You can add multiple documents to this list and perform the same
            // operation to all of them.
            var batchInput = new List<string> { document };

            var actions = new TextAnalyticsAction
            {
                ExtractSummaryActions = new List<ExtractSummaryAction>
                {
                    new ExtractSummaryAction()
                }
            };

            // Start analysis process.
            AnalyzeActionsOperation operation = await client.StartAnalyzeActionsAsync(
                batchInput,
                actions
            );
            await operation.WaitForCompletionAsync();
            // View operation status.
            WriteLine($"AnalyzeActions operation has completed");
            WriteLine();

            WriteLine($"Created On   : {operation.CreatedOn}");
            WriteLine($"Expires On   : {operation.ExpiresOn}");
            WriteLine($"Id           : {operation.Id}");
            WriteLine($"Status       : {operation.Status}");

            WriteLine();
            // View operation results.
            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                IReadOnlyCollection<ExtractSummaryActionResult> summaryResults =
                    documentsInPage.ExtractSummaryResults;

                foreach (ExtractSummaryActionResult summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)
                    {
                        WriteLine($"  Error!");
                        WriteLine(
                            $"  Action error code: {summaryActionResults.Error.ErrorCode}."
                        );
                        WriteLine($"  Message: {summaryActionResults.Error.Message}");
                        continue;
                    }

                    foreach (
                        ExtractSummaryResult documentResults in summaryActionResults.DocumentsResults
                    )
                    {
                        if (documentResults.HasError)
                        {
                            WriteLine($"  Error!");
                            WriteLine(
                                $"  Document error code: {documentResults.Error.ErrorCode}."
                            );
                            WriteLine($"  Message: {documentResults.Error.Message}");
                            continue;
                        }

                        WriteLine(
                            $"  Extracted the following {documentResults.Sentences.Count} sentence(s):"
                        );
                        WriteLine();

                        foreach (SummarySentence sentence in documentResults.Sentences)
                        {
                            WriteLine($"  Sentence: {sentence.Text}");
                            WriteLine();
                        }
                    }
                }
            }
        }

        private static async Task RunTheTest(OpenAIClient client)
        {
            var completion = new CompletionsOptions();
            completion.Prompts.Add("You're such a wanker!");
            completion.ChoicesPerPrompt = 10;
            completion.MaxTokens = 10;
            completion.User = "DGMJR";


            var result = await client.GetCompletionsAsync("GPT-35-turbo", completion);

            foreach (var choicee in result.Value.Choices)
            {
                Console.WriteLine(choicee.Text);
            }
        }

        static async Task Main(string[] args)
        {
            var client = new OpenAIClient(endpoint, credentials);
            await RunTheTest(client);
        }
    }
}
